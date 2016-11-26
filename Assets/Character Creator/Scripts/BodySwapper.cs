using UnityEngine;

public class BodySwapper : MonoBehaviour {
  private MeshRenderer[] m_Renderers;
  private Material[] m_Materials;

  private void Start() {
    m_Renderers = GetComponentsInChildren<MeshRenderer>();
    m_Materials = Resources.LoadAll<Material>("Characters/Materials");
  }

  public void NextModel() {
    
  }

  public void NextMaterial() {
    
  }
}