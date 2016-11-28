using UnityEngine;

namespace Utils.Signals {
  public class SendSignalOnUnityEvent : MonoBehaviour, ISignalSender {
    public int Id {
      get {
        return m_Id;
      }
      set {
        m_Id = value;
      }
    }
    [SerializeField, SignalRef] private int m_Id = Signal.Enable;

    [SerializeField] private UnityEvent m_Event = UnityEvent.Start;
    [SerializeField] private SignalSpace m_To = SignalSpace.Local;

    public Component Component {
      get {
        return this;
      }
    }

    protected void Awake() {
      if (m_Event == UnityEvent.Awake) {
        SignalManager.Send(this, m_Event, m_To);
      }
    }

    protected void Start() {
      if (m_Event == UnityEvent.Start) {
        SignalManager.Send(this, m_Event, m_To);
      }
    }

    protected void OnEnable() {
      if (m_Event == UnityEvent.OnEnable) {
        SignalManager.Send(this, m_Event, m_To);
      }
    }

    protected void OnDisable() {
      if (m_Event == UnityEvent.OnDisable) {
        SignalManager.Send(this, m_Event, m_To);
      }
    }

    protected void OnDestroy() {
      if (m_Event == UnityEvent.OnDestroy) {
        SignalManager.Send(this, m_Event, m_To);
      }
    }

    private enum UnityEvent {
      Awake,
      Start,
      OnEnable,
      OnDisable,
      OnDestroy
    }
  }
}