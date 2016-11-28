using UnityEditor;
using UnityEngine;

namespace Utils.Signals.Editor {
  [CustomPropertyDrawer(typeof(SignalRefAttribute))]
  public class SignalRefDrawer : PropertyDrawer {
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
      if (label.text == "Id") {
        label.text = "Signal";
      }

      property.intValue = EditorGUI.IntPopup(position, label, property.intValue, Signal.NameLabels, Signal.Ids);
    }
  }
}