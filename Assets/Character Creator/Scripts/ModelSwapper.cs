using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using Util;

public class ModelSwapper : MonoBehaviour {
  private SkinnedMeshRenderer[] m_Models;
  private Material[] m_Materials;
  private Material[] m_ValidMaterials;
  private int m_ModelIndex;
  private int m_MaterialIndex;

  private SkinnedMeshRenderer CurrentModel {
    get {
      return m_Models[m_ModelIndex];
    }
  }

  private Material CurrentMaterial {
    get {
      return m_ValidMaterials[m_MaterialIndex];
    }
  }

  private void Start() {
    m_Models = GetComponentsInChildren<SkinnedMeshRenderer>(true);
    m_Materials = Resources.LoadAll<Material>("Characters/Materials");
    UpdateModel();
    UpdateValidMaterials();
  }

  [UsedImplicitly]
  public void NextModel() {
    m_ModelIndex = GetNext(m_Models, m_ModelIndex);
    UpdateModel();
  }

  [UsedImplicitly]
  public void PreviousModel() {
    m_ModelIndex = GetPrevious(m_Models, m_ModelIndex);
    UpdateModel();
  }

  private void UpdateModel() {
    m_Models.ForEach(m => m.gameObject.SetActive(false));
    m_Models[m_ModelIndex].gameObject.SetActive(true);
    UpdateValidMaterials();
  }

  [UsedImplicitly]
  public void NextMaterial() {
    m_MaterialIndex = GetNext(m_ValidMaterials, m_MaterialIndex);
    UpdateMaterial();
  }

  [UsedImplicitly]
  public void PreviousMaterial() {
    m_MaterialIndex = GetPrevious(m_ValidMaterials, m_MaterialIndex);
    UpdateMaterial();
  }

  private void UpdateValidMaterials() {
    string modelName = CurrentModel.name;
    modelName = modelName.ReplaceRegex("^CH_", "");
    m_ValidMaterials = m_Materials.Where(m => m.name.Contains("_" + modelName + "_", StringComparison.OrdinalIgnoreCase)).ToArray();
    UpdateMaterial();
  }

  private void UpdateMaterial() {
    CurrentModel.material = CurrentMaterial;
  }

  private static int GetNext<T>(IEnumerable<T> array, int index) {
    index = index + 1;
    return index >= array.Count() ? 0 : index;
  }

  private static int GetPrevious<T>(IEnumerable<T> array, int index) {
    index = index - 1;
    return index < 0 ? array.Count() - 1 : index;
  }
}