using UnityEngine.UI;

namespace Utils.Signals {
  public class SendSignalOnClick : AbstractSignalSender {
    protected void Start() {
      Button button = GetComponent<Button>();

      if (button) {
        button.onClick.AddListener(OnMouseDown);
      }
    }

    protected void OnMouseDown() {
      Send();
    }
  }
}