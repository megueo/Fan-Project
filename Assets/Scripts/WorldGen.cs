using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using Unity.AI.Navigation;

public class WorldGen : MonoBehaviour
{
    public NavMeshSurface surface;

    IEnumerator Start()
    {
        yield return null;

        surface.BuildNavMesh();

        foreach (var agent in FindObjectsByType<NavMeshAgent>(FindObjectsSortMode.None))
        {
            agent.enabled = true;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(agent.transform.position, out hit, 5f, NavMesh.AllAreas))
            {
                agent.Warp(hit.position);
            }
        }
    }
}
