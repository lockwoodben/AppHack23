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
    public bool isDashing {get; private set;}

    // jump
    private bool isPartialJump;
    private bool isFalling;
    private int jCount = 0;
    private int wCount = 0;
    private bool onGround;
    
    // wall jump
    private float wallJumpTimer;
    private int lastWallJumpD;

    // Dash
    private bool dashOnCD;
    private Vector2 lastDashDir;
    private bool isDashAttacking;
    private int dashLeft;
    #endregion

    // controls basic movement
    #region INPUT PARAMETERS
    private Vector2 moveInput;

    public float lastJumpTime {get; private set;}
    public float lastDashTime {get; private set;}
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
            lastDashTime -= Time.deltaTime;
            lastOnWallLeftTimer -= Time.deltaTime;
            lastOnWallTimer -= Time.deltaTime; 
            #endregion


            #region INPUT HANDLER
            moveInput.x = Input.GetAxisRaw("Horizontal"); // gets direction of movement
            moveInput.y = Input.GetAxisRaw("Vertical"); // gets the jump

            if (moveInput.x != 0) {
                CheckDirectionToFace(moveInput.x > 0);
            }

            if(Input.GetKeyDown(KeyCode.Space) || Input.GetButtonDown("Jump")) {
                OnJumpInput();
            }
            
            if (Input.GetKeyUp(KeyCode.Space) || Input.GetButtonUp("Jump")) {
                OnJumpUpInput();
            }

            if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetAxis("Fire1") != 0) {
                OnDashInput();
            }
            
            
            #endregion


            #region COLLISION CHECKS
            // ground check
            if (!isDashing && !isJumping) {
                if (Physics2D.OverlapBox(groundCheckPoint.position, groundCheckSize, 0, groundLayer) && !isJumping) { // check if hitbox is colliding with ground
                    jCount = 0;
                    wCount = 0;
                    lastOnGroundTimer = Attrib.coyoteTime;
                    onGround = false;
                }

                // right wall check
                if (((Physics2D.OverlapBox(frontWallCheckPoint.position, wallCheckSize, 0, groundLayer) && isFacingRight)
                    || (Physics2D.OverlapBox(backWallCheckPoint.position, wallCheckSize, 0, groundLayer) && !isFacingRight)) && !isWallJumping) {
                        lastOnWallRightTimer = Attrib.coyoteTime;
                        onGround = true;
                    }
                // left wall check
                if (((Physics2D.OverlapBox(frontWallCheckPoint.position, wallCheckSize, 0, groundLayer) && !isFacingRight)
                    || (Physics2D.OverlapBox(backWallCheckPoint.position, wallCheckSize, 0, groundLayer) && isFacingRight)) && !isWallJumping) {
                        lastOnWallLeftTimer = Attrib.coyoteTime;
                        onGround = true;
                    }

                lastOnWallTimer = Mathf.Max(lastOnWallLeftTimer, lastOnWallRightTimer);
            }
            #endregion


            #region JUMP CHECKS
            // Checks if the player is jumping 
            if (isJumping && rigid.velocity.y < 0) {
                isJumping = false;
                
                if (!isWallJumping) {
                    isFalling = true;
                }
            }

            // wall jump check
            if (isWallJumping && Time.time - wallJumpTimer > Attrib.wallJTime) {
                isWallJumping = false;
            }

            // Checks 
            if (lastOnGroundTimer > 0 && !isJumping && !isWallJumping) {
                isPartialJump = false;

                if (!isJumping) {
                    isFalling = false;
                }
            }

            if (!isDashing) {
                 // call jump
                if (CanJump() && lastJumpTime > 0) {
                    isJumping = true;
                    isPartialJump = false;
                    isFalling = false;
                    Jump();
                }
                else if (CanWallJump() && lastJumpTime > 0) {
                    isWallJumping = true;
                    isJumping = false;
                    isPartialJump = false;
                    isFalling = false;
                    
                    
                    wallJumpTimer = Time.time;
                    if (lastOnWallRightTimer > 0) {
                        lastWallJumpD = -1;
                    }
                    else {
                        lastWallJumpD = 1;
                    }
                    WallJump(lastWallJumpD);
                }   

            }
            #endregion
            
            #region DASH CHECKS
            if (CanDash() && lastDashTime > 0) {
                //freeze game for moment before dash
                Sleep(Attrib.dashSleepTime);

                if (moveInput != Vector2.zero) {
                    lastDashDir = moveInput;
                } 
                else {
                    if (isFacingRight) {
                        lastDashDir = Vector2.right;
                    } else {
                        lastDashDir = Vector2.left;
                    }
                }
                    
                isDashing = true;
                isJumping = false;
                isWallJumping = false;
                isPartialJump = false;

                StartCoroutine(nameof(StartDash), lastDashDir);
                
            }
            #endregion

            #region GRAVITY
            if(!isDashAttacking) {
                if (isPartialJump) {
                    // higher grav if jump is released
                    // set new grav scale = gScale * partial Jump gravity multiplier
                    SetGScale(Attrib.gScale * Attrib.partialJumpGravMult);
                    // update velocity, keep y velocity lte max fall speed
                    rigid.velocity = new Vector2(rigid.velocity.x, Mathf.Max(rigid.velocity.y, -Attrib.maxFallSpeed));  
                } else if ((isJumping || isWallJumping || isFalling) && Mathf.Abs(rigid.velocity.y) < Attrib.jHangTime) {
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
                } else {
                    SetGScale(0);
                }
            #endregion
        }

    private void FixedUpdate() {
        if (!isDashing)
        {
            Run(1);
        }
        else if (isDashAttacking) {
            Run(Attrib.dashEndRunLerp);
        }
    }

    // movement
    #region RUN METHODS
    private void Run(float lerpNum)
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

    private void Sleep(float time) {
        StartCoroutine(nameof(PerformSleep), time);
    }

    private IEnumerator PerformSleep(float time) {
        Time.timeScale = 0;
        yield return new WaitForSecondsRealtime(time); 
        Time.timeScale = 1;
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
        return (lastOnGroundTimer > 0 && !isJumping) || jCount < 2;
    }

    private bool CanPartialJump() {
        return isJumping && rigid.velocity.y > 0;
    }

    private bool CanWallPJump() {
        return isWallJumping && rigid.velocity.y > 0;
    }
    private bool CanDash() {
        if (!isDashing && dashLeft < Attrib.dashUses &&lastOnGroundTimer > 0 && !dashOnCD) {
            StartCoroutine(nameof(RefillDash), 1);
        }

        return dashLeft > 0;
    }
    
     private bool CanWallJump() {
        
            return onGround && lastOnGroundTimer <= 0 && lastOnWallTimer > 0 &&
            lastJumpTime > 0 && wCount < 1 && (!isWallJumping ||
            (lastOnWallRightTimer > 0 && lastWallJumpD == 1) || 
            (lastOnWallLeftTimer > 0 && lastWallJumpD == -1));
    }
    #endregion

    // This will flip the player around
    public void turn() {

        // the scale to flip
        Vector3 scale = transform.localScale;

        scale.x *= -1;
        transform.localScale = scale;
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
        
        // if you are falling reset the y velocity
        if (rigid.velocity.y < 0) {
            Vector3 vel = rigid.velocity;
            vel.y = 0f;
            rigid.velocity = vel;
        }

        if(jCount == 1) {
            rigid.AddForce(Vector2.up * Mathf.Min(force, Attrib.maxJumpSpeed), ForceMode2D.Impulse);
        } else {
            rigid.AddForce(Vector2.up * force, ForceMode2D.Impulse);
        }
        jCount++;

        // stops from wall jumping in air
        if (jCount == 2) {
            onGround = false;
        }
    }
    #endregion

    private void WallJump(int direction) {
        // make sure you cant wall jump multiple times'
        lastJumpTime = 0;
        lastOnGroundTimer = 1;
        lastOnWallLeftTimer = 0;
        lastOnWallRightTimer = 0;

        #region WALL JUMP
        Vector2 force = new Vector2(Attrib.wallJForce.x, Attrib.wallJForce.y);
        
        if (moveInput.x > 0) {
            // apply the force in the oppsite direction
            force.x *= direction;
        }
        else if (moveInput.x < 0){
            turn();
        }

        if (Mathf.Sign(rigid.velocity.x) != Mathf.Sign(force.x))
			force.x -= rigid.velocity.x;

        if (rigid.velocity.y < 0) {
            force.y -= rigid.velocity.y;
        }

        rigid.AddForce(force, ForceMode2D.Impulse);
        wCount++;
        #endregion
    }
    #endregion

    
    #region DASH METHODS
    // dash coroutine
    private IEnumerator StartDash(Vector2 dir) {
       lastOnGroundTimer = 0;
       lastDashTime = 0;
       

       float startTime = Time.time;

       dashLeft--;
       isDashAttacking = true;
       SetGScale(0);

       while (Time.time - startTime <= Attrib.dashAttackTime) {
            rigid.velocity = dir.normalized * Attrib.dashSpeed;
            // pauses loop until next frame
            yield return null;
       }

       startTime = Time.time;
       isDashAttacking = false;

       // begin end of dash
       SetGScale(Attrib.gScale);
       rigid.velocity = Attrib.dashEndSpeed * dir.normalized;

       while (Time.time - startTime <= Attrib.dashEndTime) {
            yield return null;
       }

       // dash over
       isDashing = false;
    }

    private IEnumerator RefillDash(int dashes) {
        dashOnCD = true;
        yield return new WaitForSeconds(Attrib.dashCD);
        dashOnCD = false;
        dashLeft++;
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

    public void OnDashInput() {
        lastDashTime = Attrib.dashInputBufferTime;
    }

    #endregion
}
    
