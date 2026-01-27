using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class Pathfinder : MonoBehaviour
{
    public static Pathfinder Instance;

    List<Vector3> lastDebugPath;
    Dictionary<Vector3Int, Node> debugNodes;

    void Awake()
    {
        Instance = this;
    }

    public List<Vector3> FindPath(Vector3 startWorld, Vector3 endWorld)
    {
        Debug.Log($"[Pathfinder] FindPath start={startWorld} end={endWorld}");

        Vector3Int start = SnapToWalkable(startWorld);
        Vector3Int end = SnapToWalkable(endWorld);

        Debug.Log($"[Pathfinder] Grid start={start} end={end}");

        if (start.y < 0 || end.y < 0)
        {
            Debug.LogWarning("[Pathfinder] Start or End not walkable!");
            return null;
        }

        var open = new List<Node>();
        var closed = new HashSet<Vector3Int>();
        debugNodes = new Dictionary<Vector3Int, Node>();

        Node startNode = new Node(start, null, 0, Heuristic(start, end));
        open.Add(startNode);
        debugNodes[start] = startNode;

        while (open.Count > 0)
        {
            open.Sort((a, b) => a.F.CompareTo(b.F));
            Node current = open[0];
            open.RemoveAt(0);

            if (current.pos == end)
            {
                lastDebugPath = Reconstruct(current);
                Debug.Log($"[Pathfinder] Path found! Length={lastDebugPath.Count}");
                return lastDebugPath;
            }

            closed.Add(current.pos);

            foreach (var n in GetNeighbors(current.pos))
            {
                if (closed.Contains(n)) continue;
                if (!WorldGen.Instance.IsWalkable(n.x, n.y, n.z)) continue;

                float g = current.G + Vector3Int.Distance(current.pos, n);

                Node existing = open.Find(o => o.pos == n);
                if (existing == null)
                {
                    Node node = new Node(n, current, g, Heuristic(n, end));
                    open.Add(node);
                    debugNodes[n] = node;
                }
                else if (g < existing.G)
                {
                    existing.parent = current;
                    existing.G = g;
                }
            }
        }

        Debug.LogWarning("[Pathfinder] No path found!");
        lastDebugPath = null;
        return null;
    }

    void OnDrawGizmos()
    {
        if (lastDebugPath != null)
        {
            Gizmos.color = Color.cyan;
            for (int i = 0; i < lastDebugPath.Count - 1; i++)
            {
                Gizmos.DrawSphere(lastDebugPath[i], 0.15f);
                Gizmos.DrawLine(lastDebugPath[i], lastDebugPath[i + 1]);
            }
        }

#if UNITY_EDITOR
        if (debugNodes == null) return;

        foreach (var kv in debugNodes)
        {
            Vector3 world = GridToWorld(kv.Key);
            Node n = kv.Value;

            Handles.Label(
                world + Vector3.up * 0.3f,
                $"G:{n.G:F1}\nH:{n.H:F1}\nF:{n.F:F1}"
            );
        }
#endif
    }

    List<Vector3> Reconstruct(Node node)
    {
        var path = new List<Vector3>();
        while (node != null)
        {
            path.Add(GridToWorld(node.pos));
            node = node.parent;
        }
        path.Reverse();
        return path;
    }

    float Heuristic(Vector3Int a, Vector3Int b)
    {
        return Vector3Int.Distance(a, b);
    }

    Vector3 GridToWorld(Vector3Int g)
    {
        return g + Vector3.one * 0.5f;
    }

    IEnumerable<Vector3Int> GetNeighbors(Vector3Int p)
    {
        for (int x = -1; x <= 1; x++)
            for (int z = -1; z <= 1; z++)
            {
                if (x == 0 && z == 0) continue;
                yield return new Vector3Int(p.x + x, p.y, p.z + z);
            }
    }

    Vector3Int SnapToWalkable(Vector3 world)
    {
        int gx = Mathf.FloorToInt(world.x);
        int gz = Mathf.FloorToInt(world.z);

        for (int y = WorldGen.Instance.chunkSize * 2; y >= 0; y--)
        {
            if (WorldGen.Instance.IsWalkable(gx, y, gz))
            {
                Debug.Log($"[SnapToWalkable] Found walkable at ({gx},{y},{gz})");
                return new Vector3Int(gx, y, gz);
            }
        }

        Debug.LogWarning($"[SnapToWalkable] No walkable found at column x={gx} z={gz}");
        return new Vector3Int(gx, -1, gz);
    }

    class Node
    {
        public Vector3Int pos;
        public Node parent;
        public float G;
        public float H;
        public float F => G + H;

        public Node(Vector3Int p, Node parent, float g, float h)
        {
            pos = p;
            this.parent = parent;
            G = g;
            H = h;
        }
    }
}
