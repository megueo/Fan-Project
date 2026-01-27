using UnityEngine;
using System.Collections.Generic;

public class WorldGen : MonoBehaviour
{
    public static WorldGen Instance;

    public GameObject chunkPrefab;
    public GameObject totemPrefab;
    public GameObject unitPrefab;

    public int worldSizeX = 4;
    public int worldSizeZ = 4;
    public int chunkSize = 16;

    Dictionary<Vector2Int, Chunk> chunks = new();

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        for (int x = 0; x < worldSizeX; x++)
            for (int z = 0; z < worldSizeZ; z++)
            {
                Vector3 pos = new(x * chunkSize, 0, z * chunkSize);
                var go = Instantiate(chunkPrefab, pos, Quaternion.identity, transform);
                chunks[new Vector2Int(x, z)] = go.GetComponent<Chunk>();
            }
    }

    public void SetBlock(int x, int y, int z, BlockType type, byte data = 0)
    {
        Chunk chunk = GetChunkFromWorld(x, z);
        if (chunk == null) return;

        Vector3Int local = WorldToLocal(x, y, z);
        if (!InBounds(chunk, local)) return;

        chunk.voxels[local.x, local.y, local.z] = new Block
        {
            type = type,
            data = data
        };
    }

    public bool IsSolid(int x, int y, int z)
    {
        Chunk chunk = GetChunkFromWorld(x, z);
        if (chunk == null) return false;

        Vector3Int local = WorldToLocal(x, y, z);
        if (!InBounds(chunk, local)) return false;

        return chunk.voxels[local.x, local.y, local.z].IsSolid;
    }

    public bool IsWalkable(int x, int y, int z)
    {
        return IsSolid(x, y - 1, z)
            && !IsSolid(x, y, z)
            && !IsSolid(x, y + 1, z);
    }

    public void PlaceTotem(Vector3Int pos)
    {
        if (!IsSolid(pos.x, pos.y - 1, pos.z))
            return;

        int width = 2;
        int depth = 2;
        int height = 3;

        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                for (int z = 0; z < depth; z++)
                {
                    if (IsSolid(pos.x + x, pos.y + y, pos.z + z))
                        return;
                }

        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                for (int z = 0; z < depth; z++)
                {
                    bool isTop = (y == height - 1);

                    SetBlock(
                        pos.x + x,
                        pos.y + y,
                        pos.z + z,
                        isTop ? BlockType.TotemTop : BlockType.TotemBase
                    );
                }

        HashSet<Chunk> rebuilt = new();

        for (int x = 0; x < width; x++)
            for (int z = 0; z < depth; z++)
            {
                Chunk chunk = GetChunkFromWorld(pos.x + x, pos.z + z);
                if (chunk != null && rebuilt.Add(chunk))
                    chunk.Rebuild();
            }

        Vector3 center = pos + new Vector3(
            width * 0.5f,
            height * 0.8f,
            depth * 0.5f
        );

        Instantiate(
            totemPrefab,
            center,
            Quaternion.Euler(90, 0, 0),
            transform
        );

        TotemSystem.Instance.RegisterTotem(pos);
    }

    public List<Vector3Int> GetPositionsAroundTotem(Vector3Int pos, int width, int depth)
    {
        var result = new List<Vector3Int>();

        int gap = 1;
        int distance = 1;

        int minX = pos.x - gap - distance;
        int maxX = pos.x + width - 1 + gap + distance;
        int minZ = pos.z - gap - distance;
        int maxZ = pos.z + depth - 1 + gap + distance;

        int innerMinX = pos.x - gap;
        int innerMaxX = pos.x + width - 1 + gap;
        int innerMinZ = pos.z - gap;
        int innerMaxZ = pos.z + depth - 1 + gap;

        int y = pos.y;

        for (int x = minX; x <= maxX; x++)
            for (int z = minZ; z <= maxZ; z++)
            {
                if (x >= innerMinX && x <= innerMaxX &&
                    z >= innerMinZ && z <= innerMaxZ)
                    continue;

                bool isRing =
                    x == minX || x == maxX ||
                    z == minZ || z == maxZ;

                if (!isRing)
                    continue;

                if (IsWalkable(x, y, z))
                    result.Add(new Vector3Int(x, y, z));
            }

        return result;
    }

    Chunk GetChunkFromWorld(int x, int z)
    {
        int cx = Mathf.FloorToInt((float)x / chunkSize);
        int cz = Mathf.FloorToInt((float)z / chunkSize);

        chunks.TryGetValue(new Vector2Int(cx, cz), out var chunk);
        return chunk;
    }

    Vector3Int WorldToLocal(int x, int y, int z)
    {
        int lx = x % chunkSize;
        int lz = z % chunkSize;
        if (lx < 0) lx += chunkSize;
        if (lz < 0) lz += chunkSize;

        return new Vector3Int(lx, y, lz);
    }

    bool InBounds(Chunk c, Vector3Int l) =>
        l.x >= 0 && l.y >= 0 && l.z >= 0 &&
        l.x < c.size && l.y < c.size && l.z < c.size;
}
