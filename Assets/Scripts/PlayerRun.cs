using System.Collections;
using UnityEngine;

public class PlayerRun : MonoBehaviour
{

    // holds all movement attributes for player
    public PlayerAttributes Attrib;


    // handles all plater animations
    #region COMPONENTS
    public Rigidbody2D rigid { get; private set; }
    #endregion

    // variables that control and actions players have
    #region STATE PARAMETERS
    //public bool isFacingRight {get; private set;} 
    public float lastOnGroundTimer { get; private set; }
    #endregion

    // controls basic movement
    #region INPUT PARAMETERS
    private Vector2 moveInput;
    #endregion

    // utilizes inspection to 
    #region CHECK PARAMETERS
    [Header("Checks")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private Vector2 groundCheckSize = new Vector2(0.49f, 0.03f);
    #endregion

    #region LAYERS AND TAGS
    [Header("Layers and Tags")]
    [SerializeField] private LayerMask groundLayer;
    #endregion

    // activates rigid object
    private void awake() {
        rigid = GetComponent<Rigidbody2D>();
    }

    private void Update() {
            //updates ground timer
            #region TIMERS
            lastOnGroundTimer -= Time.deltaTime;
            #endregion

            #region INPUT HANDLER
            moveInput.x = Input.GetAxisRaw("Horizontal"); // gets direction of movement

           // if (moveInput.x != 0)
             //   CheckDirectionToFace(moveInput.x > 0); 
            #endregion

            #region COLLISION CHECKS
            // ground check
            if (Physics2D.OverlapBox(groundCheckPoint.position, groundCheckSize, 0, groundLayer)) // check if hitbox is colliding with ground
                lastOnGroundTimer = 0.1f;
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
            else {
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
}
    
