using System;
using UnityEngine;

namespace Utils.Signals {
  public class ActionReceiver : ISignalReceiver {
    public Component Component { get; private set; }

    public int Id { get; set; }

    private readonly string m_Name;
    private readonly Action<Component, object> m_Action;

    public ActionReceiver(Component receiver, int signal, Action<Component, object> action) {
      Component = receiver;
      m_Name = Component.name + ":" + Component.GetType().Name;
      Id = signal;
      m_Action = action;
    }

    public void Receive(Component sender, object data) {
      if (m_Action != null) {
        m_Action(sender, data);
      }
    }

    public override string ToString() {
      return m_Name;
    }
  }
}