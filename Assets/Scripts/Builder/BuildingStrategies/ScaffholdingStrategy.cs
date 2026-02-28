using UnityEngine.InputSystem;
using UnityEngine;

//Sense the graphics of the prefab changes dynamically. scaffholds have their own stragedy
public class ScaffholdingStrategy : IBuilderStrategy
{
    BlockAuthoringComponent data;
    GameObject Prefab;
    GameObject MainObject;

    Vector3Int GridPosition;
    Vector3Int GridStart;
    float Angle;
    bool HoldDown;
    public ScaffholdingStrategy(GameObject Prefab)
    {
        this.data = Prefab.GetComponent<BlockAuthoringComponent>();
        this.Prefab = Prefab;
        MainObject = GameObject.Instantiate(Prefab);
        MeshRenderer[] rends = MainObject.GetComponentsInChildren<MeshRenderer>();
        for (int i = 0; i < rends.Length; i++)
        {
            rends[i].material = Builder.Instance.TransparentMaterial;
        }
        MainObject.transform.SetParent(WorldGen.Instance.transform);
        MainObject.transform.GetComponent<BoxCollider>().enabled = false;
    }

    public void Dispose()
    {
        GameObject.Destroy(MainObject);
    }

    public void OnMouseDown()
    {
        HoldDown = true;
        GridStart = GridPosition;
    }

    public void OnMousePerform()
    {

    }

    public void OnMouseUp()
    {
        if (HoldDown == true)
        {
            //Scales horizontally. Something worth mentioning is not of the building have specific requirements for placement
            //Which allows them to be placed in the air
            GameObject fab = GameObject.Instantiate(Prefab, MainObject.transform.position, Quaternion.Euler(0, 0, 0));
            Vector3 scale = MainObject.transform.GetChild(0).transform.localScale;
            //Note, too big of size, too tired to care
            fab.GetComponent<BoxCollider>().size = new Vector3(scale.x, 1, scale.y);

            fab.transform.GetChild(0).localScale = MainObject.transform.GetChild(0).transform.localScale;

            fab.transform.GetChild(1).localPosition = MainObject.transform.GetChild(1).transform.localPosition;
            fab.transform.GetChild(2).localPosition = MainObject.transform.GetChild(2).transform.localPosition;
            fab.transform.GetChild(3).localPosition = MainObject.transform.GetChild(3).transform.localPosition;
            fab.transform.GetChild(4).localPosition = MainObject.transform.GetChild(4).transform.localPosition;

            int MinX = Mathf.Min(GridStart.x, GridPosition.x);
            int MinZ = Mathf.Min(GridStart.z, GridPosition.z);
            int MaxX = Mathf.Max(GridStart.x, GridPosition.x);
            int MaxZ = Mathf.Max(GridStart.z, GridPosition.z);
            for (int x = MinX; x < MaxX; x++)
            {
                for (int z = MinZ; z < MaxZ; z++)
                {
                    WorldGen.Instance.SetBlock(x, GridPosition.y, z, ChunkBlockType.Scaffholding);
                }
            }
            Component.Destroy(fab.GetComponent<BlockAuthoringComponent>());

            Vector3 Center = new Vector3(((float)GridPosition.x + (float)GridPosition.x) / 2, GridPosition.y, ((float)GridPosition.z + (float)GridPosition.z) / 2);
            Vector3 Size = new Vector3(((float)GridPosition.x - (float)GridPosition.x), 1, ((float)GridPosition.z - (float)GridPosition.z));
            MainObject.transform.position = Center + new Vector3(0.5f, 0.5f, 0.5f);
            MainObject.transform.GetChild(0).transform.localScale = new Vector3(Mathf.Abs(Size.x * 1.25f) + 1, Mathf.Abs(Size.z * 1.25f) + 1, 1);

            MainObject.transform.GetChild(1).transform.localPosition = new Vector3(Mathf.Abs(Size.x) / 2, 0.5f, -Mathf.Abs(Size.z) / 2);
            MainObject.transform.GetChild(2).transform.localPosition = new Vector3(Mathf.Abs(Size.x) / 2, 0.5f, Mathf.Abs(Size.z) / 2);
            MainObject.transform.GetChild(3).transform.localPosition = new Vector3(-Mathf.Abs(Size.x) / 2, 0.5f, Mathf.Abs(Size.z) / 2);
            MainObject.transform.GetChild(4).transform.localPosition = new Vector3(-Mathf.Abs(Size.x) / 2, 0.5f, -Mathf.Abs(Size.z) / 2);
            HoldDown = false;
        }
    }

    public void OnRotate()
    {

    }

    public void Preview()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {

            GridPosition = Vector3Int.FloorToInt(hit.point + hit.normal * 0.01f);
            if (HoldDown == false)
            {
                MainObject.transform.position = GridPosition + new Vector3(0.5f, 0.5f, 0.5f);
                MainObject.transform.rotation = Quaternion.Euler(0, Angle, 0);
                MainObject.SetActive(true);
            }
            else
            {
                //The math here is a special pain
                int MinX = Mathf.Min(GridStart.x, GridPosition.x);
                int MinZ = Mathf.Min(GridStart.z, GridPosition.z);
                int MaxX = Mathf.Max(GridStart.x, GridPosition.x);
                int MaxZ = Mathf.Max(GridStart.z, GridPosition.z);

                Vector3 Center = new Vector3(((float)MinX + (float)MaxX) / 2, GridPosition.y, ((float)MinZ + (float)MaxZ) / 2);
                Vector3 Size = new Vector3(((float)MaxX - (float)MinX), 1, ((float)MaxZ - (float)MinZ));
                MainObject.transform.position = Center + new Vector3(0.5f, 0.5f, 0.5f);
                MainObject.transform.GetChild(0).transform.localScale = new Vector3(Mathf.Abs(Size.x * 1.25f) + 1, Mathf.Abs(Size.z * 1.25f) + 1, 1);

                MainObject.transform.GetChild(1).transform.localPosition = new Vector3(Mathf.Abs(Size.x) / 2, 0.5f, -Mathf.Abs(Size.z) / 2);
                MainObject.transform.GetChild(2).transform.localPosition = new Vector3(Mathf.Abs(Size.x) / 2, 0.5f, Mathf.Abs(Size.z) / 2);
                MainObject.transform.GetChild(3).transform.localPosition = new Vector3(-Mathf.Abs(Size.x) / 2, 0.5f, Mathf.Abs(Size.z) / 2);
                MainObject.transform.GetChild(4).transform.localPosition = new Vector3(-Mathf.Abs(Size.x) / 2, 0.5f, -Mathf.Abs(Size.z) / 2);
            }
        }
        else
        {
            MainObject.SetActive(false);
        }
    }
}