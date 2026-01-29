using UnityEngine;
using UnityEngine.InputSystem;

//Builder, Works hand in hand with structures and blocks to build them
public class Builder : MonoBehaviour
{
    public static Builder Instance;

    //For landscaping
    public Mesh SphereGraphic;
    //Likely gonna be general
    public Material TransparentMaterial;
    public IBuilderStragedy stragedy;
    bool hold;

    public void Start()
    {
        Instance = this;
    }

    private void Update()
    {
        if (stragedy == null) return;

        stragedy.Preview();

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            stragedy.OnMouseDown();
            hold = true;
        }
        if(hold == true)
        {
            stragedy.OnMousePerform();
        }
        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            stragedy.OnMouseUp();
            hold = false;
        }
    }

    public void ChangeStragedy(IBuilderStragedy stragedy)
    {
        if (this.stragedy != null)
        {
            this.stragedy.Dispose();
            this.stragedy = null;
        }
        this.stragedy = stragedy;
    }
}
