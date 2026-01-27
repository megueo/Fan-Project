using UnityEngine;

public class Chunk : MonoBehaviour
{
    public int size = 16; //The big cube (which is made from the units) is 16x16x16 in all dimensions
    public Block[,,] voxels; //That bool stores a tridimensional shape, it contains 3 dimensions (represents a cube)  

    ChunkMesher mesher;

    void Awake()
    {
        voxels = new Block[size, size, size]; //Apply the size above in the bool (x, y and z)

        int groundHeight = size / 2;

        for (int x = 0; x < size; x++) //Render the cubes in X 
            for (int y = 0; y < size; y++)//Render the cubes in Y 
                for (int z = 0; z < size; z++) //Render the cubes in Z
                {
                    BlockType type;

                    if (y == groundHeight - 1)
                        type = BlockType.Grass; // Top block
                    else if (y < groundHeight - 1)
                        type = BlockType.Dirt; // Underground
                    else
                        type = BlockType.Air; // Above ground

                    voxels[x, y, z] = new Block { type = type };
                }

        mesher = GetComponent<ChunkMesher>();
    }

    public bool InBounds(Vector3Int p)
    {
        return p.x >= 0 && p.y >= 0 && p.z >= 0 &&
               p.x < size && p.y < size && p.z < size;
    }

    public void Rebuild()
    {
        if (mesher != null)
            mesher.Build();
    }
}
