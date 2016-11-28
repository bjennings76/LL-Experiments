using UnityEngine;

namespace Utils.Signals {
  public class SignalRelay : MonoBehaviour {
    [SerializeField, SignalRef] private int m_ReceiveSignal;
    [SerializeField] private SignalSpace m_From = SignalSpace.Local;
    [Space(10), SerializeField, SignalRef] private int m_SendSignal;
    [SerializeField] private SignalSpace m_To = SignalSpace.Parent;

    protected void Start() {
      SignalManager.Register(this, m_ReceiveSignal, OnSignalReceived, m_From);
    }

    protected void OnDestroy() {
      SignalManager.UnregisterAll(this);
    }

    private void OnSignalReceived(Component sender, object data) {
      SignalManager.Send(sender, m_SendSignal, data, m_To);
    }
  }
}