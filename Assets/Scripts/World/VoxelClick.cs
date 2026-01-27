using UnityEngine;
using UnityEngine.InputSystem;

public class VoxelPicker : MonoBehaviour
{
    public Chunk chunk;
    Vector3Int? selected;

    void Update()
    {
        if (Mouse.current == null || chunk == null) return;

        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            Ray r = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(r, out RaycastHit hit, 200f))
            {
                Chunk c = hit.collider.GetComponentInParent<Chunk>();
                if (c == null) return;

                Vector3 world = hit.point - hit.normal * 0.01f;
                Vector3 local = c.transform.InverseTransformPoint(world);

                int x = Mathf.FloorToInt(local.x);
                int y = Mathf.FloorToInt(local.y);
                int z = Mathf.FloorToInt(local.z);

                if (x >= 0 && y >= 0 && z >= 0 &&
                    x < c.size && y < c.size && z < c.size)
                {
                    selected = new Vector3Int(x, y, z);
                    chunk = c;
                }
                else
                {
                    selected = null;
                }
            }

        }
    }

    void OnDrawGizmos()
    {
        if (!selected.HasValue || chunk == null) return;

        Gizmos.color = Color.yellow;
        Vector3 local = selected.Value + Vector3.one * 0.5f;
        Vector3 world = chunk.transform.TransformPoint(local);
        Gizmos.DrawWireCube(world, Vector3.one);
    }
}
