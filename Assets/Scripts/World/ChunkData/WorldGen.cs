using UnityEngine;
using System.Collections.Generic;

public class WorldGen : MonoBehaviour
{
    public static WorldGen Instance;

    public GameObject chunkPrefab;

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
    public void SetBlock(int x, int y, int z, ChunkBlockType type, byte data = 0)
    {
        Chunk chunk = GetChunkFromWorld(x, z);
        if (chunk == null) return;

        Vector3Int local = WorldToLocal(x, y, z);
        if (!InBounds(chunk, local)) return;

        chunk.voxels[local.x, local.y, local.z] = new ChunkBlock
        {
            type = type,
            data = data
        };
    }

    public Chunk GetChunk(int x, int z)
    {
        return GetChunkFromWorld(x, z);
    }

    public bool TryGetGroundY(int x, int z, out int groundY)
    {
        groundY = -1;

        Chunk chunk = GetChunkFromWorld(x, z);
        if (chunk == null) return false;

        for (int y = chunk.size - 1; y >= 0; y--)
        {
            if (IsSolid(x, y, z))
            {
                groundY = y;
                return true;
            }
        }

        return false;
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

    public List<Vector3Int> GetWalkableRing(Vector3Int center, int radius, int y)
    {
        var result = new List<Vector3Int>();

        int minX = center.x - radius;
        int maxX = center.x + radius;
        int minZ = center.z - radius;
        int maxZ = center.z + radius;

        for (int x = minX; x <= maxX; x++)
            for (int z = minZ; z <= maxZ; z++)
            {
                bool isRing = x == minX || x == maxX || z == minZ || z == maxZ;
                if (!isRing) continue;

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

    public Vector3Int WorldToLocal(int x, int y, int z)
    {
        int lx = x % chunkSize;
        int lz = z % chunkSize;
        if (lx < 0) lx += chunkSize;
        if (lz < 0) lz += chunkSize;

        return new Vector3Int(lx, y, lz);
    }

    public bool InBounds(Chunk c, Vector3Int l) =>
        l.x >= 0 && l.y >= 0 && l.z >= 0 &&
        l.x < c.size && l.y < c.size && l.z < c.size;
}
