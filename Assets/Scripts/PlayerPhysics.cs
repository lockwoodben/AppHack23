using System.Collections;
using UnityEngine;

public class PlayerPhysics : MonoBehaviour
{

    // holds all movement attributes for player
    public PlayerAttributes Attrib;


    // handles all plater animations
    #region COMPONENTS
    public Rigidbody2D rigid { get; private set; }
    #endregion

    // variables that control what actions players have
    #region STATE PARAMETERS

    public float lastOnGroundTimer { get; private set; }
    public float lastOnWallTimer { get; private set; }
    public float lastOnWallRightTimer { get; private set; }
    public float lastOnWallLeftTimer {get; private set;}
    public bool isJumping {get; private set;}
    public bool isFacingRight {get; private set;} 
    public bool isWallJumping {get; private set;}
    #endregion

    // jump bools
    private bool isPartialJump;
    private bool isFalling;

    // controls basic movement
    #region INPUT PARAMETERS
    private Vector2 moveInput;

    public float lastJumpTime {get; private set;}
    #endregion

    // utilizes inspection to 
    #region CHECK PARAMETERS
    [Header("Checks")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private Vector2 groundCheckSize = new Vector2(0.49f,0.3f);
    [Space(5)]
    [SerializeField] private Transform frontWallCheckPoint;
	[SerializeField] private Transform backWallCheckPoint;
    [SerializeField] private Vector2 wallCheckSize = new Vector2(0.5f,1f);
    #endregion

    #region LAYERS AND TAGS
    [Header("Layers and Tags")]
    [SerializeField] private LayerMask groundLayer;
        #endregion

    // activates rigid object
    private void Start() {
        SetGScale(Attrib.gScale);
        isFacingRight = true;
    }

    private void Awake() {
        rigid = GetComponent<Rigidbody2D>();
    }

    private void Update() {
            

            #region TIMERS
            lastOnGroundTimer -= Time.deltaTime;
            
            lastJumpTime -= Time.deltaTime;
            #endregion


            #region INPUT HANDLER
            moveInput.x = Input.GetAxisRaw("Horizontal"); // gets direction of movement
            moveInput.y = Input.GetAxisRaw("Vertical"); // gets the jump

            if (moveInput.x != 0) {
                CheckDirectionToFace(moveInput.x > 0);
            }

            if(Input.GetKeyDown(KeyCode.Space)) {
                OnJumpInput();
            }
            
            if (Input.GetKeyUp(KeyCode.Space)) {
                OnJumpUpInput();
            }
            
            
            #endregion


            #region COLLISION CHECKS
            // ground check
            if (!isJumping) {
                if (Physics2D.OverlapBox(groundCheckPoint.position, groundCheckSize, 0, groundLayer) && !isJumping) { // check if hitbox is colliding with ground
                    lastOnGroundTimer = Attrib.coyoteTime;
                }
            
            // right wall check
            if (((Physics2D.OverlapBox(frontWallCheckPoint.position, wallCheckSize, 0, groundLayer) && isFacingRight)
                || (Physics2D.OverlapBox(backWallCheckPoint.position, wallCheckSize, 0, groundLayer) && !isFacingRight)) && !isWallJumping) {
                    lastOnWallRightTimer = Attrib.coyoteTime;
                }
            // left wall check
            if (((Physics2D.OverlapBox(frontWallCheckPoint.position, wallCheckSize, 0, groundLayer) && !isFacingRight)
                || (Physics2D.OverlapBox(backWallCheckPoint.position, wallCheckSize, 0, groundLayer) && isFacingRight)) && !isWallJumping) {
                    lastOnWallLeftTimer = Attrib.coyoteTime;
                }
            }
            #endregion


            #region JUMP CHECKS
            // Checks if the player is jumping 
            if (isJumping && rigid.velocity.y < 0) {
                isJumping = false;
                isFalling = true;
            }

            // Checks 
            if (lastOnGroundTimer > 0 && !isJumping) {
                isPartialJump = false;

                if (!isJumping) {
                    isFalling = false;
                }
            }

            // call jump
            if (CanJump() && lastJumpTime > 0) {
                isJumping = true;
                isPartialJump = false;
                isFalling = false;
                Jump();
            }
            #endregion

            #region GRAVITY
            if (isPartialJump) {
                // higher grav if jump is released
                // set new grav scale = gScale * partial Jump gravity multiplier
                SetGScale(Attrib.gScale * Attrib.partialJumpGravMult);
                // update velocity, keep y velocity lte max fall speed
                rigid.velocity = new Vector2(rigid.velocity.x, Mathf.Max(rigid.velocity.y, -Attrib.maxFallSpeed));  
            } else if ((isJumping || isFalling) && Mathf.Abs(rigid.velocity.y) < Attrib.jHangTime) {
                SetGScale(Attrib.gScale * Attrib.jHangGravMult);
            } else if (rigid.velocity.y < 0) {
                // higher grav when falling
                SetGScale(Attrib.gScale * Attrib.fallGravMult);
                // caps fall speed
                rigid.velocity = new Vector2(rigid.velocity.x, 
                    Mathf.Max(rigid.velocity.y, -Attrib.maxFallSpeed));
            } else {
                // default grav
                SetGScale(Attrib.gScale);
            }
            #endregion
        }

    private void FixedUpdate() {
        Run();
    }

    // movement
    #region RUN METHODS
    private void Run()
    {
        
        float targetSpeed = moveInput.x * Attrib.runMaxSpeed; // calculate direction and velocity


        #region Calculate AccelRate
        float accelRate;

        // calculates acceleration based on if we're accelerating or turning or deccelerating
        // apply airborne multiplier if applicable
        if (lastOnGroundTimer > 0) {
            if (Mathf.Abs(targetSpeed) > 0.01f) {
                accelRate = Attrib.runAccelVal;
            }
            else {
                accelRate = Attrib.runDeccelVal;
            }
        }
        else {
            if (Mathf.Abs(targetSpeed) > 0.01f) {
                accelRate = Attrib.runAccelVal * Attrib.accelInAir;
            }
            else
            {
                accelRate = Attrib.runDeccelVal * Attrib.deccelInAir;
            }
        }
        #endregion

        #region Conserve Momentum
        if (Attrib.doConserveMomentum && Mathf.Abs(rigid.velocity.x) > Mathf.Abs(targetSpeed) && 
            Mathf.Sign(rigid.velocity.x) == Mathf.Sign(targetSpeed) && 
            Mathf.Abs(targetSpeed) > 0.01f && lastOnGroundTimer < 0) 
        {
            // stop deccelRate from changing in order to preserve momentum
            accelRate = 0;
        }
        #endregion

        // calculate the diff between max and current horrizontal speed
        float speed = targetSpeed - rigid.velocity.x;

        // calculate the force to add to rigid
        float force = speed * accelRate;

        // apply the horrizontal force
        rigid.AddForce(force * Vector2.right, ForceMode2D.Force);
    }
    #endregion


    #region GEN METHODS
    
    public void SetGScale(float scale) {
        rigid.gravityScale = scale;
    }
    #endregion


    #region  CHECK FUNCTIONS

    // This checks the direction of the player
    public void CheckDirectionToFace(bool isMovingRight) {

        if (isMovingRight != isFacingRight) {
            turn();
        }
    }

    private bool CanJump() {
        return lastOnGroundTimer > 0 && !isJumping;
    }

    private bool CanPartialJump() {
        return isJumping && rigid.velocity.y > 0;
    }
    #endregion

    // This will flip the player around
    public void turn() {

        // the scale to flip
        Vector3 scale = transform.localScale;

        scale.x *= -1;
        isFacingRight = !isFacingRight;
    }


    #region JUMP FUNCTIONS
    private void Jump() {

        // make sure jump can only be called once 
        // by resetting its timers
        lastJumpTime = 0;
        lastOnGroundTimer = 0;

        #region Jump
        // calculate the the jump force
        float force = Attrib.jForce;

        // if the player is set the y velocity to 0
        if (rigid.velocity.y < 0) {
            force -= rigid.velocity.y;
        }

        rigid.AddForce(Vector2.up * force, ForceMode2D.Impulse);

        #endregion
    }
    #endregion


    #region INPUT CHECKS
    // these handle input detected in Update()
    public void OnJumpInput() {
            // set 
            lastJumpTime = Attrib.jBuffTime;
    }

    public void OnJumpUpInput() {
        if (CanPartialJump()) {
            isPartialJump = true;
        }
    }

    #endregion
}
    
