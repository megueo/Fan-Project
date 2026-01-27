using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class UnitSelectionManager : MonoBehaviour
{
    public static UnitSelectionManager Instance { get; private set; }

    [Header("Units")]
    public List<GameObject> allUnitsList = new List<GameObject>();
    public HashSet<GameObject> unitsSelected = new HashSet<GameObject>();

    [Header("Markers")]
    public LayerMask clickable; 
    public LayerMask ground;
    public GameObject groundMarker;
    public GameObject selectionIndicatorPrefab;

    private Camera cam;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    private void Start()
    {
        cam = Camera.main;
        if (groundMarker != null) groundMarker.SetActive(false);
    }

    private void Update()
    {
        HandleLeftClickSelection();
        HandleRightClickMovement();
    }

    #region Selection

    private void HandleLeftClickSelection()
    {
        if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame) return;

        Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, clickable))
        {
            GameObject unit = hit.collider.gameObject;

            if (Keyboard.current.leftShiftKey.isPressed)
                MultiSelect(unit);
            else
                SelectByClicking(unit);
        }
        else if (!Keyboard.current.leftShiftKey.isPressed)
        {
            DeselectAll();
        }
    }

    private void MultiSelect(GameObject unit)
    {
        if (!unitsSelected.Contains(unit))
        {
            unitsSelected.Add(unit);
            TriggerSelectionIndicator(unit, true);
        }
        else
        {
            TriggerSelectionIndicator(unit, false);
            unitsSelected.Remove(unit);
        }
    }

    private void SelectByClicking(GameObject unit)
    {
        if (unitsSelected.Count == 1 && unitsSelected.Contains(unit))
        {
            TriggerSelectionIndicator(unit, false);
            unitsSelected.Remove(unit);
            groundMarker.SetActive(false);
            return;
        }

        DeselectAll();
        unitsSelected.Add(unit);
        TriggerSelectionIndicator(unit, true);
    }

    private void DeselectAll()
    {
        foreach (var unit in unitsSelected)
            TriggerSelectionIndicator(unit, false);

        unitsSelected.Clear();
        if (groundMarker != null) groundMarker.SetActive(false);
    }

    private void TriggerSelectionIndicator(GameObject unit, bool isVisible)
    {
        Transform indicator = unit.transform.Find("SelectionIndicator");

        if (indicator == null && isVisible)
        {
            GameObject inst = Instantiate(selectionIndicatorPrefab, unit.transform);
            inst.name = "SelectionIndicator";

            Collider col = unit.GetComponent<Collider>();
            float yOffset = 0.05f;

            if (col != null)
                inst.transform.localPosition = new Vector3(0, -col.bounds.extents.y + yOffset, 0);
            else
                inst.transform.localPosition = new Vector3(0, 0.1f, 0);

            indicator = inst.transform;
        }

        if (indicator != null)
            indicator.gameObject.SetActive(isVisible);
    }


    #endregion

    #region Movement

    private void HandleRightClickMovement()
    {
        if (Mouse.current == null || !Mouse.current.rightButton.wasPressedThisFrame || unitsSelected.Count == 0)
            return;

        Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, ground))
        {
            Vector3 destination = hit.point;

            Debug.Log($"[Selection] Right click at {destination}");

            if (groundMarker != null)
            {
                groundMarker.SetActive(true);
                groundMarker.transform.position = destination + Vector3.up * 0.1f;
            }

            foreach (var unit in unitsSelected)
            {
                UnitMovement movement = unit.GetComponent<UnitMovement>();
                if (movement != null)
                    movement.MoveTo(destination);
                else
                    Debug.LogWarning("[Selection] Unit has no UnitMovement!");
            }
        }
    }

    #endregion
}
