using UnityEngine;

namespace Utils.Signals {
  public class Rotate : MonoBehaviour {
    public Vector3 Speed = new Vector3(0, 20, 0);
    public Space Space = Space.Self;

    protected void Update() {
      transform.Rotate(Speed*Time.deltaTime, Space);
    }
  }
}