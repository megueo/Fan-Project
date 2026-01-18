using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

// These lines have assignment levels to ensure they are in the code, these predefined components create the mesh (the geometrical shape the Unity draws)
[RequireComponent(typeof(Chunk))] 
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshCollider))]

public class ChunkMesher : MonoBehaviour
{
    Chunk chunk;
    MeshFilter mf;
    MeshCollider mc;

    // Now that part is kinda "computer graphics", to create a world shape for the cube voxels

    static readonly Vector3[] cubeVerts = { // The vertices/edges of the cubes (8 in total)
        new(0,0,0), // 0
        new(1,0,0), // 1
        new(1,1,0), // 2
        new(0,1,0), // 3
        new(0,0,1), // 4
        new(1,0,1), // 5
        new(1,1,1), // 6
        new(0,1,1) // 7
    };

    static readonly int[][] faces = { // Faces of the cubes (6 in total)
        new[]{0,1,2,3}, //front 
        new[]{5,4,7,6}, // back
        new[]{4,0,3,7}, // left
        new[]{1,5,6,2}, // right 
        new[]{3,2,6,7}, // upper
        new[]{4,5,1,0}, //lower
    };

    static readonly Vector3Int[] dirs = { // Voxel unit directions 
        new(0,0,-1), // front
        new(0,0,1),// back
        new(-1,0,0), //left
        new(1,0,0), // right
        new(0,1,0), // upper
        new(0,-1,0) // lower
    }; // "Why we need directions?" answer: for optimization, We don't need to render EVERY face of the cubes, only the ones we can view 

    void Start()
    {
        chunk = GetComponent<Chunk>();
        mf = GetComponent<MeshFilter>();
        mc = GetComponent<MeshCollider>();

        Build();
    }

    void Build()
    {
        var verts = new List<Vector3>(); // Contains all the vertices on the mesh
        var tris = new List<int>(); // Store the index for each vertice (the "verts" var)

        int size = chunk.size; 

        for (int x = 0; x < size; x++) // Voxels in X
            for (int y = 0; y < size; y++) // Voxels in Y
                for (int z = 0; z < size; z++) // Voxels in Z
                {
                    if (!chunk.voxels[x, y, z]) continue; // It iterates through all voxels, If the voxel is false (air), it ignores it. ( read "Chunck.cs" to see the "air" explanation :D )

                    for (int f = 0; f < 6; f++) // For each solid block, look at its 6 faces and calculate its neighbor in that direction
                    {
                        Vector3Int n = dirs[f];
                        int nx = x + n.x; // neightbor cube in X
                        int ny = y + n.y; // neightbor cube in Y
                        int nz = z + n.z; // neightbor cube in Z

                        bool neighborSolid =
                            nx >= 0 && ny >= 0 && nz >= 0 &&
                            nx < size && ny < size && nz < size &&
                            chunk.voxels[nx, ny, nz]; //If there is a solid (we can view) neighbor, do not draw the face

                        if (neighborSolid) continue; // It doesn't draw a face that the player can't see

                        int v = verts.Count;
                        foreach (int i in faces[f])
                            verts.Add(new Vector3(x, y, z) + cubeVerts[i]); // Add the 4 vertices of the face, shifted to the (x,y,z) position of the block

                        // Transform the square into two triangles (our GPU sees the square and other shapes/polygons as triangles, the simplest polygon)
                        tris.Add(v + 0); tris.Add(v + 1); tris.Add(v + 2);
                        tris.Add(v + 0); tris.Add(v + 2); tris.Add(v + 3);
                    }
                }

        // Create the Mesh, apply it to the MeshFilter (visual) and to the MeshCollider(collision)
        Mesh m = new Mesh();
        m.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        m.SetVertices(verts);
        m.SetTriangles(tris, 0);
        m.RecalculateNormals();

        mf.mesh = m;
        mc.sharedMesh = m;
    }
}
