[System.Serializable]
public struct Block
{
    public BlockType type;
    public byte data;

    public bool IsSolid => type != BlockType.Air;
}
