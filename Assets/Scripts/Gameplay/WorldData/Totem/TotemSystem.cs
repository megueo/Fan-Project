using System.Collections.Generic;
using UnityEngine;

public class TotemSystem : MonoBehaviour
{
    [Header("Unit Spawn Settings")]
    public GameObject unitPrefab;
    public float spawnInterval = 5f;
    public int maxAliveUnits = 5;
    public int spawnRadius = 3;

    private Vector3Int gridPos;
    private float timer;
    private int aliveUnits;

    void Awake()
    {
        if (!WorldGen.Instance.TryGetGroundY(
            Mathf.RoundToInt(transform.position.x),
            Mathf.RoundToInt(transform.position.z),
            out int groundY))
        {
            groundY = Mathf.RoundToInt(transform.position.y);
        }

        gridPos = new Vector3Int(
            Mathf.RoundToInt(transform.position.x),
            groundY,
            Mathf.RoundToInt(transform.position.z)
        );
    }

    void Update()
    {
        if (aliveUnits >= maxAliveUnits) return;

        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer = 0f;
            TrySpawnUnit();
        }
    }

    void TrySpawnUnit()
    {
        if (unitPrefab == null) return;

        int baseX = Mathf.RoundToInt(transform.position.x);
        int baseZ = Mathf.RoundToInt(transform.position.z);

        for (int attempt = 0; attempt < 10; attempt++)
        {
            Vector2 offset = Random.insideUnitCircle * spawnRadius;
            int x = baseX + Mathf.RoundToInt(offset.x);
            int z = baseZ + Mathf.RoundToInt(offset.y);

            if (!WorldGen.Instance.TryGetGroundY(x, z, out int groundY))
                continue;

            int spawnY = groundY + 1;

            if (!WorldGen.Instance.IsWalkable(x, spawnY, z))
                continue;

            Vector3 spawnPos = new Vector3(
                x + 0.5f,
                spawnY,
                z + 0.5f
            );

            GameObject unitGO = Instantiate(unitPrefab, spawnPos, Quaternion.identity);

            UnitSystem unit = unitGO.GetComponent<UnitSystem>();
            if (unit == null)
            {
                Destroy(unitGO);
                return;
            }

            unit.Init(this);
            aliveUnits++;

            return;
        }
    }

    public void NotifyUnitDeath()
    {
        aliveUnits = Mathf.Max(0, aliveUnits - 1);
    }
}
