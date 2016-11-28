using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

namespace Utils.Signals {
  public class SignalHub : MonoBehaviour {
    public readonly Dictionary<int, List<ISignalReceiver>> ReceiverLookup = new Dictionary<int, List<ISignalReceiver>>();

    private readonly Dictionary<Component, Dictionary<int, ISignalReceiver>> m_ComponentLookup = new Dictionary<Component, Dictionary<int, ISignalReceiver>>();

    private readonly Dictionary<int, List<SignalPacket>> m_SignalsLastFrame = new Dictionary<int, List<SignalPacket>>();

    protected void Awake() {
      FindReceivers();
    }

    protected void Update() {
#if XDEBUG
			foreach (List<SignalPacket> packets in m_SignalsLastFrame.Values) {
				foreach (SignalPacket packet in packets) {
					if (packet.NoReceivers) {
						XDebug.LogWarning(this, "No receivers found for '{0}'", Signal.LookupName(packet.Signal));
					}
				}
			}
#endif
      m_SignalsLastFrame.Clear();
    }

    public void Register(Component receiver, int signal, Action callback) {
      if (callback == null) {
        XDebug.LogError(receiver, "Missing callback.");
        return;
      }

      Register(receiver, signal, (sender, data) => callback());
    }

    public void Register(Component receiver, int signal, Action<Component> callback) {
      if (callback == null) {
        XDebug.LogError(receiver, "Missing callback.");
        return;
      }

      Register(receiver, signal, (sender, data) => callback(sender));
    }

    public void Register([NotNull] Component receiver, int signal, [NotNull] Action<Component, object> callback) {
      if (receiver == null) {
        throw new ArgumentNullException("receiver");
      }
      if (callback == null) {
        throw new ArgumentNullException("callback");
      }

      // Unregister any previous receivers for this component/signal combo.
      Unregister(receiver, signal);

      // Create a new receiver for this component.
      ActionReceiver r = new ActionReceiver(receiver, signal, callback);

      // Keep track of this component's receivers.
      Dictionary<int, ISignalReceiver> pairs;
      if (!m_ComponentLookup.TryGetValue(receiver, out pairs)) {
        pairs = m_ComponentLookup[receiver] = new Dictionary<int, ISignalReceiver>();
      }
      pairs[signal] = r;

      // Register the receiver as usual.
      Register(r);
    }

    public void Register([NotNull] ISignalReceiver receiver) {
      if (receiver == null) {
        throw new ArgumentNullException("receiver");
      }

      int key = receiver.Id;

      if (!ReceiverLookup.ContainsKey(key)) {
        ReceiverLookup[key] = new List<ISignalReceiver>();
      }
      List<ISignalReceiver> list = ReceiverLookup[key];
      if (list.Contains(receiver)) {
        return;
      }
      list.Add(receiver);

      List<SignalPacket> packetList;
      if (m_SignalsLastFrame.TryGetValue(key, out packetList)) {
        foreach (SignalPacket packet in packetList) {
          receiver.Receive(packet.Sender, packet.Data);
          packet.NoReceivers = false;
        }
      }
    }

    public void Unregister([NotNull] Component receiver, int signal) {
      if (receiver == null) {
        throw new ArgumentNullException("receiver");
      }

      Dictionary<int, ISignalReceiver> pairs;

      if (!m_ComponentLookup.TryGetValue(receiver, out pairs)) {
        return;
      }

      ISignalReceiver r;

      if (!pairs.TryGetValue(signal, out r)) {
        return;
      }

      Unregister(r);
      pairs.Remove(signal);
      if (pairs.Count == 0) {
        m_ComponentLookup.Remove(receiver);
      }
    }

    private void Unregister([NotNull] ISignalReceiver receiver) {
      if (receiver == null) {
        throw new ArgumentNullException("receiver");
      }

      int key = receiver.Id;

      List<ISignalReceiver> list;

      ReceiverLookup.TryGetValue(key, out list);

      if ((list != null) && list.Contains(receiver)) {
        list.Remove(receiver);
      }
    }

    public void UnregisterAll([NotNull] Component receiver) {
      if (receiver == null) {
        throw new ArgumentNullException("receiver");
      }

      Dictionary<int, ISignalReceiver> pairs;

      if (!m_ComponentLookup.TryGetValue(receiver, out pairs)) {
        return;
      }

      foreach (ISignalReceiver r in pairs.Values) {
        Unregister(r);
      }

      pairs.Clear();
      m_ComponentLookup.Remove(receiver);
    }

    public bool Send([NotNull] Component sender, int signal, object data = null) {
      if (sender == null) {
        throw new ArgumentNullException("sender");
      }

      SignalPacket signalPacket = new SignalPacket {Data = data, Sender = sender, Signal = signal};
      AddSignalPacket(signalPacket);

      if (!ReceiverLookup.ContainsKey(signal)) {
        signalPacket.NoReceivers = true;
        return false;
      }

      List<ISignalReceiver> receivers = ReceiverLookup[signal].ToList();
      if (receivers.Any(r => r == null)) {
        receivers = ReceiverLookup[signal] = receivers.Where(r => r != null).ToList();
      }

#if XDEBUG
			XDebug.Log(sender, "{0} --[ {1} ]--> {2}", sender.name + ":" + sender.GetType().Name, Signal.LookupName(signal),
				receivers.AggregateString(", ") + (data != null ? " (" + data + ")" : ""));
#endif

      receivers.ForEach(r => {
                          if (r != null) {
                            r.Receive(sender, data);
                          }
                        });

      return true;
    }

    private void AddSignalPacket(SignalPacket packet) {
      List<SignalPacket> list;
      if (!m_SignalsLastFrame.TryGetValue(packet.Signal, out list)) {
        m_SignalsLastFrame[packet.Signal] = list = new List<SignalPacket>();
      }
      list.Add(packet);
    }

    public void FindReceivers() {
      if (!this) {
        return;
      }

      ReceiverLookup.Clear();
      FindChildSignalUsers(transform, ReceiverLookup);
    }

    private static void FindChildSignalUsers<T>([NotNull] Transform t, [NotNull] Dictionary<int, List<T>> lookUp) where T : ISignalUser {
      if (t == null) {
        throw new ArgumentNullException("t");
      }
      if (lookUp == null) {
        throw new ArgumentNullException("lookUp");
      }

      foreach (T receiver in t.GetComponents<T>()) {
        if (!lookUp.ContainsKey(receiver.Id)) {
          lookUp[receiver.Id] = new List<T>();
        }

        lookUp[receiver.Id].Add(receiver);
      }

      foreach (Transform child in t.GetChildren()) {
        if (!child.GetComponent<SignalHub>()) {
          FindChildSignalUsers(child, lookUp);
        }
      }
    }

    private class SignalPacket {
      public Component Sender;
      public int Signal;
      public object Data;
      public bool NoReceivers;
    }
  }
}