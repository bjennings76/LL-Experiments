using UnityEngine;

namespace Utils.Signals {
  public interface ISignalUser {
    int Id { get; set; }
    Component Component { get; }
  }

  public interface ISignalSender : ISignalUser {}

  public interface ISignalReceiver : ISignalUser {
    void Receive(Component sender, object data);
  }
}