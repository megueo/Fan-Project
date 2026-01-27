using UnityEngine;
using System.Collections.Generic;

public class UnitMovement : MonoBehaviour
{
    public float speed = 5f;

    List<Vector3> path;
    int index;

    public void MoveTo(Vector3 destination)
    {
        Debug.Log($"[UnitMovement] MoveTo called: {destination}");

        Vector3 start = transform.position;

        // ⚠️ SEM criar variável local
        path = Pathfinder.Instance.FindPath(start, destination);
        index = 0;

        if (path == null || path.Count == 0)
        {
            Debug.LogWarning("[UnitMovement] Path is null or empty!");
            return;
        }

        Debug.Log($"[UnitMovement] Path received with {path.Count} nodes");
    }

    void Update()
    {
        if (path == null || path.Count == 0) return;
        if (index >= path.Count) return;

        Vector3 target = path[index];
        Vector3 dir = target - transform.position;
        dir.y = 0;

        if (dir.magnitude < 0.1f)
        {
            index++;
            Debug.Log($"[UnitMovement] Reached node {index}");
            return;
        }

        transform.position += dir.normalized * speed * Time.deltaTime;
    }

    void OnDrawGizmos()
    {
        if (path == null || path.Count == 0) return;
        if (index >= path.Count) return;

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(path[index], 0.2f);

        Gizmos.color = Color.yellow;
        for (int i = 0; i < path.Count - 1; i++)
        {
            Gizmos.DrawLine(path[i], path[i + 1]);
            Gizmos.DrawSphere(path[i], 0.1f);
        }
    }
}
