using System;
using System.Collections.Generic;
using UnityEngine;

public static class CommandRegistry
{
    static Dictionary<string, Action<string[]>> commands =
        new Dictionary<string, Action<string[]>>();

    static CommandRegistry()
    {
        Register("help", Help);
        Register("echo", Echo);
        Register("give", Give);
        Register("spawn", Spawn);
        Register("clear", Clear);
    }

    static void Register(string name, Action<string[]> action)
    {
        commands[name] = action;
    }

    public static void Execute(string input)
    {
        string[] parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return;

        string cmd = parts[0].ToLower();
        string[] args = parts.Length > 1 ? parts[1..] : Array.Empty<string>();

        if (commands.TryGetValue(cmd, out var action))
        {
            action.Invoke(args);
        }
        else
        {
            DevConsole.Instance.Log($"Unknown command: {cmd}");
        }
    }

    static void Help(string[] args)
    {
        DevConsole.Instance.Log("Commands:");
        foreach (var c in commands.Keys)
            DevConsole.Instance.Log($" - {c}");
    }

    static void Echo(string[] args)
    {
        DevConsole.Instance.Log(string.Join(" ", args));
    }

    static void Give(string[] args)
    {

    }

    static void Spawn(string[] args)
    {
        if (args.Length == 0)
        {
            DevConsole.Instance.Log("Usage: spawn <totem|unit>");
            return;
        }

        switch (args[0].ToLower())
        {
            case "totem":
                {
                    Camera cam = Camera.main;
                    if (cam == null) return;

                    if (Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, 100f))
                    {
                        Vector3Int grid = Vector3Int.FloorToInt(hit.point);
                        WorldGen.Instance.PlaceTotem(grid);
                    }
                    else
                    {
                        DevConsole.Instance.Log("No ground hit");
                    }
                    break;
                }
        }
    }


    static void Clear(string[] args)
    {
        DevConsole.Instance.ClearLog();
    }
}
