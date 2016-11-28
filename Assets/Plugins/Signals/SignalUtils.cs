using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using JetBrains.Annotations;
using UnityEngine;

namespace Utils.Signals {
  public static class SignalUtils {
    public static T GetOrAddComponent<T>(this Component component) where T : Component {
      return component.gameObject.GetOrAddComponent<T>();
    }

    public static T GetOrAddComponent<T>(this GameObject go) where T : Component {
      T result = go.GetComponent<T>();
      return result ? result : go.AddComponent<T>();
    }

    public static string AggregateString<T>(this IEnumerable<T> ie, string sep = ",", Func<T, string> xfrm = null, string prepend = "", string append = "") {
      if (xfrm == null) {
        xfrm = v => v.ToString();
      }
      return ie.Aggregate(new StringBuilder(prepend), (c, n) => c.Length > prepend.Length ? c.Append(sep).Append(xfrm(n)) : c.Append(xfrm(n))).Append(append).ToString();
    }

    private static readonly Dictionary<Type, Dictionary<string, object>> s_ConstantsDictionaryCache = new Dictionary<Type, Dictionary<string, object>>();

    public static Dictionary<string, object> GetConstantsDictionary(Type type) {
      if (s_ConstantsDictionaryCache.ContainsKey(type)) {
        return s_ConstantsDictionaryCache[type];
      }

      Dictionary<string, object> contantsDictionary = new Dictionary<string, object>();

      foreach (FieldInfo f in type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)) {
        if (!f.IsLiteral || f.IsInitOnly) {
          continue;
        }

        int val = (int) f.GetRawConstantValue();
        contantsDictionary[f.Name] = val;
      }

      return s_ConstantsDictionaryCache[type] = contantsDictionary;
    }

    public static string GetScenePath(this Component o) {
      string path = o.name + ":" + o.GetType().Name;
      Transform parent = o.transform.parent;

      while (parent) {
        path = parent.name + "/" + path;
        parent = parent.parent;
      }

      return path;
    }

    public static int IndexOf<T>(this IEnumerable<T> items, Func<T, bool> predicate) {
      if (items == null) {
        throw new ArgumentNullException("items");
      }

      if (predicate == null) {
        throw new ArgumentNullException("predicate");
      }

      int retVal = 0;
      foreach (T item in items) {
        if (predicate(item)) {
          return retVal;
        }
        retVal++;
      }
      return -1;
    }

    public static int IndexOf<T>(this IEnumerable<T> items, T item) {
      return items.IndexOf(i => EqualityComparer<T>.Default.Equals(item, i));
    }

    public static List<Transform> GetChildren([NotNull] this Transform t) {
      if (t == null) {
        throw new ArgumentNullException("t");
      }
      List<Transform> children = new List<Transform>();
      for (int i = 0; i < t.childCount; i++) {
        children.Add(t.GetChild(i));
      }
      return children;
    }

    [ContractAnnotation("source:null => true")]
    public static bool IsNullOrEmpty(this string source) {
      return string.IsNullOrEmpty(source);
    }

    public static Delegate AddEventAction(this EventInfo eventInfo, object client, Action callback) {
      if (eventInfo == null) {
        throw new ArgumentNullException("eventInfo");
      }

      if (callback == null) {
        throw new ArgumentNullException("callback");
      }

      if (client == null) {
        throw new ArgumentNullException("client");
      }

      ConstructorInfo constructor = eventInfo.EventHandlerType.GetConstructor(new[] {typeof(object), typeof(IntPtr)});
      EventHandler handler = (sender, args) => callback();

      if (constructor == null) {
        return null;
      }

      Delegate eventInfoHandler = (Delegate) constructor.Invoke(new[] {handler.Target, handler.Method.MethodHandle.GetFunctionPointer()});
      eventInfo.AddEventHandler(client, eventInfoHandler);
      return eventInfoHandler;
    }
  }
}