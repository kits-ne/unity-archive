using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(CircleCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class Enemy : MonoBehaviour
{
    [SerializeField] private float speed;
    private Rigidbody2D _rigidbody;

    public Transform target;
    public bool movable = false;
    public bool useVelocity = true;
    public bool isDestroy = false;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        if (movable && ReferenceEquals(target, null) == false)
        {
            var pos = _rigidbody.position;
            var dir = ((Vector2) target.position - pos).normalized;
            if (useVelocity)
            {
                var vel = dir * (speed * 100 * Time.fixedDeltaTime);
                _rigidbody.velocity = vel;
            }
            else
            {
                pos += dir * (speed * Time.deltaTime);
                _rigidbody.MovePosition(pos);
            }
        }
    }

    private void Reset()
    {
        var body = GetComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Kinematic;
    }
}