using UnityEditor;
using UnityEngine;

namespace Utils.Signals.Editor {
  [CustomEditor(typeof(SendSignalOnEvent), true)]
  public class SendSignalOnEventEditor : UnityEditor.Editor {
    private SendSignalOnEvent Event {
      get {
        return m_Event ? m_Event : (m_Event = (SendSignalOnEvent) target);
      }
    }
    private SendSignalOnEvent m_Event;

    private Component m_LastComponent;

    public override void OnInspectorGUI() {
      base.OnInspectorGUI();

      if (Event && (m_LastComponent != Event.EventComponent)) {
        m_LastComponent = Event.EventComponent;
        Event.UpdateEvents();
      }

      if (Event.EventNameList.Length > 0) {
        Event.EventIndex = EditorGUILayout.Popup("On", Event.EventIndex, Event.EventNameList);
      } else {
        EditorGUILayout.LabelField("No events found" + (Event.EventComponent == null ? "." : " in " + Event.EventComponent.GetType().Name + "."));
      }
    }
  }
}