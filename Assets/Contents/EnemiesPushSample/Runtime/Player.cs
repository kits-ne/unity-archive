using System;
using System.Linq;
using Shapes;
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(CircleCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class Player : ImmediateModeShapeDrawer
{
    [SerializeField] private float speed = 10;
    [SerializeField] private LayerMask enemyMask;
    private Rigidbody2D _rigidbody;
    private Vector2 _direction;


    private bool _isDebugQ = false;
    [SerializeField] private float radiusQ;
    [SerializeField] private Color colorQ;
    public bool useVelocity = true;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        _direction.x = Input.GetAxisRaw("Horizontal");
        _direction.y = Input.GetAxisRaw("Vertical");

        if (Input.GetKeyDown(KeyCode.Q))
        {
            _isDebugQ = true;
        }
        else if (Input.GetKeyUp(KeyCode.Q))
        {
            _isDebugQ = false;
            ProcessQ();
        }
    }

    private void FixedUpdate()
    {
        if (useVelocity)
        {
            // var vel = _rigidbody.velocity;
            // vel += _direction * (speed * Time.deltaTime);
            var vel = _direction * (speed * Time.deltaTime);
            _rigidbody.velocity = vel;
        }
        else
        {
            var pos = _rigidbody.position;
            pos += _direction * (speed * Time.deltaTime);
            _rigidbody.MovePosition(pos);
        }
    }

    public override void DrawShapes(Camera cam)
    {
        using (Draw.Command(cam))
        {
            if (_isDebugQ)
            {
                Draw.Ring(_rigidbody.position, Vector3.forward, radiusQ, colorQ);
            }
        }
    }

    private Collider2D[] _bufferQ = new Collider2D[20];

    private void ProcessQ()
    {
        int numColliders = Physics2D.OverlapCircleNonAlloc(
            _rigidbody.position,
            radiusQ,
            _bufferQ,
            enemyMask
        );
        for (int i = 0; i < numColliders; i++)
        {
            Destroy(_bufferQ[i].gameObject);
            _bufferQ[i] = null;
        }

        Debug.Log($"{numColliders} / {_bufferQ.Count(_ => !ReferenceEquals(_, null))}");
    }

    private void Reset()
    {
        var body = GetComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Kinematic;
    }
}