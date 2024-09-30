using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;
using UnityEngine;

public class Plant : MonoBehaviour
{
    public GameObject BranchPrefab;
    public float BranchRadius;
    public float BranchLength;

    public LSystem _lsystem;
    public LSystem LSystem {get{return this._lsystem;} set{this._lsystem=value;}}
    public Turtle _turtle;
    public Turtle Turtle{get{return this._turtle;}set{this._turtle=value;}}

    public int NumBranches {get; private set;}

    public void CreateBranch(Vector3 startPos, Vector3 endPos)
    {
        // Debug line
        Debug.DrawLine(startPos, endPos, Color.red, 60f);
        float Length = Vector3.Distance(startPos, endPos);
        Vector3 spawnPosition = Vector3.Lerp(startPos, endPos, 0.5f);
        Vector3 Orientation = Vector3.Normalize(endPos - startPos);
        // Scale is based on the prefab dimensions (cilinder of R=1, L=2; Length is y-axis)
        BranchPrefab.transform.localScale = new Vector3(BranchRadius, Length/2f, BranchRadius);
        Quaternion spawnRotation = Quaternion.FromToRotation(new Vector3(0f,1f,0f), Orientation);
        GameObject createdBranch = Instantiate(BranchPrefab, spawnPosition, spawnRotation, transform);
        createdBranch.name = "Branch_" + NumBranches.ToString();
    }

    public void Grow(int Iterations)
    {
        _lsystem.IterRewrite(Iterations, true);
        Vector3 startPos;
        foreach (int index in Enumerable.Range(0, _lsystem.CurrentString.Length))
        {
            char c = _lsystem.CurrentString[index];
            switch (c)
            {
                case 'F':
                case 'X':
                    NumBranches+=1;
                    startPos = _turtle.State.Position;
                    _turtle.Forward(BranchLength);
                    CreateBranch(startPos, _turtle.State.Position);
                    break;
                case '+':
                    _turtle.Rotate(_lsystem.Angle, _turtle.State.Orientation.up); 
                    break;
                case '-':
                    _turtle.Rotate(-1*_lsystem.Angle, _turtle.State.Orientation.up); 
                    break;
                case '&':
                    _turtle.Rotate(_lsystem.Angle, _turtle.State.Orientation.left); 
                    break;
                case '^':
                    _turtle.Rotate(-1*_lsystem.Angle, _turtle.State.Orientation.left); 
                    break;
                case '>':
                    _turtle.Rotate(_lsystem.Angle, _turtle.State.Orientation.head); 
                    break;
                case '<':
                    _turtle.Rotate(-1*_lsystem.Angle, _turtle.State.Orientation.head); 
                    break;
                case '|':
                    _turtle.Rotate(180.0f, _turtle.State.Orientation.up); 
                    break;
                case '[':
                    _turtle.PushState();
                    break;
                case ']':
                    _turtle.PopState();
                    break;
                default:
                    break;
            }
        }
    }
}