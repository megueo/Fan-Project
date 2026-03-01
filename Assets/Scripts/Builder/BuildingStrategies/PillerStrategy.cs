using UnityEngine;
using UnityEngine.InputSystem;

//For the piller object specifically. Sense the graphic change dynamically, it as it's own stragedy
public class PillerStrategy : IBuilderStrategy
{
    BlockAuthoringComponent data;
    GameObject Prefab;
    GameObject MainObject;

    Vector3Int GridPosition;
    Vector3Int StartGridPosition;
    Vector3Int EndGridPosition;
    Vector3Int oldposition;
    bool Holding;
    float Height;
    Vector2 pressed;

    public PillerStrategy(GameObject Prefab)
    {
        this.data = Prefab.GetComponent<BlockAuthoringComponent>();
        this.Prefab = Prefab;
        MainObject = GameObject.Instantiate(Prefab);
        MainObject.GetComponent<BoxCollider>().enabled = false;
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

        //I will have to fix a issue that's in both the Piller and VerticalObjectStrategy. And that the vertical value of the mouse doesn't match with the zoom
        //So objects don't quite match the mouse at farther distances
        float ratio = cam.pixelWidth / cam.pixelHeight;
        Height += (Mouse.current.position.ReadValue().y - pressed.y) * ratio * 0.01f;
        EndGridPosition = StartGridPosition + new Vector3Int(0, Mathf.RoundToInt(Height), 0);
        pressed = Mouse.current.position.ReadValue();
    }

    public void OnMouseUp()
    {
        //Places the object. Alot of the math is a pain
        GameObject fab = GameObject.Instantiate(Prefab, MainObject.transform.position, Quaternion.Euler(0, 0, 0));
        Vector3 scale = MainObject.transform.GetChild(2).transform.localScale;
        fab.GetComponent<BoxCollider>().center = new Vector3(0, ((float)Mathf.RoundToInt(Height) / 2), 0);
        fab.GetComponent<BoxCollider>().size = new Vector3(1, Mathf.RoundToInt(Height) + 1, 1);

        fab.transform.GetChild(0).localPosition = MainObject.transform.GetChild(0).transform.localPosition;
        fab.transform.GetChild(1).localPosition = MainObject.transform.GetChild(1).transform.localPosition;
        fab.transform.GetChild(2).localScale = MainObject.transform.GetChild(2).transform.localScale;
        fab.transform.GetChild(2).localPosition = MainObject.transform.GetChild(2).transform.localPosition;

        int MinY = Mathf.Min(StartGridPosition.y, EndGridPosition.y);
        int MaxY = Mathf.Max(StartGridPosition.y, EndGridPosition.y);
        for (int i = MinY; i < MaxY; i++)
        {
            WorldGen.Instance.SetBlock(GridPosition.x, i, GridPosition.z, ChunkBlockType.Piller);
        }
        Component.Destroy(fab.GetComponent<BlockAuthoringComponent>());

        MainObject.transform.position = StartGridPosition + new Vector3(0.5f, 0.5f, 0.5f);
        MainObject.transform.GetChild(0).localPosition = new Vector3(0, 0, 0);
        MainObject.transform.GetChild(1).localPosition = new Vector3(0, 0, 0);

        MainObject.transform.GetChild(2).transform.localScale = new Vector3(0.9f, ((float)Mathf.RoundToInt(0) + 1) * 0.5f, 0.9f);
        MainObject.transform.GetChild(2).transform.localPosition = new Vector3(0, ((float)Mathf.RoundToInt(0) / 2), 0);
        Holding = false;
        Height = 0;

    }

    public void OnRotate()
    {

    }

    public void Preview()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        if (Holding == false)
        {
            Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            {

                GridPosition = Vector3Int.FloorToInt(hit.point + hit.normal * 0.01f);
                MainObject.transform.position = GridPosition + new Vector3(0.5f, 0.5f, 0.5f);
                MainObject.SetActive(true);
            }
            else
            {
                MainObject.SetActive(false);
            }
        }
        else
        {
            MainObject.SetActive(true);
            if (oldposition != EndGridPosition)
            {
                int MinY = Mathf.Min(StartGridPosition.y, EndGridPosition.y);
                int MaxY = Mathf.Max(StartGridPosition.y, EndGridPosition.y);
                Vector3 Size = new Vector3(1, ((float)MaxY - (float)MinY), 1);

                MainObject.transform.position = StartGridPosition + new Vector3(0.5f, 0.5f, 0.5f);
                MainObject.transform.GetChild(0).localPosition = new Vector3(0, Size.y, 0);
                MainObject.transform.GetChild(1).localPosition = new Vector3(0, 0, 0);

                MainObject.transform.GetChild(2).transform.localScale = new Vector3(0.9f, ((float)Mathf.RoundToInt(Height) + 1) * 0.5f, 0.9f);
                MainObject.transform.GetChild(2).transform.localPosition = new Vector3(0, ((float)Mathf.RoundToInt(Height) / 2), 0);

                oldposition = EndGridPosition;
            }
        }
    }
}
