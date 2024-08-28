using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KevinCastejon.MoreAttributes;
using UnityEngine.InputSystem;
using Cinemachine.Utility;
using TMPro;

public enum PlayerStates
{
    idle,
    dashing,
    moving,
    jumping
}
public class PlayerController : MonoBehaviour, PCMInterface
{
    
    // Start is called before the first frame update
    [field: Header("Core"), SerializeField]
    public PlayerComponentManager PCM { get; set; }
    [field: SerializeField]
    public Rigidbody rb {  get; private set; }
    public Transform body { get; private set; }
    [field: Header("Movement")]    
    [field: SerializeField]

    /// <summary>
    /// maximum speed from normal running
    /// </summary>
    public float maxspeed { get; private set; }
    [field: ReadOnly, SerializeField]

    /// <summary>
    /// current modified maxSpeed
    /// </summary>
    public float currentMaxSpeed { get; private set; }
    [field: SerializeField]

    /// <summary>
    /// acceleration in unity units per second
    /// </summary>
    public float acceleration { get; private set; }
    [field: ReadOnly, SerializeField]
    public Vector2 dir { get; private set; } = Vector2.zero;
    [field: SerializeField, Tooltip("Degrees per second")]
    ///<summary>
    ///Degrees per second
    ///</summary>
    public float rotSpeed {  get; private set; }

    [SerializeField]
    private Transform bodyForward;
    [Header("Jump")]
    [SerializeField]
    private float jumpHeight;
    [SerializeField, ReadOnly]
    private float initialVelocity;
    [SerializeField]
    private int coyoteFrames;
    private int currentCoyoteFrames;
    [Header("Layers"), SerializeField]
    private LayerMask terrain;

    [Header("Dash")]
    [SerializeField]
    private float dashDistance;
    [SerializeField]
    private float dashDuration;
    [SerializeField]
    private float dashDelay;


    [field: Header("Physics"), SerializeField, Range(0, 100)]
    public float Gravity { get; private set; } = 10;
    [field: SerializeField, Min(0)]
    public float TerminalVelocity { get; private set; } = 30;
    [field: SerializeField, Min(0), Tooltip("Rate of deccleration when not moving")]
    ///<summary>
    ///decceleration from fake drag
    ///</summary>
    public float dragDeccel { get; private set; }
    [field: SerializeField]
    public Transform groundCheck { get; private set; }

    [field: SerializeField, Range(0, 3)]
    public float AirControl { get; private set; } = 1;

    [Header("States")]
    [SerializeField, ReadOnly]
    private PlayerStates currentState = PlayerStates.idle;
    [SerializeField, ReadOnly]
    private bool isGrounded;
    [SerializeField, ReadOnly]
    private bool isCoyote;
    [SerializeField, ReadOnly]
    private bool hasJumped = false;
    [SerializeField, ReadOnly]
    private bool isDashing;


    #region Inputs

    [SerializeField, ReadOnly]
    private Vector2 lastdir = Vector3.forward;
    public void OnMove(InputAction.CallbackContext context)
    {
        dir = context.ReadValue<Vector2>();
        if (!dir.Equals(Vector2.zero))
            lastdir = dir;
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        Jump();
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        Dash();
    }

    #endregion

    #region Unity Functions
    void Start()
    {
        currentMaxSpeed = maxspeed;
        calculateJumpVel();
        SetDashDelay();
    }

    // Update is called once per frame
    void Update()
    {
        CalculateState();
    }

    private void FixedUpdate()
    {
        CalculatePhysics();
        MovePlayer();
    }
    #endregion

    #region Movement
    private void MovePlayer()
    {
        Vector3 dir = DirToWorld(this.dir);
        Vector3 accel = dir * acceleration * Time.fixedDeltaTime;
        Vector3 newHori;
        if (isGrounded )
        {
            if ((GetHoriVel() + accel).magnitude <= currentMaxSpeed)
            {
                newHori = (GetHoriVel() + accel);
            }
            else
            {
                newHori = (GetHoriVel() + accel).normalized * rb.velocity.magnitude;
            }
        }
        else
        {
            newHori = (rb.velocity + (accel * AirControl)).normalized * rb.velocity.magnitude;
        }
        newHori.y = rb.velocity.y;
        rb.velocity = newHori;

        if (dir.Equals(Vector3.zero))
            dir = bodyForward.forward;
        float singleStep = Mathf.Deg2Rad * rotSpeed * Time.fixedDeltaTime;
        Vector3 newDir = Vector3.RotateTowards(bodyForward.forward, dir, singleStep, 0.0f);
        bodyForward.rotation = Quaternion.LookRotation(newDir);
    }
    private void Jump()
    {
        if (isCoyote && !hasJumped)
        {
            hasJumped = true;
            isCoyote = false;
            rb.velocity += Vector3.up * initialVelocity;
        }
    }

