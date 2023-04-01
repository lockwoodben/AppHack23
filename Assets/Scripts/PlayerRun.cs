using System.Collections;
using UnityEngine;

public class PlayerRun : MonoBehaviour
{

    // holds all movement attributes for player
    public PlayerRunAttributes Attrib;


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
    private Vector2 _moveInput;
    #endregion

    // utilizes inspection to 
    #region CHECK PARAMETERS
    [Header("Checks")]
    [SerializeField] private Transform _groundCheckPoint;
    [SerializeField] private Vector2 _groundCheckSize = new Vector2(0.49f, 0.03f);
    #endregion

    #region LAYERS AND TAGS
    [Header("Layers and Tags")]
    [SerializedField] private LayerMask _groundLayer;
    #endregion

    // activates rigid object
    private void awake() {
        rigid = getComponent<Rigidbody2D>();
    }

    private void Update() {
            //updates ground timer
            #region TIMERS
            lastOnGroundTimer -= Time.deltaTime;
            #endregion

            #region INPUT HANDLER
            _moveInput.x = _moveInput.GetAxisRaw("Horizontal"); // gets direction of movement

            if (_moveInput.x != 0)
                CheckDirectionToFace(_moveInput.x > 0); 
            #endregion

            #region COLLISION CHECKS
            // ground check
            if (Physics2D.OverlapBox(_groundCheckPoint.position, _groundCheckSize, 0, _groundLayer)) // check if hitbox is colliding with ground
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
        
        float targetSpeed = _moveInput.x * Data.runMaxSpeed; // calculate direction and velocity

        #region Calculate AccelRate
        float accelRate;

        // calculates acceleration based on if we're accelerating or turning or deccelerating
        // apply airborne multiplier if applicable
        if (lastOnGroundTimer > 0)
            accelRate = (Mathf.Abs)
    }

    }

