using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

//General interface for different ways of placing stuff, In a straight line, or drag over a wide field, or replace a blocktype within a sphere
public interface IBuilderStragedy
{
    public void Dispose();
    public void OnMouseDown();
    public void OnMousePerform();
    public void OnMouseUp();
    public void OnPrefabChange();
    public void Preview();
}

//Gonna make this sometime soon, This is more for bricks or thing's that are suppose to go in a straight line - Lonely
public class DragLineStragedy : IBuilderStragedy
{
    public void Dispose()
    {

    }

    public void OnMouseDown()
    {

    }

    public void OnMousePerform()
    {
        
    }

    public void OnMouseUp()
    {

    }

    public void OnPrefabChange()
    {
        
    }

    public void Preview()
    {

    }
}

//Meant for landscaping, I'll likely add more landscaping tools meant for islands
public class SphereLandscapeStragedy : IBuilderStragedy
{
    GameObject SphereGraphic;
    public BlockType type;
    Vector3Int GridPosition;
    float t;

    public SphereLandscapeStragedy(BlockType type)
    {
        this.type = type;
        if(this.SphereGraphic == null)
        {
            this.SphereGraphic = new GameObject();
            this.SphereGraphic.AddComponent<MeshFilter>().mesh = Builder.Instance.SphereGraphic;
            this.SphereGraphic.AddComponent<MeshRenderer>().material = Builder.Instance.TransparentMaterial;
            this.SphereGraphic.transform.localScale = new Vector3(9, 9, 9);
            this.SphereGraphic.transform.SetParent(WorldGen.Instance.transform);
        }
    }

    public void Dispose()
    {
        GameObject.Destroy(SphereGraphic);
    }

    public void OnMouseDown()
    {
        
    }

    public void OnMousePerform()
    {
        if (SphereGraphic.activeSelf == false) return;

        t += Time.deltaTime;
        if (t >= 0.1f)
        {
            HashSet<Chunk> rebuilt = new();
            //Loop through the current target position in a square order(bottom, back, left. To top, forward, right)
            for (int x = GridPosition.x - 5; x <= GridPosition.x + 5; x++)
            {
                for (int y = GridPosition.y - 5; y <= GridPosition.y + 5; y++)
                {
                    for (int z = GridPosition.z - 5; z <= GridPosition.z + 5; z++)
                    {
                        //Is this inside the sphere? if so, change the block
                        if (Vector3.Distance(GridPosition, new Vector3(x, y, z)) <= 4)
                        {
                            WorldGen.Instance.SetBlock(x, y, z, type);
                        }

                        //Get any chunk that's effected, the distance of the sphere is slightly smaller then the cube of blocks
                        //this is so we can get chunks that while don't have any blocks that change, they do have a block right on their border that
                        //as changed. Doing this makes sure there won't be a hole in the world
                        Chunk chunk = WorldGen.Instance.GetChunk(x, z);
                        if (!rebuilt.Contains(chunk))
                            if (chunk != null && !rebuilt.Contains(chunk))
                                rebuilt.Add(chunk);
                    }
                }
            }

            //There's likely a better way to rebuild the chunk. Maybe there should be additional code in the world gen
            //A function that adds any changed chunks to a list or whatever. and a update function that 
            //goes through the list and updates the mesh
            foreach (var chunk in rebuilt)
            {
                chunk.Rebuild();
            }

            t = 0;
        }
    }

    public void OnMouseUp()
    {
        t = 0;
    }

    public void OnPrefabChange()
    {

    }

    //For the preview of objects
    public void Preview()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            GridPosition = Vector3Int.FloorToInt(hit.point);
            SphereGraphic.transform.position = GridPosition;
            SphereGraphic.SetActive(true);
        }
        else
        {
            SphereGraphic.SetActive(false);
        }
    }
}