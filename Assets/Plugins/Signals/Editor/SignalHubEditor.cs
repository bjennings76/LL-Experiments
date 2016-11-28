using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Utils.Signals.Editor {
  [CustomEditor(typeof(SignalHub), true)]
  public class SignalHubEditor : UnityEditor.Editor {
    private readonly Dictionary<string, bool> m_SignalGroups = new Dictionary<string, bool>();

    private readonly Dictionary<int, List<ISignalSender>> m_SenderLookup = new Dictionary<int, List<ISignalSender>>();

    private readonly Dictionary<string, List<SignalUserEntry>> m_MergedLookup = new Dictionary<string, List<SignalUserEntry>>();

    private List<string> m_MergedLookupOrderedKeys;

    private SignalHub Hub {
      get {
        return m_Hub ? m_Hub : (m_Hub = (SignalHub) target);
      }
    }
    private SignalHub m_Hub;

    private SignalHub ParentHub { get; set; }

    protected void OnEnable() {
      UpdateSignalUsers();
      EditorApplication.hierarchyWindowChanged += UpdateSignalUsers;
    }

    protected void OnDisable() {
      EditorApplication.hierarchyWindowChanged -= UpdateSignalUsers;
    }

    public override void OnInspectorGUI() {
      base.OnInspectorGUI();

      GUI.enabled = false;
      foreach (string group in m_MergedLookupOrderedKeys) {
        string lastType = null;

        foreach (SignalUserEntry entry in m_MergedLookup[group]) {
          if (!entry.User.Component) {
            continue;
          }

          int id = entry.User.Id;
          if (!m_SignalGroups.ContainsKey(group)) {
            m_SignalGroups[group] = true;
          }
          if (lastType != entry.Type.ToString()) {
            lastType = entry.Type.ToString();
            GUI.enabled = true;
            m_SignalGroups[group] = EditorGUI.Foldout(EditorGUILayout.GetControlRect(), m_SignalGroups[group], string.Format("{0} {1} ({2})", Signal.LookupName(id), entry.Type, m_MergedLookup[group].Count), true);
            GUI.enabled = false;
          }
          if (m_SignalGroups[group]) {
            OnSignalUserGUI(entry);
          }
        }
      }

      if (!ParentHub) {
        EditorGUILayout.TextField("My Hub", "GlobalHub");
      } else {
        EditorGUILayout.ObjectField("My Hub", ParentHub, typeof(SignalHub), true);
      }

      GUI.enabled = true;
    }

    private static void OnSignalUserGUI(SignalUserEntry entry) {
      Component r = entry.User.Component;
      string originalName = r.name;
      // HACK: Ugly workaround for the ObjectField not showing the object type. (BDJ)
      r.name = string.Format("{0} ({1})", r.name, r.GetType().Name);
      EditorGUILayout.ObjectField(r, typeof(Component), true);
      r.name = originalName;
    }

    private void UpdateSignalUsers() {
      if (!Hub) {
        return;
      }

      if (!EditorApplication.isPlaying) {
        Hub.FindReceivers();
      }

      FindSenders();
      MergeSignalUsers();
      ParentHub = SignalManager.FindParentHub(Hub);
      Repaint();
    }

    private void FindSenders() {
      List<AbstractSignalSender> senders = FindChildSenders(Hub.transform);

      foreach (AbstractSignalSender sender in senders) {
        if (!m_SenderLookup.ContainsKey(sender.Id)) {
          m_SenderLookup[sender.Id] = new List<ISignalSender>();
        }

        if (!m_SenderLookup[sender.Id].Contains(sender)) {
          m_SenderLookup[sender.Id].Add(sender);
        }
      }
    }

    private void MergeSignalUsers() {
      m_MergedLookup.Clear();

      // Merge Senders
      foreach (KeyValuePair<int, List<ISignalSender>> kvp in m_SenderLookup) {
        string group = string.Format("{0}_{1}", kvp.Key, (int) SignalUserType.Sender);
        if (!m_MergedLookup.ContainsKey(group)) {
          m_MergedLookup[group] = new List<SignalUserEntry>();
        }
        m_MergedLookup[group].AddRange(kvp.Value.Select(s => new SignalUserEntry(SignalUserType.Sender, s)));
      }

      // Merge Receivers
      foreach (KeyValuePair<int, List<ISignalReceiver>> kvp in Hub.ReceiverLookup) {
        string group = string.Format("{0}_{1}", kvp.Key, (int) SignalUserType.Receiver);
        if (!m_MergedLookup.ContainsKey(group)) {
          m_MergedLookup[group] = new List<SignalUserEntry>();
        }
        m_MergedLookup[group].AddRange(kvp.Value.Select(r => new SignalUserEntry(SignalUserType.Receiver, r)));
      }

      m_MergedLookupOrderedKeys = m_MergedLookup.Keys.OrderBy(k => k).ToList();
    }

    private static List<AbstractSignalSender> FindChildSenders(Transform t) {
      List<AbstractSignalSender> list = t.GetComponents<AbstractSignalSender>().ToList();

      foreach (Transform child in t.GetChildren()) {
        if (!child.GetComponent<SignalHub>()) {
          list.AddRange(FindChildSenders(child));
        }
      }

      return list;
    }

    private class SignalUserEntry {
      public SignalUserType Type { get; private set; }
      public ISignalUser User { get; private set; }

      public SignalUserEntry(SignalUserType type, ISignalUser user) {
        Type = type;
        User = user;
      }
    }

    private enum SignalUserType {
      Sender,
      Receiver
    }
  }
}