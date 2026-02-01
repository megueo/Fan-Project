using UnityEngine;

public class UnitSelectable : MonoBehaviour
{
    private void OnEnable()
    {
        UnitSelectionManager.Instance?.RegisterUnit(gameObject);
    }

    private void OnDisable()
    {
        UnitSelectionManager.Instance?.UnregisterUnit(gameObject);
    }
}
