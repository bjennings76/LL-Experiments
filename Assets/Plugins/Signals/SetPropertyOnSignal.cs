using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Utils.Signals {
  public class SetPropertyOnSignal : AbstractSignalReceiver {
    public Component PropertyComponent {
      get {
        return m_PropertyComponent;
      }
    }
    [SerializeField] private Component m_PropertyComponent;

    public int PropertyIndex {
      get {
        return m_PropertyIndex;
      }
      set {
        m_PropertyIndex = value;
      }
    }
    [SerializeField, HideInInspector] private int m_PropertyIndex = -1;

    public string[] PropertyNameList {
      get {
        if (m_PropertyNameList == null) {
          Init();
        }
        return m_PropertyNameList;
      }
    }
    private string[] m_PropertyNameList;

    private PropertyInfo[] m_PropertyInfos;

    private PropertyInfo PropertyInfo {
      get {
        if ((m_PropertyInfos == null) || (m_PropertyInfos.Length < m_PropertyIndex) || (m_PropertyIndex < 0)) {
          return null;
        }

        return m_PropertyInfos[m_PropertyIndex];
      }
    }

    protected override void Start() {
      base.Start();
      Init();
    }

    public void Init() {
      if (!PropertyComponent) {
        m_PropertyInfos = new PropertyInfo[0];
        m_PropertyNameList = new string[0];
        return;
      }

      m_PropertyInfos = PropertyComponent.GetType().GetProperties();
      Array.Sort(m_PropertyInfos, (e1, e2) => string.Compare(e1.Name, e2.Name, StringComparison.Ordinal));
      m_PropertyNameList = m_PropertyInfos.Select(e => e.Name).ToArray();
    }

    public override void Receive(Component sender, object data) {
      if (PropertyInfo != null) {
        PropertyInfo.SetValue(PropertyComponent, data, null);
      }
    }
  }
}