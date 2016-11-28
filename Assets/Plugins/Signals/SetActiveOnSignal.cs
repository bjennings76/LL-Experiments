using UnityEngine;

namespace Utils.Signals {
  public class SetActiveOnSignal : MonoBehaviour {
    public GameObject Target;
    public int EnableSignal {
      get {
        return m_EnableSignal;
      }
      set {
        m_EnableSignal = value;
      }
    }
    [SerializeField, SignalRef] private int m_EnableSignal = Signal.Enable;

    public int DisableSignal {
      get {
        return m_DisableSignal;
      }
      set {
        m_DisableSignal = value;
      }
    }
    [SerializeField, SignalRef] private int m_DisableSignal = Signal.Disable;

    private void Start() {
      SignalManager.Register(this, EnableSignal, OnEnableSignal);
      SignalManager.Register(this, DisableSignal, OnDisabelSignal);
      Target = Target ? Target : gameObject;
    }

    private void OnDestroy() {
      SignalManager.UnregisterAll(this);
    }

    private void OnEnableSignal() {
      Target.SetActive(true);
    }

    private void OnDisabelSignal() {
      Target.SetActive(false);
    }
  }
}