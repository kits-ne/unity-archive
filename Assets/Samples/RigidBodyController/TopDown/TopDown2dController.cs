using UnityEngine;

namespace Samples.RigidBodyController
{
    public struct FrameInput
    {
        public Vector2 Move;
        public bool DashDown;
        public bool Attack;
    }

    [RequireComponent(typeof(Rigidbody2D))]
    public class TopDown2dController : MonoBehaviour //, IOverlapSource2D
    {
        [SerializeField] private Rigidbody2D rb;

        public Vector2 Position => rb.position;
        public Vector2 Direction { get; private set; } = Vector2.right;

        private FrameInput _frameInput;
        private Vector2 _speed;

        public Vector2 Speed => _speed;

        private int _fixedFrame;

        [SerializeField] private float maxSpeed = 10;
        public float MaxSpeed => maxSpeed;
        [SerializeField] private Vector2 acceleration = Vector2.one;
        [SerializeField] private float deceleration = 1f;

        [SerializeField] private float dashVelocity;
        private bool _dashToConsume;
        private Vector2 _dashVel;
        private bool _dashing = false;
        private bool _canDash = true;
        private int _dashStartedFrame;
        [SerializeField] private int dashFrames = 10;
        [SerializeField] private float dashEndMultiplier = 0.3f;

        // [SerializeField] private Animator animator;
        // private static readonly int MovingId = Animator.StringToHash("moving");
        // private static readonly int AttackId = Animator.StringToHash("attack");

        private bool _isLeft;

        public bool IsLeft
        {
            get
            {
                if (Mathf.Abs(_frameInput.Move.x) > 0.1f)
                {
                    _isLeft = _frameInput.Move.x < 0;
                }

                return _isLeft;
            }
        }

        private bool _hasControl = true;

        public bool IsInitialized { get; private set; } = false;

        // [SerializeField] private OverlapCircle overlapAttack;
        // [SerializeField] private GameObject attackPrefab;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();

            // var stateEvents = animator.GetBehaviours<StateEventBridge>();
            // stateEvents.First(_ => _.Key.Equals("attack")).OnStateExitEvent += OnOnStateExitEvent;

            // overlapAttack.Source = this;

            Physics2D.queriesStartInColliders = false;
            Physics2D.queriesHitTriggers = false;
            IsInitialized = true;
        }


        private void Update()
        {
            // if (!_attackStarted)
            // {
            _frameInput.Move = new Vector2(
                Input.GetAxisRaw("Horizontal"),
                Input.GetAxisRaw("Vertical")
            ).normalized;
            // }

            if (_frameInput.Move != Vector2.zero)
            {
                // if (!_attackStarted)
                // {
                Direction = _frameInput.Move;
                // }
            }
            // else if (!_attackStarted)
            else
            {
                Direction = IsLeft ? Vector2.left : Vector2.right;
            }

            _frameInput.DashDown = Input.GetKeyDown(KeyCode.Space);
            _frameInput.Attack = Input.GetKeyDown(KeyCode.Q);

            if (_frameInput.DashDown)
            {
                _dashToConsume = true;
            }

            // animator.SetBool(MovingId, _speed != Vector2.zero);
            // animator.transform.rotation = Quaternion.Euler(new Vector3(
            //     0, IsLeft ? 0 : 180, 0
            // ));
            //
            // if (!_attackStarted && _frameInput.Attack)
            // {
            //     // animator.SetTrigger(AttackId);
            //     _attackStarted = true;
            //     _speed = Vector2.zero;
            //     _coefficients.Clear();
            //     _coefficients.Enqueue(0.15f);
            //     _coefficients.Enqueue(0.4f);
            //     _coefficients.Enqueue(0.7f);
            //
            //     if (overlapAttack.OverlapNonAlloc() > 0)
            //     {
            //         var targetDir = overlapAttack.GetDirection();
            //         var dot = Vector2.Dot(Direction, targetDir);
            //         if (dot < 0.85f)
            //         {
            //             Direction = targetDir;
            //         }
            //     
            //         overlapAttack.FlushResult();
            //     }
            //
            //     _attackVfxRotations.Enqueue(new Vector3(-37, -15, -29));
            // }
        }

