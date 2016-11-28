using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Utils.Editor {
	public static class AutoPopulateUtils {

		private class XDebug : global::XDebug.Channel<XDebug> {
			public XDebug() : base("AutoPopulate", Colors.Extended.LightCyan) {}
		}

		[MenuItem("Tools/AutoPopulate/Refresh",priority = 626)] // magic number corresponds to entry in ToolsMenuPriority, but that is in the game assembly :(
		public static void RefreshMenu() {
			RepopulateSelection(false);
		}

		[MenuItem("Tools/AutoPopulate/Force Repopulate", priority = 627)]
		public static void RepopulateMenu() {
			if (EditorUtility.DisplayDialog(
				"Force Repopulate",
				"Force Repopulate will overwrite the values of ALL AutoPopulated fields",
				"DO IT!","Cancel")) {
				RepopulateSelection(true);
			}
		}

		private static void RepopulateSelection(bool forceRepopulate) {
			List<Object> objects = new List<Object>();
			ErrorReport report = new ErrorReport();
			try {
				if (EditorUtility.DisplayCancelableProgressBar("Repopulating", "Searching...", 0)) {
					report.Error("User canceled");
				} else {
					foreach (Object o in Selection.objects) {
						GameObject go = o as GameObject;
						if (go != null) {
							if (!objects.Contains(go)) {
								objects.Add(go);
								List<GameObject> children = new List<GameObject>();
								GetChildrenRecursive(go, children);
								foreach (GameObject child in children) {
									if (!objects.Contains(child)) {
										objects.Add(child);
									}
								}
							}
						} else {
							Component component = o as Component;
							if (component != null) {
								objects.Add(component);
								AutoPopulate(component, forceRepopulate, report);
							}
						}
					}

					for (int i = 0; i < objects.Count; i++) {
						Object o = objects[i];
						if (EditorUtility.DisplayCancelableProgressBar(
							"Repopulating",
							o.FullName(Extensions.FullNameFlags.FullSceneOrAssetPath),
							(float)i/objects.Count)) {
							report.Error("User canceled");
							break;
						}
						GameObject go = o as GameObject;
						if (go != null) {
							// use non-recursive, because we already unrolled all the child objects when we were searching above
							AutoPopulate(go, false, forceRepopulate, report);
						} else {
							Component component = o as Component;
							if (component != null) {
								AutoPopulate(component, forceRepopulate, report);
							}
						}
					}
				}
			} catch (Exception e) {
				report.HandleException(e);
			} finally {
				if (objects.Count == 0) {
					report.Warning("Nothing selected was Auto-populatable");
				}
				report.Done();
				EditorUtility.ClearProgressBar();
			}
		}

		private static void GetChildrenRecursive(GameObject go, List<GameObject> children) {
			foreach (Transform child in go.transform) {
				children.Add(child.gameObject);
				GetChildrenRecursive(child.gameObject, children);
			}
		}


		public static void AutoPopulate(UnityEngine.GameObject go,bool recursive, bool forceRepopulate = false,IErrorReport report=null) {
			Component[] components = go.GetComponentsInChildren<Component>();
			foreach (Component component in components) {
				AutoPopulate(component, forceRepopulate, report);
			}
			if (recursive) {
				foreach (Transform child in go.transform) {
					AutoPopulate(child.gameObject,recursive,forceRepopulate,report);
				}
			}
		}

		public static void AutoPopulate(UnityEngine.Component component,bool forceRepopulate= false, IErrorReport report = null) {
			SerializedObject so = new SerializedObject(component);
			so.Update();

			AutoPopulate(so, forceRepopulate, report);

			so.ApplyModifiedProperties();
		}

		public static void AutoPopulate(SerializedObject so, bool forceRepopulate = false, IErrorReport report = null) {
			SerializedProperty prop = so.GetIterator();
			if (prop.hasChildren) {
				while (prop.Next(true)) {
					FieldInfo fieldInfo = PropertyUtils.GetFieldInfoFromProperty(prop);
					if (fieldInfo != null) {
						AutoPopulateAttribute attribute = GetAutoPopulateAttribute(fieldInfo);
						if (attribute != null) {
							bool needsPopulating = NeedsPopulating(prop, fieldInfo.FieldType);
							if (forceRepopulate || needsPopulating) {
								if (report != null) {
									report.Log(so.targetObject,"Repopulating {0}.{1}",so.targetObject.FullName(),prop.propertyPath);
								}
								PopulateValue(fieldInfo.FieldType, prop, attribute);
							} else {
								if (report != null) {
									report.Log(so.targetObject, "Skipping {0}.{1}", so.targetObject.FullName(), prop.propertyPath);
								}
							}
						}
					}
				}
			}
		}

		private static AutoPopulateAttribute GetAutoPopulateAttribute(FieldInfo fieldInfo) {
			object[] attribs = fieldInfo.GetCustomAttributes(typeof(AutoPopulateAttribute),true);
			if (attribs != null && attribs.Length > 0) {
				AutoPopulateAttribute attribute = attribs[0] as AutoPopulateAttribute;
				return attribute;
			}
			return null;
		}

		public  static bool NeedsPopulating(SerializedProperty property,Type fieldType) {
			if (!property.hasMultipleDifferentValues) {
				if (property.isArray) {
					return property.arraySize == 0;
				}
				if (property.propertyType == SerializedPropertyType.ObjectReference) {
					return property.objectReferenceValue == null;
				}
				if (IsInterfaceReference(property, fieldType)) {
					return InterfaceReferenceDrawer.GetReferenceValue(property) == null;
				}
			}
			return false;
		}

		public static bool PopulateValue(Type fieldType, SerializedProperty property, AutoPopulateAttribute autoPopulate) {
			bool populated = false;
			if (!property.hasMultipleDifferentValues) {
				if (property.isArray) {
					populated = PopulateValueArray(fieldType, property, autoPopulate);
				} else if (property.propertyType == SerializedPropertyType.ObjectReference || IsInterfaceReference(property, fieldType)) {
					populated = PopulateValueSingle(fieldType, property, autoPopulate);
				}
			}
			return populated;
		}

		private static bool PopulateValueArray(Type fieldType, SerializedProperty property, AutoPopulateAttribute autoPopulate) {
			Object[] values = null;
			Type elementType = fieldType.GetElementType();
			Type searchType = GetSearchType(elementType, autoPopulate);

			foreach (Object targetObject in property.serializedObject.targetObjects) {
				MonoBehaviour behaviour = targetObject as MonoBehaviour;

				if (behaviour == null) {
					XDebug.LogWarning(targetObject, "Couldn't find target behaviour on {0}.", targetObject);
					continue;
				}

				SerializedObject singleObject = new SerializedObject(targetObject);
				SerializedProperty singleProperty = singleObject.FindProperty(property.propertyPath);

				if (singleProperty == null) {
					XDebug.LogWarning(behaviour, "Couldn't find {0} property for {1}", property.propertyPath, behaviour);
					continue;
				}

				Transform root = FindRoot(behaviour.gameObject, autoPopulate.FindRelativeTo);

				if (root == null) {
					XDebug.LogWarning(behaviour, "Couldn't find root object for {0}.", behaviour.gameObject);
					continue;
				}

				switch (autoPopulate.Find) {
					case AutoPopulateAttribute.FindBehaviour.Self:
						values = FindObjectsInSelf(root, searchType);
						break;
					case AutoPopulateAttribute.FindBehaviour.Children:
						values = FindObjectsInChildren(root, searchType);
						break;
					case AutoPopulateAttribute.FindBehaviour.Parent:
						values = FindObjectsInParent(root, searchType);
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}

				singleProperty.arraySize = values.Length;
				for (int i = 0; i < values.Length; i++) {
					SerializedProperty element = singleProperty.GetArrayElementAtIndex(i);
					SetValue(element, elementType, values[i]);
					element.objectReferenceValue = values[i];
				}
				singleObject.ApplyModifiedProperties();
			}

			if (values != null && values.Length > 0) {
				property.serializedObject.SetIsDifferentCacheDirty();
			}

			return values != null && values.Length > 0;
		}

		private static bool PopulateValueSingle(Type fieldType, SerializedProperty property, AutoPopulateAttribute autoPopulate) {
			Object value = null;
			Type searchType = GetSearchType(fieldType, autoPopulate);

			foreach (Object targetObject in property.serializedObject.targetObjects) {
				MonoBehaviour behaviour = targetObject as MonoBehaviour;

				if (behaviour == null) {
					XDebug.LogWarning(targetObject, "Couldn't find target behaviour on {0}.", targetObject);
					continue;
				}

				SerializedObject singleObject = new SerializedObject(targetObject);
				SerializedProperty singleProperty = singleObject.FindProperty(property.propertyPath);

				if (singleProperty == null) {
					XDebug.LogWarning(behaviour, "Couldn't find {0} property for {1}", property.propertyPath, behaviour);
					continue;
				}

				Transform root = FindRoot(behaviour.gameObject, autoPopulate.FindRelativeTo);

				if (root == null) {
					continue;
				}

				switch (autoPopulate.Find) {
					case AutoPopulateAttribute.FindBehaviour.Self:
						value = FindObjectInSelf(root, searchType);
						break;
					case AutoPopulateAttribute.FindBehaviour.Children:
						value = FindObjectInChildren(root, searchType);
						break;
					case AutoPopulateAttribute.FindBehaviour.Parent:
						value = FindObjectInParent(root, searchType);
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}

				SetValue(singleProperty, fieldType, value);
				singleObject.ApplyModifiedProperties();
			}

			if (value != null) {
				property.serializedObject.SetIsDifferentCacheDirty();
			}

			return value != null;
		}

		private static void SetValue(SerializedProperty property,Type fieldType, Object value) {
			if (InterfaceReferenceDrawer.GetInterfaceType(fieldType) != null) {
				InterfaceReferenceDrawer.SetReferenceValue(property, value);
			} else {
				property.objectReferenceValue = value;
			}
		}

		public static bool IsInterfaceReference(SerializedProperty property, Type fieldType) {
			if (property.propertyType == SerializedPropertyType.Generic) {
				if (fieldType != null) {
					return InterfaceReferenceDrawer.GetInterfaceType(fieldType) != null;
				}
			}

			return false;
		}

		private static Transform FindRoot(GameObject gameObject, Type findRelativeTo) {
			// default to searching locally if we don't specify findRelativeTo
			if (findRelativeTo == null) {
				return gameObject.transform;
			}

			// search UP the hierarchy looking for a compontent of type "findRelativeTo" to 
			Transform potentialRoot = gameObject.transform;
			while (potentialRoot != null) {
				if (potentialRoot.GetComponent(findRelativeTo) != null) {
					// Found our root component
					return potentialRoot;
				}
				potentialRoot = potentialRoot.parent;
			}
			// failed to find our root component
			return null;
		}

		// We implement the search functions ourselves, because the built in GetComponentInChildren() ones won't search for components on disabled objects
		private static Object FindObjectInSelf(Transform root, Type searchType) {
			if (searchType == typeof(GameObject)) {
				// This is kind of dumb, but we might as well support it, because someone already tried it.... hence why I'm writing this.
				return root.gameObject;
			} else {
				return root.GetComponent(searchType);
			}
		}

		private static Object FindObjectInChildren(Transform root, Type searchType) {
			Queue<Transform> queue = new Queue<Transform>();
			queue.Enqueue(root);
			while (queue.Count > 0) {
				Transform t = queue.Dequeue();
				Object obj = FindObjectInSelf(t, searchType);
				if (obj != null) {
					return obj;
				}
				foreach (Transform child in t) {
					queue.Enqueue(child);
				}
			}
			return null;
		}

		private static Object FindObjectInParent(Transform root, Type searchType) {
			Transform t = root;
			while (t != null) {
				Object obj = FindObjectInSelf(t, searchType);
				if (obj != null) {
					return obj;
				}
				t = t.parent;
			}
			return null;
		}


		// We implement the search functions ourselves, because rthe built in ones won't search for components on disabled objects
		private static Object[] FindObjectsInSelf(Transform root, Type searchType) {
			if (searchType == typeof(GameObject)) {
				// This is kind of dumb, but we might as well support it, because someone already tried it.... hence why I'm writing this.
				return new Object[] {root.gameObject};
			} else if (typeof(Component).IsAssignableFrom(searchType)) {
				return root.GetComponents(searchType);
			} else {
				return new Object[0];
			}
		}

		private static Object[] FindObjectsInChildren(Transform root, Type searchType) {
			// Should this be breadth or Depth first?  
			// Currently it is breadth, which may be the wrong choice.
			List<Object> results = new List<Object>();
			Queue<Transform> queue = new Queue<Transform>();
			queue.Enqueue(root);
			while (queue.Count > 0) {
				Transform t = queue.Dequeue();
				results.AddRange(FindObjectsInSelf(t, searchType));
				foreach (Transform child in t) {
					queue.Enqueue(child);
				}
			}
			return results.ToArray();
		}

		private static Object[] FindObjectsInParent(Transform root, Type searchType) {
			List<Object> results = new List<Object>();
			Transform t = root;
			while (t != null) {
				results.AddRange(FindObjectsInSelf(t, searchType));
				t = t.parent;
			}
			return results.ToArray();
		}




		public static Type GetSearchType(Type fieldType, AutoPopulateAttribute autoPopulate) {
			Type searchType = fieldType;

			Type interfaceType = InterfaceReferenceDrawer.GetInterfaceType(searchType);
			if (interfaceType != null) {
				searchType = interfaceType;
			}

			if (autoPopulate.TypeOverride != null) {
				searchType = autoPopulate.TypeOverride;
			}
			return searchType;
		}
	}
}
