using UnityEngine;

public class Chunk : MonoBehaviour
{
    public int size = 16; //The big cube (which is made from the units) is 16x16x16 in all dimensions
    public bool[,,] voxels; //That bool stores a tridimensional shape, it contains 3 dimensions (represents a cube)  

    void Awake()
    {
        voxels = new bool[size, size, size]; //Apply the size above in the bool (x, y and z)

        for (int x = 0; x < size; x++) //Render the cubes in X 
            for (int y = 0; y < size; y++)//Render the cubes in Y 
                for (int z = 0; z < size; z++) //Render the cubes in Z
                {
                    voxels[x, y, z] = (y < size / 2); //If the cube is above 0 to 7, it's "solid", if it's 8 to 15, it's "air" (there will be changes in that)
                }
    }
}
