using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Utils.Editor {
  [CustomPropertyDrawer(typeof(AutoPopulateAttribute), true)]
  public class AutoPopulateDrawer : PropertyDrawer {
    private GUIContent m_MakeEditableLabel;
    private GUIContent m_MakeReadOnlyLabel;
    private GUIContent m_ForceRepopulateLabel;

    private Texture m_FindSelfIcon;
    private Texture m_FindParentsIcon;
    private Texture m_FindChildrenIcon;

    private Texture m_MissingSelfIcon;
    private Texture m_MissingParentsIcon;
    private Texture m_MissingChildrenIcon;

    private Texture m_ErrorIcon;

    private static Dictionary<string, bool> s_Editable;

    private GUIContent MakeReadonlyLabel {
      get {
        if (m_MakeReadOnlyLabel == null) {
          m_MakeReadOnlyLabel = new GUIContent("Make read only");
        }
        return m_MakeReadOnlyLabel;
      }
    }

    private GUIContent MakeEditableLabel {
      get {
        if (m_MakeEditableLabel == null) {
          m_MakeEditableLabel = new GUIContent("Make editable");
        }
        return m_MakeEditableLabel;
      }
    }

    public GUIContent ForceRepopulateLabel {
      get {
        if (m_ForceRepopulateLabel == null) {
          m_ForceRepopulateLabel = new GUIContent("Force repopulate");
        }
        return m_ForceRepopulateLabel;
      }
    }

    private Texture FindSelfIcon {
      get {
        return GetEditorIcon(ref m_FindSelfIcon, "AutoPopulateFindSelf13.png");
      }
    }

    private Texture FindParentsIcon {
      get {
        return GetEditorIcon(ref m_FindParentsIcon, "AutoPopulateFindParents13.png");
      }
    }

    private Texture FindChildrenIcon {
      get {
        return GetEditorIcon(ref m_FindChildrenIcon, "AutoPopulateFindChildren13.png");
      }
    }

    private Texture MissingSelfIcon {
      get {
        return GetEditorIcon(ref m_MissingSelfIcon, "AutoPopulateMissingSelf13.png");
      }
    }

    private Texture MissingParentsIcon {
      get {
        return GetEditorIcon(ref m_MissingParentsIcon, "AutoPopulateMissingParents13.png");
      }
    }

    private Texture MissingChildrenIcon {
      get {
        return GetEditorIcon(ref m_MissingChildrenIcon, "AutoPopulateMissingChildren13.png");
      }
    }

    private Texture ErrorIcon {
      get {
        return GetEditorIcon(ref m_ErrorIcon, "AutoPopulateError13.png");
      }
    }

    private static Texture GetEditorIcon(ref Texture variable, string iconName) {
      if (variable == null) {
        variable = AssetDatabaseUtils.LoadEditorIconRelativeToScript(iconName, typeof(AutoPopulateDrawer));
      }
      return variable;
    }

    private static string GetInstanceKey(SerializedProperty property) {
      Object targetObject = property.serializedObject.targetObject;
      int instanceId = 0;
      if (targetObject != null) {
        instanceId = targetObject.GetInstanceID();
      }
      string key = string.Format("{0}:{1}", instanceId, property.propertyPath);
      return key;
    }

    private bool GetEditableInstance(SerializedProperty property) {
      bool editable = false;

      if (s_Editable != null) {
        string key = GetInstanceKey(property);
        s_Editable.TryGetValue(key, out editable);
      }
      return editable;
    }

    private void SetEditableInstance(SerializedProperty property, bool editable) {
      if (s_Editable == null) {
        s_Editable = new Dictionary<string, bool>();
      }
      string key = GetInstanceKey(property);
      s_Editable[key] = editable;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
      AutoPopulateAttribute autoPopulate = attribute as AutoPopulateAttribute;
      if (CanAutoPopulate(property, autoPopulate)) {
        bool wasEnabled = GUI.enabled;
        GUI.enabled = GetEditable(property, autoPopulate);

        bool populated = false;

        if (AutoPopulateUtils.NeedsPopulating(property, fieldInfo.FieldType)) {
          populated = AutoPopulateUtils.PopulateValue(fieldInfo.FieldType, property, autoPopulate);
        } else {
          populated = true;
        }

        GUIContent tweakedLabel = new GUIContent(label.text, GetIcon(autoPopulate, populated), GetTooltip(property, autoPopulate));

        if (AutoPopulateUtils.IsInterfaceReference(property, fieldInfo.FieldType)) {
          InterfaceReferenceDrawer.PropertyField(position, property, fieldInfo, tweakedLabel);
        } else {
          EditorGUI.PropertyField(position, property, tweakedLabel);
        }

        GUI.enabled = wasEnabled;
        HandlePopup(position, property, autoPopulate);
      } else {
        GUIContent tweakedLabel = new GUIContent(label.text, ErrorIcon, GetErrorTooltip());
        EditorGUI.PropertyField(position, property, tweakedLabel);
      }
    }

    private bool CanAutoPopulate(SerializedProperty property, AutoPopulateAttribute autoPopulate) {
      if (autoPopulate == null) {
        return false;
      }
      if (property.isArray) {
        return false; // Disable array support for now, 
        // Unity doesn't call the drawer for the array, it calls it for the elements in the array :(
      }
      if (property.propertyPath.EndsWith("]")) {
        return false; // as above, we use the ] char as a sign that Unity is trying to edit an array element
      }

      if (property.propertyType == SerializedPropertyType.ObjectReference) {
        return true;
      }

      if (AutoPopulateUtils.IsInterfaceReference(property, fieldInfo.FieldType)) {
        return true;
      }

      return false;
    }

    private bool GetEditable(SerializedProperty property, AutoPopulateAttribute autoPopulate) {
      bool editable = true;
      switch (autoPopulate.Editable) {
        case AutoPopulateAttribute.EditBehaviour.ReadOnlyUntilUserAction:
          editable = GetEditableInstance(property);
          break;
        case AutoPopulateAttribute.EditBehaviour.Never:
          editable = false;
          break;
        case AutoPopulateAttribute.EditBehaviour.Always:
          editable = true;
          break;
        default:
          throw new ArgumentOutOfRangeException();
      }
      return editable;
    }

    private void HandlePopup(Rect position, SerializedProperty property, AutoPopulateAttribute autoPopulate) {
      Event e = Event.current;
      if ((e.type == EventType.MouseUp) && (e.button == 1)) {
        if (position.Contains(e.mousePosition)) {
          bool editable = false;
          bool canChangeEditable = false;

          switch (autoPopulate.Editable) {
            case AutoPopulateAttribute.EditBehaviour.ReadOnlyUntilUserAction:
              editable = GetEditable(property, autoPopulate);
              canChangeEditable = true;
              break;
            case AutoPopulateAttribute.EditBehaviour.Never:
              editable = false;
              canChangeEditable = false;
              break;
            case AutoPopulateAttribute.EditBehaviour.Always:
              editable = true;
              canChangeEditable = false;
              break;
            default:
              throw new ArgumentOutOfRangeException();
          }

          GUI.FocusControl(null);
          GenericMenu menu = new GenericMenu();

          if (editable) {
            menu.AddItem(MakeReadonlyLabel, false, canChangeEditable ? MakeReadonly : (GenericMenu.MenuFunction2) null, property);
          } else {
            menu.AddItem(MakeEditableLabel, false, canChangeEditable ? MakeEditable : (GenericMenu.MenuFunction2) null, property);
          }
          menu.AddItem(ForceRepopulateLabel, false, ForceRepopulate, property);
          menu.ShowAsContext();
          e.Use();
        }
      }
    }

    private void MakeReadonly(object data) {
      SerializedProperty property = data as SerializedProperty;
      if (property != null) {
        SetEditableInstance(property, false);
      }
    }

    private void MakeEditable(object data) {
      SerializedProperty property = data as SerializedProperty;
      if (property != null) {
        SetEditableInstance(property, true);
      }
    }

    private void ForceRepopulate(object data) {
      SerializedProperty property = data as SerializedProperty;

      AutoPopulateAttribute autoPopulate = attribute as AutoPopulateAttribute;
      if ((property != null) && (autoPopulate != null)) {
        property.serializedObject.Update();
        AutoPopulateUtils.PopulateValue(fieldInfo.FieldType, property, autoPopulate);
        property.serializedObject.ApplyModifiedProperties();
      }
    }

    private Texture GetIcon(AutoPopulateAttribute autoPopulate, bool populated) {
      Texture foundIcon = null;
      Texture missingIcon = null;
      switch (autoPopulate.Find) {
        case AutoPopulateAttribute.FindBehaviour.Self:
          foundIcon = FindSelfIcon;
          missingIcon = MissingSelfIcon;
          break;
        case AutoPopulateAttribute.FindBehaviour.Children:
          foundIcon = FindChildrenIcon;
          missingIcon = MissingChildrenIcon;
          break;
        case AutoPopulateAttribute.FindBehaviour.Parent:
          foundIcon = FindParentsIcon;
          missingIcon = MissingParentsIcon;
          break;
        default:
          throw new ArgumentOutOfRangeException();
      }
      if (populated) {
        return foundIcon;
      }
      return missingIcon;
    }

    private string GetTooltip(SerializedProperty property, AutoPopulateAttribute autoPopulate) {
      Type searchType;
      if (property.isArray) {
        searchType = AutoPopulateUtils.GetSearchType(fieldInfo.FieldType.GetElementType(), autoPopulate);
      } else {
        searchType = AutoPopulateUtils.GetSearchType(fieldInfo.FieldType, autoPopulate);
      }
      string relativeTo = "";
      if (autoPopulate.FindRelativeTo != null) {
        string componentName = autoPopulate.FindRelativeTo.Name;
        string componentArticle;
        if (IsVowel(componentName[0])) {
          componentArticle = "an";
        } else {
          componentArticle = "a";
        }

        relativeTo = string.Format(" on the first parent with {0} {1} component", componentArticle, autoPopulate.FindRelativeTo.Name);
      }
      switch (autoPopulate.Find) {
        case AutoPopulateAttribute.FindBehaviour.Self:
          return string.Format("Auto-populated by calling gameObject.GetComponent({0}){1}", searchType.Name, relativeTo);
        case AutoPopulateAttribute.FindBehaviour.Children:
          return string.Format("Auto-populated by calling gameObject.GetComponentInChildren({0}){1}", searchType.Name, relativeTo);
        case AutoPopulateAttribute.FindBehaviour.Parent:
          return string.Format("Auto-populated by calling gameObject.GetComponentInParent({0}){1}", searchType.Name, relativeTo);
        default:
          throw new ArgumentOutOfRangeException();
      }
    }

    private bool IsVowel(char c) {
      const string vowels = "aeiou";
      return vowels.IndexOf(char.ToLower(c)) >= 0;
    }

    private string GetErrorTooltip() {
      return string.Format("Cannot auto-populate fileds of type {0}", fieldInfo.FieldType.Name);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
      return base.GetPropertyHeight(property, label);
    }
  }
}