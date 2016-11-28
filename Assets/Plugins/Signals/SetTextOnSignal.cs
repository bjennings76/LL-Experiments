using UnityEngine;
using UnityEngine.UI;

namespace Utils.Signals {
  public class SetTextOnSignal : AbstractSignalReceiver {
    [SerializeField, AutoPopulate] private Text m_Text;

    public override void Receive(Component sender, object data) {
      if (m_Text) {
        m_Text.text = data as string;
      }
    }

    private void Reset() {
      m_Id = Signal.UI_SetText;
    }
  }
}