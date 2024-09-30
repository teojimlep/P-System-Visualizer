using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
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

public class Turtle
{
    public TurtleState State;
    public Stack<TurtleState> stateStack = new Stack<TurtleState>();
    public Turtle(Vector3 position, TurtleOrientation orientation)
    {
        this.State = new TurtleState(position, orientation);
    }

    // Move the turtle forward
    public void Forward(float distance)
    {
        State.Position += State.Orientation.head * distance;
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