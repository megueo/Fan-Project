using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

//General interface for different ways of placing stuff, In a straight line, or drag over a wide field, or replace a blocktype within a sphere
//2.0. All scripts that use this interfaces have been moved due to how complicated and big the code got
public interface IBuilderStrategy
{
    //Disposes of any object or previews
    public void Dispose();
    //When mouse is first down
    public void OnMouseDown();
    //While mouse is down
    public void OnMousePerform();
    //Mouse up
    public void OnMouseUp();
    //Rotate on press of R
    public void OnRotate();
    //Previews the Strategy
    public void Preview();
}
