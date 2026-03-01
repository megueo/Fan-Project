using UnityEngine;

public class BlockAuthoringComponent : MonoBehaviour
{
    public BlockPoint[] Blocks;
    public Vector3Int Step;

    public Vector3Int StepFromAngle(float angle)
    {
        switch (angle)
        {
            case 90:
                return new Vector3Int(Step.z, Step.y, Step.x);
            case 270:
                return new Vector3Int(Step.z, Step.y, Step.x);
            case -90:
                return new Vector3Int(Step.z, Step.y, Step.x);
            default:
                break;
        }
        return Step;
    }

    public void AuthorizeComponent()
    {
        for (int i = 0; i < Blocks.Length; i++)
        {
            //Note, this doesn't account for angles
            Vector3Int Position = Blocks[i].point + new Vector3Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y), Mathf.RoundToInt(transform.position.z));
            WorldGen.Instance.SetBlock(Position.x, Position.y, Position.z, Blocks[i].blockType);
        }

        Destroy(this.GetComponent<BlockAuthoringComponent>());
    }

    private void OnDrawGizmosSelected()
    {
        for (int i = 0; i < Blocks.Length; i++)
        {
            Gizmos.DrawWireCube(Blocks[i].point, Vector3.one);
        }
    }
}

[System.Serializable]
public struct BlockPoint
{
    public Vector3Int point;
    public ChunkBlockType blockType;
}
