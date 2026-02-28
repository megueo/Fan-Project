
using UnityEngine.InputSystem;
using UnityEngine;

//Placement of single objects, fairly simple
public class SingleObjectStrategy : IBuilderStrategy
{
    BlockAuthoringComponent data;
    GameObject Prefab;
    GameObject MainObject;

    Vector3Int GridPosition;
    float Angle;

    public SingleObjectStrategy(GameObject Prefab)
    {
        this.data = Prefab.GetComponent<BlockAuthoringComponent>();
        this.Prefab = Prefab;
        MainObject = GameObject.Instantiate(Prefab);
        MainObject.GetComponentInChildren<MeshRenderer>().material = Builder.Instance.TransparentMaterial;
        MainObject.transform.SetParent(WorldGen.Instance.transform);
        //Oh yeah, sense they have colliders when we place them, we gotta shut them off for the preview. this along with the rendering is inconsistent between strategies
        //Sometime I'll find a way to merge it in a single void
        Collider[] colliders = MainObject.GetComponents<Collider>();
        for (int i = 0; i < colliders.Length; i++)
        {
            colliders[i].enabled = false;
        }
    }

    public void Dispose()
    {
        GameObject.Destroy(MainObject);
    }

    public void OnMouseDown()
    {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (!Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            return;
        }
        GameObject fab = GameObject.Instantiate(Prefab, GridPosition + new Vector3(0.5f, 0.5f, 0.5f), Quaternion.Euler(0, Angle, 0));
        fab.GetComponent<BlockAuthoringComponent>().AuthorizeComponent();
    }

    public void OnMousePerform()
    {
    }

    public void OnMouseUp()
    {

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
}