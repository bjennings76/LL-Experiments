using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Serialization;

namespace Utils.Signals {
  public class SendSignalOnEvent : AbstractSignalSender {
    [SerializeField] private SignalSpace m_Direction;
    [SerializeField, FormerlySerializedAs("m_TargetComponent")] private Component m_EventComponent;
    [SerializeField, FormerlySerializedAs("OptionalDataComponent")] private Component m_OptionalDataComponent;

    public Component EventComponent {
      get {
        return m_EventComponent;
      }
    }

    public int EventIndex {
      get {
        Init();
        return m_EventIndex;
      }
      set {
        m_EventIndex = value;
        m_EventName = EventInfo != null ? EventInfo.Name : null;
      }
    }

    [SerializeField, HideInInspector] private int m_EventIndex = -1;
    [SerializeField, HideInInspector] private string m_EventName;

    public string[] EventNameList {
      get {
        if (m_EventNameList == null) {
          Init();
        }
        return m_EventNameList;
      }
    }
    private string[] m_EventNameList;

    private EventInfo[] m_Events;

    private EventInfo EventInfo {
      get {
        if ((m_Events == null) || (m_Events.Length == 0)) {
          return null;
        }

        EventInfo selectedEvent = m_Events.FirstOrDefault(e => e.Name == m_EventName);

        if (selectedEvent == null) {
          selectedEvent = (m_EventIndex < m_Events.Length) && (m_EventIndex > -1) ? m_Events[m_EventIndex] : m_Events.FirstOrDefault();
          m_EventName = selectedEvent != null ? selectedEvent.Name : null;
        }

        return selectedEvent;
      }
    }

    private Delegate m_EventActionDelegate;
    private bool m_Init;

    private void Init() {
      if (m_Init) {
        return;
      }

      UpdateEvents();
    }

    public void UpdateEvents() {
      m_Init = true;
      if (!EventComponent) {
        m_Events = new EventInfo[0];
        m_EventNameList = new string[0];
        return;
      }

      m_Events = EventComponent.GetType().GetEvents();
      Array.Sort(m_Events, (e1, e2) => string.Compare(e1.Name, e2.Name, StringComparison.Ordinal));
      m_EventNameList = m_Events.Select(e => e.Name).ToArray();
      m_EventIndex = GetRealIndex();
    }

    private int GetRealIndex() {
      if ((m_Events == null) || (m_Events.Length == 0)) {
        return -1;
      }

      int index = m_Events.IndexOf(e => e.Name == m_EventName);
      return index < 0 ? m_EventIndex : index;
    }

    protected void OnEnable() {
      if (m_Events == null) {
        Init();
      }

      if (EventInfo == null) {
        return;
      }

      m_EventActionDelegate = EventInfo.AddEventAction(EventComponent, Send);
    }

    private void Send() {
      Component data = m_OptionalDataComponent ? m_OptionalDataComponent : EventComponent;
      SignalManager.Send(this, data, m_Direction);
    }

    protected void OnDisable() {
      if (!EventComponent || (m_EventActionDelegate == null) || (EventInfo == null)) {
        return;
      }

      EventInfo.RemoveEventHandler(EventComponent, m_EventActionDelegate);
    }
  }
}