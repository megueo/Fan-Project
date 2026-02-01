using UnityEngine;
using System.Collections;

public class UnitSystem : MonoBehaviour
{
    [Header("Owner")]
    public TotemSystem ownerTotem;

    Rigidbody rb;

    void Awake()
    {
        transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = true;
        }
    }
    public void Init(TotemSystem owner)
    {
        ownerTotem = owner;

        if (ownerTotem == null)
        {
            Destroy(gameObject);
            return;
        }

        SnapToGroundVoxel();
        StartCoroutine(EnablePhysicsNextFrame());
    }

    void SnapToGroundVoxel()
    {
        Vector3 p = transform.position;

        int x = Mathf.FloorToInt(p.x);
        int z = Mathf.FloorToInt(p.z);

        if (WorldGen.Instance.TryGetGroundY(x, z, out int groundY))
        {
            transform.position = new Vector3(
                x + 0.5f,
                groundY + 2f,
                z + 0.5f
            );
        }
    }

    IEnumerator EnablePhysicsNextFrame()
    {
        yield return null;

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }
    }
}
