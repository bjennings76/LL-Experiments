using UnityEngine;
using UnityEngine.UI;

namespace Utils.Signals {
  public class SetColorOnSignal : AbstractSignalReceiver {
    public Color Color = Color.white;

    public override void Receive(Component sender, object data) {
      Renderer r = GetComponent<Renderer>();

      if (r && r.material) {
        r.material.color = Color;
        return;
      }

      Graphic graphic = GetComponent<Graphic>();

      if (graphic) {
        graphic.color = Color;
      }
    }
  }
}