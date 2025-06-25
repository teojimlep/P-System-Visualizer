using System.Collections;
using System.Collections.Generic;
//using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class TurtleOrientation
{
    public Vector3 head;
    public Vector3 left;
    public Vector3 up;

    public TurtleOrientation(Vector3 head, Vector3 left, Vector3 up)
    {
        this.head = head;
        this.left = left;
        this.up = up;
    }

    public TurtleOrientation Copy()
    {
        return new TurtleOrientation(new Vector3(head.x, head.y, head.z),
                                     new Vector3(left.x, left.y, left.z),
                                     new Vector3(up.x, up.y, up.z));
    }
}

public class TurtleState
{
    public Vector3 Position;
    public TurtleOrientation Orientation;
    
    public TurtleState(Vector3 position, TurtleOrientation orientation)
    {
        this.Position = position;
        this.Orientation = orientation;
    }

    // Deep copy method
    public TurtleState Copy()
    {
        Vector3 positionCopy = new Vector3(Position.x, Position.y, Position.z);
        TurtleOrientation orientationCopy = Orientation.Copy();
        return new TurtleState(positionCopy, orientationCopy);
    }   
}

public class TurtleSpace
{
    // bounding box explpred by the turtle
    public List<float> XLim;
    public List<float> YLim;
    public List<float> ZLim;

    public TurtleSpace(Vector3 initPosition)
    {
        XLim = new List<float> { initPosition.x, initPosition.x };
        YLim = new List<float> { initPosition.y, initPosition.y };
        ZLim = new List<float> { initPosition.z, initPosition.z };
    }

    // Update the bounds of the explored space
    public void UpdateBounds(Vector3 position)
    {
        if (position.x < XLim[0]) XLim[0] = position.x;
        if (position.x > XLim[1]) XLim[1] = position.x;

        if (position.y < YLim[0]) YLim[0] = position.y;
        if (position.y > YLim[1]) YLim[1] = position.y;

        if (position.z < ZLim[0]) ZLim[0] = position.z;
        if (position.z > ZLim[1]) ZLim[1] = position.z;
    }

    public Vector3 GetCenter()
    {
        // Calculate the center by averaging the min and max bounds for each axis
        float centerX = (XLim[0] + XLim[1]) / 2f;
        float centerY = (YLim[0] + YLim[1]) / 2f;
        float centerZ = (ZLim[0] + ZLim[1]) / 2f;

        return new Vector3(centerX, centerY, centerZ);
    }

    public Vector3 GetDimensions()
    {
        float lengthX = Mathf.Abs(XLim[0] - XLim[1]);
        float lengthY = Mathf.Abs(YLim[0] - YLim[1]);
        float lengthZ = Mathf.Abs(ZLim[0] - ZLim[1]);

        return new Vector3(lengthX, lengthY, lengthZ);
    }

    // Visualize the bounding box in the Unity editor using Gizmos
    public void DrawBoundingBox()
    {

        Vector3 min = new Vector3(XLim[0], YLim[0], ZLim[0]);
        Vector3 max = new Vector3(XLim[1], YLim[1], ZLim[1]);
        Debug.Log(min);
        Debug.Log(max);

        // Draw lines to represent the bounding box
        Debug.DrawLine(new Vector3(min.x, min.y, min.z), new Vector3(max.x, min.y, min.z), Color.blue, 20f); // Bottom front
        Debug.DrawLine(new Vector3(max.x, min.y, min.z), new Vector3(max.x, min.y, max.z), Color.blue, 20f); // Bottom right
        Debug.DrawLine(new Vector3(max.x, min.y, max.z), new Vector3(min.x, min.y, max.z), Color.blue, 20f); // Bottom back
        Debug.DrawLine(new Vector3(min.x, min.y, max.z), new Vector3(min.x, min.y, min.z), Color.blue, 20f); // Bottom left

        Debug.DrawLine(new Vector3(min.x, max.y, min.z), new Vector3(max.x, max.y, min.z), Color.blue, 20f); // Top front
        Debug.DrawLine(new Vector3(max.x, max.y, min.z), new Vector3(max.x, max.y, max.z), Color.blue, 20f); // Top right
        Debug.DrawLine(new Vector3(max.x, max.y, max.z), new Vector3(min.x, max.y, max.z), Color.blue, 20f); // Top back
        Debug.DrawLine(new Vector3(min.x, max.y, max.z), new Vector3(min.x, max.y, min.z), Color.blue, 20f); // Top left

        Debug.DrawLine(new Vector3(min.x, min.y, min.z), new Vector3(min.x, max.y, min.z), Color.blue, 20f); // Left side
        Debug.DrawLine(new Vector3(max.x, min.y, min.z), new Vector3(max.x, max.y, min.z), Color.blue, 20f); // Right side
        Debug.DrawLine(new Vector3(max.x, min.y, max.z), new Vector3(max.x, max.y, max.z), Color.blue, 20f); // Right side
        Debug.DrawLine(new Vector3(min.x, min.y, max.z), new Vector3(min.x, max.y, max.z), Color.blue, 20f); // Left side
    }
}

public class Turtle
{
    public TurtleState State;
    public Stack<TurtleState> stateStack = new Stack<TurtleState>();
    public TurtleSpace ExploredSpace; // intialized as 0,0
    public Turtle(Vector3 position, TurtleOrientation orientation)
    {
        this.State = new TurtleState(position, orientation);
        this.ExploredSpace = new TurtleSpace(position);
    }

    // Move the turtle forward
    public void Forward(float distance)
    {
        State.Position += State.Orientation.head * distance;
        ExploredSpace.UpdateBounds(State.Position);
    }
    // Rotate the turtle around an axis by a given angle
    public void Rotate(float angle, Vector3 axis)
    {
        Quaternion quaternionRotation = Quaternion.AngleAxis(angle, axis);
        State.Orientation.head = quaternionRotation*State.Orientation.head;
        State.Orientation.left = quaternionRotation*State.Orientation.left;
        State.Orientation.up = quaternionRotation*State.Orientation.up;
    }
    // Push the current position and orientation into the stack
    public void PushState()
    {
        stateStack.Push(State.Copy());
    }
    // Pop the position and orientation from the stack and restore them
    public void PopState()
    {
        if (stateStack.Count > 0)
        {
            State = stateStack.Pop();
        }
    }
}