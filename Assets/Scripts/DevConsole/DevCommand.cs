using System;

public class DevCommand
{
    public string Name;
    public string Description;
    public Action<string[]> Execute;

    public DevCommand(string name, string description, Action<string[]> execute)
    {
        Name = name;
        Description = description;
        Execute = execute;
    }
}
