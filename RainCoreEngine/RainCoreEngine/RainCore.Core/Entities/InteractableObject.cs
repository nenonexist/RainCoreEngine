using OpenTK.Mathematics;

namespace RainCore;

                                                                                            
public sealed class InteractableObject : IInteractable
{
    public string Id { get; }
    public Vector3 Position { get; }
    public float InteractRadius { get; }
    public string DisplayName { get; }

                 
                                                                     
                                                                              
                                                                            
                                                                          
                  
    public bool IsActive { get; private set; }
    public string ActiveText { get; }
    public string InactiveText { get; }

                                                                                                                 
    public bool ExamineOnly { get; }

                                                                                                                      
    public bool OnceOnly { get; }

                                                                                                                      
    public string? RequiredItem { get; }
    public int RequiredItemAmount { get; }

                                                                                                              
    public bool ConsumesItem { get; }

                                                                                                                       
    public string LockedText { get; }

                                                                                                                             
    public string MarkerPath { get; }

                                                                                      
                                                                  
    public float MarkerScale { get; }

    public InteractableObject(
        string id,
        string displayName,
        Vector3 position,
        bool initialState = false,
        string activeText = "Включено.",
        string inactiveText = "Выключено.",
        float interactRadius = 2.5f,
        bool examineOnly = false,
        bool onceOnly = false,
        string? requiredItem = null,
        int requiredItemAmount = 1,
        bool consumesItem = false,
        string lockedText = "Заперто.",
        string markerPath = "",
        float markerScale = 1f)
    {
        if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Id объекта не может быть пустым.", nameof(id));
        Id = id;
        DisplayName = displayName;
        Position = position;
        IsActive = initialState;
        ActiveText = activeText;
        InactiveText = inactiveText;
        InteractRadius = interactRadius;
        ExamineOnly = examineOnly;
        OnceOnly = onceOnly;
        RequiredItem = requiredItem;
        RequiredItemAmount = requiredItemAmount;
        ConsumesItem = consumesItem;
        LockedText = lockedText;
        MarkerPath = markerPath;
        MarkerScale = markerScale <= 0f ? 1f : markerScale;
    }

    public string? Interact()
    {
        bool hasRequiredItem = string.IsNullOrWhiteSpace(RequiredItem) || Inventory.Get(RequiredItem) >= RequiredItemAmount;
        if (!hasRequiredItem)
            return LockedText;

        if (ExamineOnly)
        {
            if (OnceOnly && IsActive) return null;                                                
            if (ConsumesItem && !string.IsNullOrWhiteSpace(RequiredItem))
                Inventory.Remove(RequiredItem, RequiredItemAmount);
            if (OnceOnly) IsActive = true;
            return ActiveText;
        }

        if (ConsumesItem && !string.IsNullOrWhiteSpace(RequiredItem))
            Inventory.Remove(RequiredItem, RequiredItemAmount);

        IsActive = !IsActive;
        return IsActive ? ActiveText : InactiveText;
    }

    public void SetState(bool active)
    {
        IsActive = active;
    }
}
