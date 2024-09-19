using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class TriggerVolume : MonoBehaviour
{
    public enum TriggerType
    {
        Enter,
        Exit,
        Stay
    }

    [System.Serializable]
    public class TriggerEvent
    {
        public string eventName;
        public TriggerType triggerType;
        public UnityEvent onTriggerEvent;
        public List<string> triggerTags = new List<string>();
    }

    public List<TriggerEvent> triggerEvents = new List<TriggerEvent>();
    public LayerMask triggerLayers = -1; // Default to everything

    private Dictionary<Collider, HashSet<string>> triggeredEvents = new Dictionary<Collider, HashSet<string>>();

    private void OnTriggerEnter(Collider other)
    {
        if (!IsInLayerMask(other.gameObject.layer)) return;

        foreach (var triggerEvent in triggerEvents)
        {
            if (triggerEvent.triggerType == TriggerType.Enter && IsAllowedTag(other.tag, triggerEvent))
            {
                ExecuteTriggerEvent(other, triggerEvent);
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!IsInLayerMask(other.gameObject.layer)) return;

        foreach (var triggerEvent in triggerEvents)
        {
            if (triggerEvent.triggerType == TriggerType.Stay && IsAllowedTag(other.tag, triggerEvent))
            {
                ExecuteTriggerEvent(other, triggerEvent);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsInLayerMask(other.gameObject.layer)) return;

        foreach (var triggerEvent in triggerEvents)
        {
            if (triggerEvent.triggerType == TriggerType.Exit && IsAllowedTag(other.tag, triggerEvent))
            {
                ExecuteTriggerEvent(other, triggerEvent);
            }
        }

        // Clean up the dictionary
        if (triggeredEvents.ContainsKey(other))
        {
            triggeredEvents.Remove(other);
        }
    }

    private void ExecuteTriggerEvent(Collider other, TriggerEvent triggerEvent)
    {
        if (!triggeredEvents.ContainsKey(other))
        {
            triggeredEvents[other] = new HashSet<string>();
        }

        if (!triggeredEvents[other].Contains(triggerEvent.eventName))
        {
            triggerEvent.onTriggerEvent.Invoke();
            triggeredEvents[other].Add(triggerEvent.eventName);
        }
    }

    private bool IsInLayerMask(int layer)
    {
        return ((1 << layer) & triggerLayers) != 0;
    }

    private bool IsAllowedTag(string tag, TriggerEvent triggerEvent)
    {
        return triggerEvent.triggerTags.Count == 0 || triggerEvent.triggerTags.Contains(tag);
    }
}