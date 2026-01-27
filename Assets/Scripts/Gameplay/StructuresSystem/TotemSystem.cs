using System.Collections.Generic;
using UnityEngine;

// Future idea for that script: If the Unit dies, the Totem will decrement the number of alive Units

public class TotemSystem
{
    public static TotemSystem Instance = new();
    public struct TotemData
    {
        public Vector3Int pos;
        public float timer;
        public int aliveUnits;
    }

    List<TotemData> totems = new();

    public float spawnInterval = 5f;

    public void RegisterTotem(Vector3Int pos)
    {
        totems.Add(new TotemData
        {
            pos = pos,
            timer = 0f
        });
    }

    public void Tick(float dt)
    {
        int maxAlivePerTotem = 5;

        for (int i = 0; i < totems.Count; i++)
        {
            var t = totems[i];

            if (t.aliveUnits >= maxAlivePerTotem)
                continue;

            t.timer += dt;

            if (t.timer >= spawnInterval)
            {
                t.timer = 0f;
                int spawned = SpawnUnits(t.pos, maxAlivePerTotem - t.aliveUnits);
                t.aliveUnits += spawned;
            }

            totems[i] = t;
        }
    }

    void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int j = Random.Range(i, list.Count);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    int SpawnUnits(Vector3Int pos, int maxToSpawn)
    {
        int width = 2;
        int depth = 2;

        var spots = WorldGen.Instance
            .GetPositionsAroundTotem(pos, width, depth);

        if (spots.Count == 0)
            return 0;

        Shuffle(spots);

        int spawned = 0;

        for (int i = 0; i < Mathf.Min(maxToSpawn, spots.Count); i++)
        {
            Vector3 worldPos = spots[i] + new Vector3(0.5f, 0.8f, 0.5f);

            Object.Instantiate(
                WorldGen.Instance.unitPrefab,
                worldPos,
                Quaternion.Euler(90, 0, 0)
            );

            spawned++;
        }

        foreach (var p in spots)
            Debug.DrawRay(p + Vector3.up, Vector3.up, Color.green, 2f);

        return spawned;
    }
}
