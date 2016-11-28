using UnityEngine;

namespace Utils.Signals {
  public abstract class AbstractSignalUser : MonoBehaviour, ISignalUser {
    public int Id {
      get {
        return m_Id;
      }
      set {
        m_Id = value;
      }
    }
    [SerializeField, SignalRef] protected int m_Id;

    public Component Component {
      get {
        return this;
      }
    }
  }
}