    private void calculateJumpVel()
    {
        initialVelocity = Mathf.Sqrt(2 * Gravity * jumpHeight);
    }
    private Coroutine dashCoroutine;
    private void Dash()
    {
        if(dashCoroutine == null)
        {
            dashCoroutine = StartCoroutine(Dashing(DirToWorld(lastdir)));
        }
    }

    private IEnumerator Dashing(Vector3 dashDir)
    {
        rb.velocity = HoriVel();
        Vector3 startPos = transform.position;
        Vector3 endPos = transform.position + dashDir.normalized * dashDistance;
        float endTime = Time.time + dashDuration;
        float timePassed = 0;
        while (Time.time < endTime)
        {
            float ratio = timePassed / dashDuration;
            isDashing = true;
            rb.position = Vector3.Lerp(startPos, endPos, ratio);
            timePassed += Time.deltaTime;
            yield return null;
        }

        yield return waitDashDelay;
        isDashing = false;
        dashCoroutine = null;
    }
    private WaitForSeconds waitDashDelay;
    private void SetDashDelay()
    {
        waitDashDelay = new WaitForSeconds(dashDelay);
    }

    #endregion

    #region State Decider
    private void CalculateState()
    {
        if (isDashing)
            currentState = PlayerStates.dashing;
        else if (hasJumped)
            currentState = PlayerStates.jumping;
        else if (!dir.Equals(Vector2.zero))
            currentState = PlayerStates.moving;
        else
            currentState = PlayerStates.idle;
    }
    #endregion

    #region Physics

    private void GroundCheck()
    {
        if (Physics.CheckBox(groundCheck.position, new Vector3(0.4f,0.1f,0.4f),PCM.values.BodyForward.rotation,terrain, QueryTriggerInteraction.Ignore))
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }
    }

    private void CalculatePhysics()
    {
        GroundCheck();
        CalculateGravity();
        CalculateHoriDrag();
    }
    private bool jumped = false;
    private void CalculateGravity()
    {
        if(!(isGrounded || isDashing))
        {
            if(!jumped && hasJumped)
            {
                jumped = true;
            }
            if (currentCoyoteFrames > 0)
            {
                currentCoyoteFrames--;
            }
            else
                isCoyote = false;
            if (rb.velocity.y > -TerminalVelocity)
                rb.velocity += Vector3.down * Gravity * Time.fixedDeltaTime;
        }
        else
        {
            if (jumped)
            {
                jumped = false;
                hasJumped = false;
            }
            Vector3 velocity = rb.velocity;
            if (velocity.y < 0)
                velocity.y = 0;
            rb.velocity = velocity;
            isCoyote = true;
            currentCoyoteFrames = coyoteFrames;
        }
    }

    private void CalculateHoriDrag()
    {
        if (dir.Equals(Vector2.zero) && isGrounded || GetHoriVel().magnitude > currentMaxSpeed)
        {
            float horiMag = GetHoriVel().magnitude;
            horiMag -= dragDeccel * Time.fixedDeltaTime;
            if (horiMag < 0f)
                horiMag = 0f;
            Vector3 newVel = rb.velocity.normalized * horiMag;
            newVel.y = rb.velocity.y;
            rb.velocity = newVel;
        }
    }

    public Vector3 GetHoriVel()
    {
        Vector3 horiVel = rb.velocity;
        horiVel.y = 0;
        return horiVel;
    }
    #endregion

    #region Utility
    /// <summary>
    /// converts vector2 to horizontal vector3
    /// </summary>
    /// <param name="dir"></param>
    /// <returns></returns>
    private Vector3 HoriVec3(Vector2 dir)
    {
        return new Vector3(dir.x, 0, dir.y);
    }

    /// <summary>
    /// Converts directional input vector into world vector direction relative to camera
    /// </summary>
    /// <param name="dir"></param>
    /// <returns></returns>
    private Vector3 DirToWorld(Vector2 dir)
    {
        Transform forward = PCM.values.CamForward;
        return forward.right * dir.x + forward.forward * dir.y;
    }

    /// <summary>
    /// returns rb's horizontal vel
    /// </summary>
    /// <returns></returns>
    private Vector3 HoriVel()
    {
        Vector3 vel = rb.velocity;
        vel.y = 0;
        return vel;
    }
    #endregion
}
