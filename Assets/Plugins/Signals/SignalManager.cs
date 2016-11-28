using System;
using JetBrains.Annotations;
using UnityEngine;

namespace Utils.Signals {
  public class SignalManager : Singleton<SignalManager> {
    public static SignalHub GlobalHub {
      get {
        if (!Application.isPlaying || !Instance) {
          return null;
        }

        return Instance.m_GlobalHub ? Instance.m_GlobalHub : Instance.m_GlobalHub = CreateGlobalHub();
      }
    }

    private SignalHub m_GlobalHub;

    public static bool Send([NotNull] ISignalSender sender, object data = null, SignalSpace direction = SignalSpace.Local) {
      if (sender == null) {
        throw new ArgumentNullException("sender");
      }

      return Send(sender.Component, sender.Id, data, direction);
    }

    public static bool Send(Component sender, int signal, object data = null, SignalSpace direction = SignalSpace.Local) {
      if (IsQuitting) {
        return false;
      }

      if (IsQuitting) {
        return false;
      }

      switch (direction) {
        case SignalSpace.Local:
          return FindHub(sender).Send(sender, signal, data);
        case SignalSpace.Parent:
          return FindParentHub(sender).Send(sender, signal, data);
        case SignalSpace.Global:
          return GlobalHub.Send(sender, signal, data);
        default:
          throw new ArgumentOutOfRangeException("direction", direction, null);
      }
    }

    public static void Register(Component receiver, int signal, [NotNull] Action callback, SignalSpace direction = SignalSpace.Local) {
      if (callback == null) {
        throw new ArgumentNullException("callback");
      }

      Register(receiver, signal, (sender, data) => callback(), direction);
    }

    public static void Register(Component receiver, int signal, [NotNull] Action<Component> callback, SignalSpace direction = SignalSpace.Local) {
      if (callback == null) {
        throw new ArgumentNullException("callback");
      }
      Register(receiver, signal, (sender, data) => callback(sender), direction);
    }

    public static void Register(Component receiver, int signal, Action<Component, object> callback, SignalSpace direction = SignalSpace.Local) {
      if (IsQuitting) {
        return;
      }

      switch (direction) {
        case SignalSpace.Local:
          FindHub(receiver).Register(receiver, signal, callback);
          break;
        case SignalSpace.Parent:
          FindParentHub(receiver).Register(receiver, signal, callback);
          break;
        case SignalSpace.Global:
          GlobalHub.Register(receiver, signal, callback);
          break;
        default:
          throw new ArgumentOutOfRangeException("direction", direction, null);
      }
    }

    public static void Register([NotNull] ISignalReceiver receiver) {
      if (receiver == null) {
        throw new ArgumentNullException("receiver");
      }
      SignalHub hub = FindHub(receiver.Component);
      if (!IsQuitting) {
        hub.Register(receiver);
      }
    }

    public static void Unregister([NotNull] ISignalReceiver receiver, SignalSpace direction = SignalSpace.Local) {
      if (receiver == null) {
        throw new ArgumentNullException("receiver");
      }
      Unregister(receiver.Component, receiver.Id, direction);
    }

    public static void Unregister(Component receiver, int signal, SignalSpace direction = SignalSpace.Local) {
      if (IsQuitting) {
        return;
      }

      switch (direction) {
        case SignalSpace.Local:
          SignalHub hub = FindHub(receiver);
          hub.Unregister(receiver, signal);
          break;
        case SignalSpace.Parent:
          SignalHub parentHub = FindParentHub(receiver);
          parentHub.Unregister(receiver, signal);
          break;
        case SignalSpace.Global:
          GlobalHub.Unregister(receiver, signal);
          break;
        default:
          throw new ArgumentOutOfRangeException("direction", direction, null);
      }
    }

    public static void UnregisterAll(Component receiver) {
      if (IsQuitting) {
        return;
      }

      SignalHub hub = FindHub(receiver);
      hub.UnregisterAll(receiver);

      SignalHub parentHub = FindParentHub(receiver);

      if (parentHub) {
        parentHub.UnregisterAll(receiver);
      }

      GlobalHub.UnregisterAll(receiver);
    }

    public static SignalHub FindHub(Component component) {
      if (IsQuitting) {
        return null;
      }

      if (!component) {
        return GlobalHub;
      }

      SignalHub hub = component.GetComponentInParent<SignalHub>();
      return hub ? hub : GlobalHub;
    }

    public static SignalHub FindParentHub([NotNull] Component component) {
      if (component == null) {
        throw new ArgumentNullException("component");
      }

      SignalHub hub = FindHub(component);

      if (!hub) {
        if (Application.isPlaying) {
          Debug.LogWarning("Can't find hub for " + component.name, component);
        }
        return null;
      }

      SignalHub parent = FindHub(hub.transform.parent);

      if (hub != parent) {
        return parent;
      }

      if (hub != GlobalHub) {
        Debug.LogWarning("Can't find parent hub for " + component.name, component);
      }
      return null;
    }

    private static SignalHub CreateGlobalHub() {
      const string kGlobalHubName = "GlobalHub";

      // See if we already have a global hub object...
      Transform t = Instance.transform.Find(kGlobalHubName);

      // Otherwise, make one.fac
      if (!t) {
        GameObject go = new GameObject();
        go.name = kGlobalHubName;
        t = go.transform;
        t.SetParent(Instance.transform);
      }

      return t.GetOrAddComponent<SignalHub>();
    }
  }
}