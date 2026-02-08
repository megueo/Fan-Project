using UnityEngine;

public class UnitSelectable : MonoBehaviour
{
    void OnEnable()
    {
        UnitSelectionManager.Instance?.RegisterUnit(gameObject);
    }

    void OnDisable()
    {
        UnitSelectionManager.Instance?.UnregisterUnit(gameObject);
    }

    public void OnSelected()
    {
        Pathfinder.Instance?.ShowDebug();
    }

    public void OnDeselected()
    {
        Pathfinder.Instance?.HideDebug();
    }
}
