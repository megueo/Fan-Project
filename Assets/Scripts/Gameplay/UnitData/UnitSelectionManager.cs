using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UnitSelectionManager : MonoBehaviour
{
    public static UnitSelectionManager Instance { get; private set; }

    [Header("Layers")]
    public LayerMask clickable;
    public LayerMask ground;

    [Header("Visuals")]
    public GameObject selectionIndicatorPrefab = null;

    private Camera cam;

    private readonly HashSet<GameObject> allUnits = new();
    private readonly HashSet<GameObject> selectedUnits = new();

    #region Unity

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        cam = Camera.main;
    }

    private void Update()
    {
        HandleSelectionInput();
        HandleMovementInput();
    }

    #endregion

    #region Public API (Units)

    public void RegisterUnit(GameObject unit)
    {
        allUnits.Add(unit);
        Debug.Log($"[Selection] Unit registrada: {unit.name}");
    }

    public void UnregisterUnit(GameObject unit)
    {
        allUnits.Remove(unit);
        selectedUnits.Remove(unit);
    }

    #endregion

    #region Selection

    private void HandleSelectionInput()
    {
        if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame)
            return;

        Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, clickable))
        {
            UnitSelectable selectable = hit.collider.GetComponentInParent<UnitSelectable>();
            if (selectable == null) return;

            GameObject unit = selectable.gameObject;

            if (Keyboard.current.leftShiftKey.isPressed)
                ToggleSelection(unit);
            else
                SelectSingle(unit);
        }
        else if (!Keyboard.current.leftShiftKey.isPressed)
        {
            DeselectAll();
        }
    }

    private void ToggleSelection(GameObject unit)
    {
        if (selectedUnits.Contains(unit))
            Deselect(unit);
        else
            Select(unit);
    }

    private void SelectSingle(GameObject unit)
    {
        DeselectAll();
        Select(unit);
    }

    private void Select(GameObject unit)
    {
        selectedUnits.Add(unit);
        SetIndicator(unit, true);
    }

    private void Deselect(GameObject unit)
    {
        selectedUnits.Remove(unit);
        SetIndicator(unit, false);
    }

    private void DeselectAll()
    {
        foreach (var unit in selectedUnits)
            SetIndicator(unit, false);

        selectedUnits.Clear();
    }

    private void SetIndicator(GameObject unit, bool active)
    {
        Transform indicator = unit.transform.Find("SelectionIndicator");

        if (indicator == null && active)
        {
            GameObject inst = Instantiate(selectionIndicatorPrefab, unit.transform);
            inst.name = "SelectionIndicator";

            Collider col = unit.GetComponentInChildren<Collider>();
            float y = col != null ? -col.bounds.extents.y + 0.05f : 0.1f;
            inst.transform.localPosition = new Vector3(0, y, 0);

            indicator = inst.transform;
        }

        if (indicator != null)
            indicator.gameObject.SetActive(active);
    }

    #endregion

    #region Movement

    private void HandleMovementInput()
    {
        if (Mouse.current == null ||
            !Mouse.current.rightButton.wasPressedThisFrame ||
            selectedUnits.Count == 0)
            return;

        Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, ground))
            return;

        Vector3 destination = hit.point;

        int cols = Mathf.CeilToInt(Mathf.Sqrt(selectedUnits.Count));
        float spacing = 1.3f;

        int i = 0;
        foreach (var unit in selectedUnits)
        {
            int row = i / cols;
            int col = i % cols;

            Vector3 offset = new(
                (col - cols / 2f) * spacing,
                0,
                (row - cols / 2f) * spacing
            );

            if (unit.TryGetComponent(out UnitMovement move))
                move.MoveTo(destination + offset);

            i++;
        }
    }

    #endregion
}
