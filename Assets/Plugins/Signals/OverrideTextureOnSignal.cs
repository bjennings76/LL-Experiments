using System.Linq;
using UnityEngine;
#pragma warning disable 0649 //Disable the "...  is never assigned to, and will always have its default value" warning
#pragma warning disable 0169 //Disable the "The field '...' is never used" warning

namespace Utils.Signals {
  [RequireComponent(typeof(Renderer))]
  public class OverrideTextureOnSignal : MonoBehaviour {
    [SerializeField, SignalRef] private int m_Signal = Signal.OverrideTexture;
    [SerializeField] private Material m_MaterialWithTexture;
    [SerializeField, AutoPopulate] private Renderer m_Renderer;

    protected void Start() {
      SignalManager.Register(this, m_Signal, OnOverrideTextureSignal);
      XDebug.AssertRequiresComponent(m_Renderer, this);
      XDebug.Assert(m_MaterialWithTexture, this, "Material reference missing.");
    }

    private void OnOverrideTextureSignal(Component sender, object data) {
      Texture texture = data as Texture;

      if (!texture || !m_MaterialWithTexture || !m_Renderer) {
        XDebug.Assert(texture, this, "Signal data doesn't contain a texture.");
        XDebug.Assert(m_MaterialWithTexture, this, "Material reference missing.");
        XDebug.Assert(m_Renderer, this, "Renderer missing.");
        return;
      }

      Material mat = m_Renderer.materials.FirstOrDefault(MatchingMaterial);

      if (!mat) {
        XDebug.LogWarning(this, "No matching texture: " + m_MaterialWithTexture.name);
        return;
      }

      mat.mainTexture = texture;
    }

    private bool MatchingMaterial(Material material) {
      return material.name.StartsWith(m_MaterialWithTexture.name);
    }
  }
}