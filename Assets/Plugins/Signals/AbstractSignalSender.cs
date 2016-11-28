namespace Utils.Signals {
  public abstract class AbstractSignalSender : AbstractSignalUser, ISignalSender {
    protected void Send(object data = null) {
      SignalManager.Send(this, data);
    }
  }
}