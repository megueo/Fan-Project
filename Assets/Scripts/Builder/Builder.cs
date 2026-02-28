using UnityEngine;
using UnityEngine.InputSystem;

//Builder, Works hand in hand with structures and blocks to build them
//Note to self: Look up a dictionary before you name a core component of a project
public class Builder : MonoBehaviour
{
    public static Builder Instance;

    //For landscaping
    public Mesh SphereGraphic;
    //Likely gonna be general
    public Material TransparentMaterial;
    public IBuilderStrategy strategy;
    bool hold;

    public void Start()
    {
        Instance = this;
    }

    private void Update()
    {
        if (strategy == null) return;

        strategy.Preview();

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            strategy.OnMouseDown();
            hold = true;
        }
        if(hold == true)
        {
            strategy.OnMousePerform();
        }
        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            strategy.OnMouseUp();
            hold = false;
        }
        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            strategy.OnRotate();
        }
    }

    public void ChangeStrategy(IBuilderStrategy stragedy)
    {
        if (this.strategy != null)
        {
            this.strategy.Dispose();
            this.strategy = null;
        }
        this.strategy = stragedy;
    }

    public void ChangeBuilderToPiller(GameObject BlockPrefab)
    {
        PillerStrategy line = new PillerStrategy(BlockPrefab);
        ChangeStrategy(line);
    }

    public void ChangeBuilderToScaffhold(GameObject BlockPrefab)
    {
        ScaffholdingStrategy line = new ScaffholdingStrategy(BlockPrefab);
        ChangeStrategy(line);
    }

    public void ChangeBuilderToHeight(GameObject BlockPrefab)
    {
        VerticalObjectStrategy line = new VerticalObjectStrategy(BlockPrefab);
        ChangeStrategy(line);
    }

    public void ChangeBuilderToSingle(GameObject BlockPrefab)
    {
        SingleObjectStrategy line = new SingleObjectStrategy(BlockPrefab);
        ChangeStrategy(line);
    }

    public void ChangeBuilderToLine(GameObject BlockPrefab)
    {
        DragLineStrategy line = new DragLineStrategy(BlockPrefab);
        ChangeStrategy(line);
    }
}
