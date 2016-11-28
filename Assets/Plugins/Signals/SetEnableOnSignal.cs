using UnityEngine;
using UnityEngine.Serialization;

namespace Utils.Signals {
  public class SetEnableOnSignal : MonoBehaviour {
    public Behaviour Target;

    private int EnableSignal {
      get {
        return m_EnableSignal;
      }
    }
    [SerializeField, SignalRef, FormerlySerializedAs("m_Id")] private int m_EnableSignal = Signal.Enable;

    private int DisableSignal {
      get {
        return m_DisableSignal;
      }
    }
    [SerializeField, SignalRef] private int m_DisableSignal = Signal.Disable;

    private void Start() {
      SignalManager.Register(this, EnableSignal, OnEnableSignal);
      SignalManager.Register(this, DisableSignal, OnDisableSignal);
    }

    private void OnDestroy() {
      SignalManager.UnregisterAll(this);
    }

    private void OnEnableSignal() {
      Target.enabled = true;
    }

    private void OnDisableSignal() {
      Target.enabled = false;
    }
  }
}