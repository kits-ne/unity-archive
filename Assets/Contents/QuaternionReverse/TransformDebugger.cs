using System;
using System.Collections;
using System.Collections.Generic;
using Shapes;
using UnityEngine;

[ExecuteAlways]
public class TransformDebugger : ImmediateModeShapeDrawer
{
    public enum AddMode
    {
        Basis,
        Relative,
    }

    public enum ReverseMode
    {
        None,
        Reflect,
        Inverse
    }

    [SerializeField] private AddMode mode = AddMode.Basis;
    [SerializeField] private ReverseMode reverse = ReverseMode.None;
    [SerializeField] private float length = 1;
    [SerializeField] private float thickness = 1;
    [SerializeField] private Vector3 addedRotation;
    [SerializeField] private float addedLength = 1;
    [SerializeField] private float basisLength = 1;
    [SerializeField] private float basisThickness = 1;


    public override void DrawShapes(Camera cam)
    {
        var tr = transform;

        using (Draw.Command(cam))
        {
            var position = tr.position;

            Draw.Line(position, position + tr.up * length, thickness, Color.green);
            Draw.Line(position, position + tr.right * length, thickness, Color.red);
            Draw.Line(position, position + tr.forward * length, thickness, Color.blue);

            // var axis = Quaternion.AngleAxis(addedRotation.y, Vector3.up) *
            // Quaternion.AngleAxis(addedRotation.x, Vector3.right) *
            // Quaternion.AngleAxis(addedRotation.z, Vector3.forward);
            var axis = Quaternion.Euler(addedRotation);
            var rot = tr.rotation;
            // transform.up 기준으로 회전
            // var added = tr.rotation * axis;
            // Vector3.up 기준으로 회전
            // var added = axis * tr.rotation;
            var added = mode switch
            {
                AddMode.Basis => axis * rot,
                AddMode.Relative => rot * axis,
                _ => throw new ArgumentOutOfRangeException()
            };

            var upColor = new Color(0, 1, 0, 0.5f);
            var rightColor = new Color(1, 0, 0, 0.5f);
            var forwardColor = new Color(0, 0, 1, 0.5f);
            switch (reverse)
            {
                case ReverseMode.None:
                    Draw.Line(position, position + added * Vector3.up * addedLength, thickness, upColor);
                    Draw.Line(position, position + added * Vector3.right * addedLength, thickness, rightColor);
                    Draw.Line(position, position + added * Vector3.forward * addedLength, thickness, forwardColor);
                    break;
                case ReverseMode.Reflect:
                    var up = position + Vector3.Reflect(tr.up, Vector3.right) * addedLength;
                    Draw.Line(position, up, thickness, upColor);

                    var right = position + Vector3.Reflect(tr.right, Vector3.right) * addedLength;
                    Draw.Line(position, right, thickness, rightColor);

                    var forward = position + Vector3.Reflect(tr.forward, Vector3.right) * addedLength;
                    Draw.Line(position, forward, thickness, forwardColor);
                    break;
                case ReverseMode.Inverse:
                    var inverse = axis * Quaternion.Inverse(rot);
                    Draw.Line(position, position + inverse * Vector3.up * addedLength, thickness, upColor);

                    break;
            }

            Draw.UseDashes = true;
            Draw.DashSize = 10;
            Draw.Line(position + Vector3.forward * basisLength,
                position + Vector3.back * basisLength,
                basisThickness, Color.white);
            Draw.Line(position + Vector3.up * basisLength,
                position + Vector3.down * basisLength,
                basisThickness, Color.white);
            Draw.Line(position + Vector3.left * basisLength,
                position + Vector3.right * basisLength,
                basisThickness, Color.white);
        }
    }
}