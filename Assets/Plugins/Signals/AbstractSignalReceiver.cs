using UnityEngine;

namespace Utils.Signals {
  public abstract class AbstractSignalReceiver : AbstractSignalUser, ISignalReceiver {
    protected virtual void Start() {
      SignalManager.Register(this);
    }

    protected void OnDestroy() {
      SignalManager.Unregister(this);
    }

    public abstract void Receive(Component sender, object data);

    public override string ToString() {
      return Component.name + ":" + Component.GetType().Name;
    }
  }
}