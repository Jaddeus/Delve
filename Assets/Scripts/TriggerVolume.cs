using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class TriggerVolume : MonoBehaviour
{
    // Enum to define different types of trigger events
    public enum TriggerType
    {
        Enter,
        Exit,
        Stay
    }

    // Serializable class to define a trigger event
    [System.Serializable]
    public class TriggerEvent
    {
        public string eventName;
        public TriggerType triggerType;
        public UnityEvent onTriggerEvent;
        public List<string> triggerTags = new List<string>();
    }

    // List of trigger events defined in the inspector
    public List<TriggerEvent> triggerEvents = new List<TriggerEvent>();
    // LayerMask to filter which objects can trigger events
    public LayerMask triggerLayers = -1; // Default to everything

    // Dictionary to keep track of triggered events for each collider
    private Dictionary<Collider, HashSet<string>> triggeredEvents = new Dictionary<Collider, HashSet<string>>();

    // Called when a collider enters the trigger volume
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

    // Called every frame for each collider staying in the trigger volume
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

    // Called when a collider exits the trigger volume
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

    // Execute the trigger event and track it in the dictionary
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

    // Check if the given layer is included in the triggerLayers mask
    private bool IsInLayerMask(int layer)
    {
        return ((1 << layer) & triggerLayers) != 0;
    }

    // Check if the given tag is allowed for the trigger event
    private bool IsAllowedTag(string tag, TriggerEvent triggerEvent)
    {
        return triggerEvent.triggerTags.Count == 0 || triggerEvent.triggerTags.Contains(tag);
    }
}