        // private bool _attackStarted = false;
        // [SerializeField] private int strangth = 100;
        // private readonly Queue<float> _coefficients = new Queue<float>();
        // private readonly Queue<Vector3> _attackVfxRotations = new Queue<Vector3>();
        // private bool _attackDashStarted = false;

        // public void Hit()
        // {
        //     if (!_attackDashStarted && Direction != Vector2.zero)
        //     {
        //         _speed = Direction * 15;
        //         _attackStarted = true;
        //     }
        //
        //     if (_coefficients.Any())
        //     {
        //         var dmg = _coefficients.Dequeue() * strangth;
        //
        //         var count = overlapAttack.OverlapNonAlloc();
        //         for (int i = 0; i < count; i++)
        //         {
        //             var e = overlapAttack.GetResult(i);
        //             // e.attachedRigidbody.AddForce(Direction * 30, ForceMode2D.Impulse);
        //             Battle_M.Instance.DamageHUD_view(dmg, e.gameObject, string.Empty);
        //             e.gameObject.SendMessage("OnDamage", (Direction, (int) dmg));
        //         }
        //
        //         if (_attackVfxRotations.Any())
        //         {
        //             var rot = _attackVfxRotations.Dequeue();
        //
        //             var rad = math.atan2(Direction.y, Direction.x);
        //             var qut = quaternion.AxisAngle(IsLeft ? Vector3.back : Vector3.forward, rad);
        //             // y axis reverse
        //             if (IsLeft)
        //             {
        //                 qut = math.mul(quaternion.Euler(math.radians(Vector3.right * 180)), qut);
        //             }
        //
        //             qut = math.mul(qut, quaternion.Euler(math.radians(rot)));
        //             Instantiate(attackPrefab, overlapAttack.Origin, qut, transform);
        //         }
        //     }
        // }

        // private void OnOnStateExitEvent(Animator anim, AnimatorStateInfo info, int layer)
        // {
        //     // end
        //     _attackStarted = false;
        //     _attackDashStarted = false;
        //     _speed = Vector2.zero;
        // }

        private void FixedUpdate()
        {
            _fixedFrame++;

            // 반대 방향 전환 시 현재 속도 초기화
            if (Vector2.Dot(_speed.normalized, _frameInput.Move) < 0)
            {
                _speed = Vector2.zero;
            }

            // Dash
            if (_dashToConsume && _canDash)
            {
                var dir = _frameInput.Move.normalized;
                if (dir == Vector2.zero)
                {
                    _dashToConsume = false;
                    return;
                }

                _dashVel = dir * dashVelocity;
                _dashing = true;
                _canDash = false;
                _dashStartedFrame = _fixedFrame;
            }

            if (_dashing)
            {
                _speed = _dashVel;
                if (_fixedFrame > _dashStartedFrame + dashFrames)
                {
                    _dashing = false;
                    _speed *= dashEndMultiplier;
                    _canDash = true;
                }

                _dashToConsume = false;
            }

            // Horizontal
            if (_frameInput.Move.x == 0)
            {
                _speed.x = Mathf.MoveTowards(_speed.x, 0, deceleration * Time.fixedDeltaTime);
            }

            // Vertical
            if (_frameInput.Move.y == 0)
            {
                _speed.y = Mathf.MoveTowards(_speed.y, 0, deceleration * Time.fixedDeltaTime);
            }


            // Movement
            // if (!_attackStarted)
            // {
            var xInput = _frameInput.Move.x;
            var yInput = _frameInput.Move.y;
            _speed.x = Mathf.MoveTowards(_speed.x, xInput * maxSpeed, acceleration.x * Time.fixedDeltaTime);
            _speed.y = Mathf.MoveTowards(_speed.y, yInput * maxSpeed, acceleration.y * Time.fixedDeltaTime);
            // }

            if (_hasControl)
            {
                rb.velocity = _speed;
            }
        }


        public virtual void TakeAwayControl(bool resetVelocity = true)
        {
            if (resetVelocity) rb.velocity = Vector2.zero;
            _hasControl = false;
        }

        public virtual void ReturnControl()
        {
            _speed = Vector2.zero;
            _hasControl = true;
        }

        private void Reset()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.gravityScale = 0;
        }
    }
}