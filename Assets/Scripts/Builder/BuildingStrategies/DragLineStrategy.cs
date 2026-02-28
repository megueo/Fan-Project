using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;

public class DragLineStrategy : IBuilderStrategy
{
    BlockAuthoringComponent data;
    GameObject Prefab;
    GameObject MainObject;
    List<GameObject> PreviewObjects;

    Vector3Int GridPosition;
    Vector3Int StartGridPosition;
    Vector3Int EndGridPosition;
    Vector3Int oldposition;
    float Angle;
    bool Holding;

    public DragLineStrategy(GameObject Prefab)
    {
        this.data = Prefab.GetComponent<BlockAuthoringComponent>();
        this.Prefab = Prefab;
        PreviewObjects = new List<GameObject>();
        MainObject = GameObject.Instantiate(Prefab);
        MainObject.GetComponentInChildren<MeshRenderer>().material = Builder.Instance.TransparentMaterial;
        Collider[] colliders = MainObject.GetComponents<Collider>();
        for (int i = 0; i < colliders.Length; i++)
        {
            colliders[i].enabled = false;
        }
        //MainObject.GetComponentInChildren<MeshRenderer>().material = Builder.Instance.TransparentMaterial;
        MainObject.transform.SetParent(WorldGen.Instance.transform);
    }

    public void Dispose()
    {
        GameObject.Destroy(MainObject);
        if (PreviewObjects.Count != 0)
        {
            for (int i = 0; i < PreviewObjects.Count; i++)
            {
                GameObject.Destroy(PreviewObjects[i]);
            }
            PreviewObjects.Clear();
        };
    }

    public void OnMouseDown()
    {
        StartGridPosition = GridPosition;
        Holding = true;
    }

    public void OnMousePerform()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            //To Summerize this code, if we're holding down the mouse, it'll find a direction at the start point
            //We find the most dominent direction and set the EndGridPosition along that direction
            Vector3Int FinalPosition = Vector3Int.FloorToInt(hit.point + hit.normal * 0.01f);
            Vector3 GridDir = (Vector3Int.FloorToInt(hit.point) - StartGridPosition);
            GridDir.Normalize();
            
            if (GridDir.x != 0 && GridDir.z != 0)
            {
                float FinalDir = Mathf.Max(Mathf.Abs(GridDir.x), Mathf.Abs(GridDir.z));
                if (FinalDir == Mathf.Abs(GridDir.x))
                {
                    FinalPosition.z = StartGridPosition.z;

                    FinalPosition.x = Vector3Int.FloorToInt(hit.point).x;
                }
                if (FinalDir == Mathf.Abs(GridDir.z))
                {
                    FinalPosition.x = StartGridPosition.x;
                    FinalPosition.z = Vector3Int.FloorToInt(hit.point).z;
                }
            }

            EndGridPosition = FinalPosition;
        }


    }

    public void OnMouseUp()
    {
        Holding = false;
        for (int i = 0; i < PreviewObjects.Count; i++)
        {
            //Same thing as VerticalObjectStrategy
            GameObject fab = GameObject.Instantiate(Prefab, PreviewObjects[i].transform.position, Quaternion.Euler(0, Angle, 0));
            fab.GetComponent<BlockAuthoringComponent>().AuthorizeComponent();
        }
    }

    public void OnRotate()
    {
        //Rotation stuff, works clockrise
        Angle += 90;
        if (Angle >= 360)
        {
            Angle = 0;
        }
    }

    public void Preview()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        if (Holding == false)
        {
            if (PreviewObjects.Count != 0)
            {
                for (int i = 0; i < PreviewObjects.Count; i++)
                {
                    GameObject.Destroy(PreviewObjects[i]);
                }
                PreviewObjects.Clear();
            };

            Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            {

                GridPosition = Vector3Int.FloorToInt(hit.point + hit.normal * 0.01f);
                MainObject.transform.position = GridPosition + new Vector3(0.5f, 0.5f, 0.5f);
                MainObject.transform.rotation = Quaternion.Euler(0, Angle, 0);
                MainObject.SetActive(true);
            }
            else
            {
                MainObject.SetActive(false);
            }
        }
        else
        {
            MainObject.SetActive(false);
            if (oldposition != EndGridPosition)
            {
                for (int i = 0; i < PreviewObjects.Count; i++)
                {
                    GameObject.Destroy(PreviewObjects[i]);
                }

                PreviewObjects.Clear();
                //To summerize the math, This basically marches in a direction till it reaches the end position
                //First it takes the steps local to the angle of the object. A step being direction and how much it moves
                //For example: Brick with a angle of 0 moves 2 steps forwards and back, sense it's longer, and 1 step left and right
                Vector3Int Step = data.StepFromAngle(Angle);
                Vector3Int ToLine = EndGridPosition - StartGridPosition;
                //The division is there so it matches the position of the mouse
                ToLine.x = Mathf.RoundToInt((float)(ToLine.x / Step.x));
                ToLine.z = Mathf.RoundToInt((float)(ToLine.z / Step.z));
                Vector3 Normalized = ((Vector3)ToLine).normalized;
                Normalized.x = Mathf.RoundToInt(Normalized.x);
                Normalized.y = Mathf.RoundToInt(Normalized.y);
                Normalized.z = Mathf.RoundToInt(Normalized.z);
                //Marches to the line
                for (int x = 0; x <= Mathf.Abs(ToLine.x); x++)
                {
                    for (int z = 0; z <= Mathf.Abs(ToLine.z); z++)
                    {
                        GameObject Obj = GameObject.Instantiate(Prefab);
                        Obj.GetComponentInChildren<MeshRenderer>().material = Builder.Instance.TransparentMaterial;
                        Obj.transform.localScale = new Vector3(1, 1, 1);
                        Obj.transform.SetParent(WorldGen.Instance.transform);
                        Obj.transform.position = StartGridPosition + new Vector3(Step.x * (x * Normalized.x), 0, Step.z * (z * Normalized.z)) + new Vector3(0.5f, 0.5f, 0.5f);
                        Obj.transform.rotation = Quaternion.Euler(0, Angle, 0);
                        Collider[] colliders = Obj.GetComponents<Collider>();
                        for (int i = 0; i < colliders.Length; i++)
                        {
                            colliders[i].enabled = false;
                        }
                        PreviewObjects.Add(Obj);
                    }
                }

                oldposition = EndGridPosition;
            }
        }
    }
}
