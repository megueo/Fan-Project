using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;

public class VerticalObjectStrategy : IBuilderStrategy
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
    float Height;
    Vector2 pressed;

    public VerticalObjectStrategy(GameObject Prefab)
    {
        this.data = Prefab.GetComponent<BlockAuthoringComponent>();
        this.Prefab = Prefab;
        PreviewObjects = new List<GameObject>();
        MainObject = GameObject.Instantiate(Prefab);

        Collider[] colliders = MainObject.GetComponents<Collider>();
        for (int i = 0; i < colliders.Length; i++)
        {
            colliders[i].enabled = false;
        }
        MeshRenderer[] rends = MainObject.GetComponentsInChildren<MeshRenderer>();
        for (int i = 0; i < rends.Length; i++)
        {
            rends[i].material = Builder.Instance.TransparentMaterial;
        }
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
        pressed = Mouse.current.position.ReadValue();
    }

    public void OnMousePerform()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        //The main difference between this and the Line strategy is the vertical mouse movement
        float ratio = cam.pixelWidth / cam.pixelHeight;
        Height += (Mouse.current.position.ReadValue().y - pressed.y) * ratio * 0.01f;
        EndGridPosition = StartGridPosition + new Vector3Int(0, Mathf.RoundToInt(Height), 0);
        pressed = Mouse.current.position.ReadValue();
    }

    public void OnMouseUp()
    {
        Holding = false;
        Height = 0;
        for (int i = 0; i < PreviewObjects.Count; i++)
        {
            //Go through each preview and spawn a object with their position and angle
            GameObject fab = GameObject.Instantiate(Prefab, PreviewObjects[i].transform.position, Quaternion.Euler(0, Angle, 0));
            fab.GetComponent<BlockAuthoringComponent>().AuthorizeComponent();
        }
    }

    public void OnRotate()
    {
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
                //Go more in detail in DragLineStrategy
                Vector3Int step = data.Step;
                switch (Angle)
                {
                    case 90:
                        step = new Vector3Int(data.Step.z, data.Step.y, data.Step.x);
                        break;
                    case 180:
                        step = new Vector3Int(data.Step.x, data.Step.y, -data.Step.z);
                        break;
                    case 270:
                        step = new Vector3Int(-data.Step.z, data.Step.y, data.Step.x);
                        break;
                    default:
                        break;
                }
                Vector3Int ToLine = EndGridPosition - StartGridPosition;
                Vector3 Normalized = ((Vector3)ToLine).normalized;
                for (int y = 0; y <= Mathf.Abs(ToLine.y); y++)
                {
                    GameObject Obj = GameObject.Instantiate(Prefab);
                    Obj.GetComponentInChildren<MeshRenderer>().material = Builder.Instance.TransparentMaterial;
                    Obj.transform.localScale = new Vector3(1, 1, 1);
                    Obj.transform.SetParent(WorldGen.Instance.transform);
                    Obj.transform.position = StartGridPosition + new Vector3(step.x * (y * Normalized.y), y * Normalized.y, step.z * (y * Normalized.y)) + new Vector3(0.5f, 0.5f, 0.5f);
                    Obj.transform.rotation = Quaternion.Euler(0, Angle, 0);
                    Collider[] colliders = Obj.GetComponents<Collider>();
                    for (int i = 0; i < colliders.Length; i++)
                    {
                        colliders[i].enabled = false;
                    }
                    PreviewObjects.Add(Obj);
                }

                oldposition = EndGridPosition;
            }
        }
    }
}
