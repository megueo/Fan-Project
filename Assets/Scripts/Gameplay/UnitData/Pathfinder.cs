using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class Pathfinder : MonoBehaviour
{
    public static Pathfinder Instance;

    public bool debugVisible = false;

    List<Vector3> lastDebugPath;
    List<Vector3Int> orderedPath = new();
    Dictionary<Vector3Int, Node> debugNodes;

    HashSet<Vector3Int> finalPathVoxels = new();
    static HashSet<Vector3Int> reservedTargets = new();

    Vector3Int debugStart;
    Vector3Int debugEnd;

    void Awake()
    {
        Instance = this;
    }

    public static bool TryReserve(Vector3Int voxel)
    {
        if (reservedTargets.Contains(voxel))
            return false;

        reservedTargets.Add(voxel);
        return true;
    }

    public static void Release(Vector3Int voxel)
    {
        reservedTargets.Remove(voxel);
    }

    public List<Vector3> FindPath(Vector3 startWorld, Vector3 endWorld)
    {
        Vector3Int start = SnapToWalkable(startWorld);
        Vector3Int end = SnapToWalkable(endWorld);

        debugStart = start;
        debugEnd = end;

        if (start.y < 0 || end.y < 0)
            return null;

        if (!TryReserve(end))
            return null;

        var open = new List<Node>();
        var closed = new HashSet<Vector3Int>();

        debugNodes = new Dictionary<Vector3Int, Node>();
        finalPathVoxels.Clear();
        orderedPath.Clear();

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
                BuildOrderedPath(current);
                BuildFinalDebugPath();
                lastDebugPath = BuildWorldPath();
                return lastDebugPath;
            }

            closed.Add(current.pos);

            foreach (var n in GetNeighbors(current.pos))
            {
                if (closed.Contains(n)) continue;
                if (!WorldGen.Instance.IsWalkable(n.x, n.y, n.z)) continue;

                bool diagonal = n.x != current.pos.x && n.z != current.pos.z;
                float cost = diagonal ? 1.4142f : 1f;

                int dy = n.y - current.pos.y;
                if (dy > 0)
                    cost += 0.5f;

                float g = current.G + cost;

                if (!debugNodes.TryGetValue(n, out Node existing))
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

        Release(end);
        return null;
    }

    void BuildOrderedPath(Node node)
    {
        while (node != null)
        {
            orderedPath.Add(node.pos);
            node = node.parent;
        }
        orderedPath.Reverse();
    }

    void BuildFinalDebugPath()
    {
        for (int i = 0; i < orderedPath.Count; i++)
        {
            finalPathVoxels.Add(orderedPath[i]);

            if (i > 0)
                AddDiagonalFill(orderedPath[i - 1], orderedPath[i]);
        }
    }

    void AddDiagonalFill(Vector3Int from, Vector3Int to)
    {
        int dx = to.x - from.x;
        int dz = to.z - from.z;

        if (Mathf.Abs(dx) == 1 && Mathf.Abs(dz) == 1)
        {
            finalPathVoxels.Add(new Vector3Int(from.x + dx, from.y, from.z));
            finalPathVoxels.Add(new Vector3Int(from.x, from.y, from.z + dz));
        }
    }

    List<Vector3> BuildWorldPath()
    {
        var list = new List<Vector3>();
        foreach (var v in orderedPath)
            list.Add(GridToWorld(v));
        return list;
    }

    void OnDrawGizmos()
    {
#if UNITY_EDITOR
        if (!debugVisible || debugNodes == null) return;

        foreach (var v in finalPathVoxels)
            DrawTile(v, new Color(0, 1, 0, 0.25f), Color.green);

        foreach (var kv in debugNodes)
        {
            if (finalPathVoxels.Contains(kv.Key)) continue;
            DrawTile(kv.Key, new Color(1, 1, 1, 0.08f), Color.white);
        }

        foreach (var kv in debugNodes)
        {
            Vector3 world = new Vector3(
                kv.Key.x + 0.5f,
                kv.Key.y + 0.01f,
                kv.Key.z + 0.5f
            );
            Node n = kv.Value;

            Handles.Label(
                world + Vector3.up * 0.02f,
                $"F:{n.F:F1}",
                EditorStyles.whiteMiniLabel
            );
        }
#endif
    }

    void DrawTile(Vector3Int v, Color fill, Color outline)
    {
#if UNITY_EDITOR
        Vector3 w = new Vector3(
            v.x + 0.5f,
            v.y + 0.01f,
            v.z + 0.5f
        );

        Handles.DrawSolidRectangleWithOutline(
            new Vector3[]
            {
                w + new Vector3(-0.45f, 0.01f, -0.45f),
                w + new Vector3(-0.45f, 0.01f,  0.45f),
                w + new Vector3( 0.45f, 0.01f,  0.45f),
                w + new Vector3( 0.45f, 0.01f, -0.45f),
            },
            fill,
            outline
        );
#endif
    }
  
    float Heuristic(Vector3Int a, Vector3Int b)
        => Vector3Int.Distance(a, b);

    Vector3 GridToWorld(Vector3Int g)
        => new Vector3(g.x + 0.5f, g.y + 1f, g.z + 0.5f);

    bool IsValidStep(Vector3Int from, Vector3Int to)
    {
        if (!WorldGen.Instance.IsWalkable(to.x, to.y, to.z))
            return false;

        if (WorldGen.Instance.IsSolid(to.x, to.y + 1, to.z))
            return false;

        int dy = to.y - from.y;

        if (Mathf.Abs(dy) > 1)
            return false;

        if (to.x != from.x && to.z != from.z)
        {
            if (!WorldGen.Instance.IsWalkable(to.x, from.y, from.z)) return false;
            if (!WorldGen.Instance.IsWalkable(from.x, from.y, to.z)) return false;
        }

        return true;
    }

    IEnumerable<Vector3Int> GetNeighbors(Vector3Int p)
    {
        for (int x = -1; x <= 1; x++)
            for (int z = -1; z <= 1; z++)
            {
                if (x == 0 && z == 0) continue;

                for (int y = -1; y <= 1; y++)
                {
                    Vector3Int n = new(
                        p.x + x,
                        p.y + y,
                        p.z + z
                    );

                    if (!IsValidStep(p, n))
                        continue;

                    yield return n;
                }
            }
    }

    Vector3Int SnapToWalkable(Vector3 world)
    {
        int gx = Mathf.FloorToInt(world.x);
        int gz = Mathf.FloorToInt(world.z);

        for (int y = WorldGen.Instance.chunkSize * 2; y >= 0; y--)
            if (WorldGen.Instance.IsWalkable(gx, y, gz))
                return new Vector3Int(gx, y, gz);

        return new Vector3Int(gx, -1, gz);
    }

    public void ShowDebug() => debugVisible = true;
    public void HideDebug() => debugVisible = false;

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