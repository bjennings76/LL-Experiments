using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Utils.Signals {
  public static partial class Signal {
    private static bool s_Initialized;
    private static readonly Dictionary<string, int> s_NamesToIds = new Dictionary<string, int>();
    private static readonly Dictionary<int, string> s_IdsToNames = new Dictionary<int, string>();

    public static int[] Ids {
      get {
        if (s_Ids == null) {
          Initialize();
        }
        return s_Ids;
      }
    }
    private static int[] s_Ids;

    public static string[] Names {
      get {
        if (s_Names == null) {
          Initialize();
        }
        return s_Names;
      }
    }
    private static string[] s_Names;

    public static GUIContent[] NameLabels {
      get {
        return s_NameLabels != null ? s_NameLabels : (s_NameLabels = Names.Select(n => new GUIContent(n)).ToArray());
      }
    }
    private static GUIContent[] s_NameLabels;

    public static string LookupName(int id) {
      Initialize();
      return s_IdsToNames[id];
    }

    public static int LookupId(string name) {
      Initialize();
      if (s_NamesToIds.ContainsKey(name)) {
        return s_NamesToIds[name];
      }

      XDebug.LogError("Couldn't find signal named '" + name + "'.");
      return None;
    }

    private static void Initialize() {
      if (s_Initialized) {
        return;
      }

      Dictionary<string, object> consts = SignalUtils.GetConstantsDictionary(typeof(Signal));
      List<int> ids = new List<int>();
      List<string> names = new List<string>();

      foreach (KeyValuePair<string, object> kv in consts) {
        string name = kv.Key;
        int val = (int) kv.Value;
        s_NamesToIds.Add(name, val);
        s_IdsToNames.Add(val, name);
        ids.Add(val);
        names.Add(name);
      }

      ids.Sort();
      names.Sort((na, nb) => s_NamesToIds[na].CompareTo(s_NamesToIds[nb]));

      s_Ids = ids.ToArray();
      s_Names = names.ToArray();
      s_NameLabels = null;
      s_Initialized = true;
    }
  }

  public class SignalRefAttribute : PropertyAttribute {}
}