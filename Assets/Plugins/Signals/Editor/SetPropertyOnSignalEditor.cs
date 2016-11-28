using UnityEditor;
using UnityEngine;

namespace Utils.Signals.Editor {
  [CustomEditor(typeof(SetPropertyOnSignal), true)]
  public class SetPropertyOnSignalEditor : UnityEditor.Editor {
    private SetPropertyOnSignal Target {
      get {
        return m_Target ? m_Target : (m_Target = (SetPropertyOnSignal) target);
      }
    }
    private SetPropertyOnSignal m_Target;

    private Component m_LastComponent;

    public override void OnInspectorGUI() {
      base.OnInspectorGUI();

      if (Target && (m_LastComponent != Target.PropertyComponent)) {
        m_LastComponent = Target.PropertyComponent;
        Target.Init();
      }

      if (Target.PropertyNameList.Length > 0) {
        Target.PropertyIndex = EditorGUILayout.Popup("Set", Target.PropertyIndex, Target.PropertyNameList);
      } else {
        EditorGUILayout.LabelField("No Propertys found" + (Target.PropertyComponent == null ? "." : " in " + Target.PropertyComponent.GetType().Name + "."));
      }
    }
  }
}