using UnityEngine;
using System.Collections.Generic;

public class UnitMovement : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 5f;

    [Header("Rotation")]
    public float rotationSpeed = 12f;

    [Header("Grounding")]
    public float raycastHeight = 1.2f;
    public float raycastDistance = 2f;
    public float footOffset = 0.05f;
    public float maxStepHeight = 0.6f;

    List<Vector3> path;
    int index;

    Rigidbody rb;
    BoxCollider box;

    Vector3 groundNormal = Vector3.up;
    bool grounded;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        box = GetComponent<BoxCollider>();

        rb.useGravity = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        rb.constraints =
            RigidbodyConstraints.FreezeRotationX |
            RigidbodyConstraints.FreezeRotationZ;
    }

    public void MoveTo(Vector3 destination)
    {
        path = Pathfinder.Instance.FindPath(transform.position, destination);
        index = 0;
    }

    void FixedUpdate()
    {
        if (path != null && index < path.Count)
        {
            MoveAlongPath();
        }
        else
        {
            SolveGround();
        }
    }

    void SolveGround()
    {
        grounded = Physics.Raycast(
            rb.position + Vector3.up * raycastHeight,
            Vector3.down,
            out RaycastHit hit,
            raycastDistance,
            LayerMask.GetMask("VoxelGround")
        );

        if (grounded)
            groundNormal = hit.normal;
    }

    void MoveAlongPath()
    {
        Vector3 target = path[index];

        Vector3 toTarget = target - rb.position;

        if (toTarget.magnitude < 0.05f)
        {
            index++;
            return;
        }

        Vector3 move = toTarget.normalized * speed * Time.fixedDeltaTime;

        rb.MovePosition(rb.position + move);

        RotateTowards(move);
    }

    void RotateTowards(Vector3 direction)
    {
        if (direction.sqrMagnitude < 0.0001f)
            return;

        Vector3 flatDir = new Vector3(direction.x, 0f, direction.z).normalized;

        float targetY = Mathf.Atan2(flatDir.x, flatDir.z) * Mathf.Rad2Deg;

        Quaternion targetRot = Quaternion.Euler(
            90f,
            targetY,
            180f
        );

        rb.MoveRotation(
            Quaternion.Slerp(
                rb.rotation,
                targetRot,
                rotationSpeed * Time.fixedDeltaTime
            )
        );
    }
}
