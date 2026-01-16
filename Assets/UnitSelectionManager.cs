using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UnitSelectionManager : MonoBehaviour
{
    public static UnitSelectionManager Instance { get;private set; }

    public List<GameObject> allUnitsList = new List<GameObject>();
    public HashSet<GameObject> unitsSelected = new HashSet<GameObject>();

    public LayerMask clickable;
    public LayerMask ground;
    public GameObject groundMarker;

    private Camera cam;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        cam = Camera.main;
    }

    private void Update()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            RaycastHit hit;
            Vector2 mousePos = Mouse.current.position.ReadValue();
            Ray ray = cam.ScreenPointToRay(mousePos);

            // If clickable object is hit
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, clickable))
            {
                if (Keyboard.current.leftShiftKey.isPressed)
                {
                    MultiSelect(hit.collider.gameObject);
                }
                else
                {
                    SelectByClicking(hit.collider.gameObject);
                }
                                  
            }
            else // If NOT hittng clickable object
            {
                if (Keyboard.current.leftShiftKey.isPressed == false)
                {
                    DeselectAll();
                }
            }
        }

        if (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame && unitsSelected.Count > 0)
        {
            RaycastHit hit;
            Vector2 mousePos = Mouse.current.position.ReadValue();
            Ray ray = cam.ScreenPointToRay(mousePos);

            // If clickable object is hit
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, ground))
            {
                groundMarker.transform.position = hit.point;

                groundMarker.SetActive(false);
                groundMarker.SetActive(true);
            }
        }

    }

    private void MultiSelect(GameObject unit)
    {
        if (unitsSelected.Contains(unit) == false)
        {
            unitsSelected.Add(unit);
            TriggerSelectionIndicator(unit, true);
            EnableUnitMovement(unit, true);
        }
        else
        {
            EnableUnitMovement(unit, false);
            TriggerSelectionIndicator(unit, false);
            unitsSelected.Remove(unit);
        }
    }

    private void DeselectAll()
    {
        foreach (var unit in unitsSelected)
        {
            EnableUnitMovement(unit, false);
            TriggerSelectionIndicator(unit, false);
        }

        groundMarker.SetActive(false);

        unitsSelected.Clear();
    }

    private void SelectByClicking(GameObject unit)
    {
        // if the same unit is clicked again, deselect it
        if (unitsSelected.Count == 1 && unitsSelected.Contains(unit))
        {
            EnableUnitMovement(unit, false);
            TriggerSelectionIndicator(unit, false);
            unitsSelected.Remove(unit);

            groundMarker.SetActive(false);
            return;
        }

        // in any other case, deselect all and select the clicked unit
        DeselectAll();

        unitsSelected.Add(unit);

        TriggerSelectionIndicator(unit, true);
        EnableUnitMovement(unit, true);
    }

    private void EnableUnitMovement(GameObject unit, bool shouldMove)
    {
        unit.GetComponent<UnitMovement>().enabled = shouldMove;
    }

    private void TriggerSelectionIndicator(GameObject unit, bool isVisible)
    {
        unit.transform.GetChild(0).gameObject.SetActive(isVisible);
    }

}
