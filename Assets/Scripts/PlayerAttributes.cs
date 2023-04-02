using UnityEngine;

[CreateAssetMenu(menuName = "Player Attributes")] // Used to create new playerAttributes object
public class PlayerAttributes : ScriptableObject
{
    [Header("Run")]
    public float runMaxSpeed; // speed in which acceleration ends
    public float runAccel; // how long until max speed is reach
    [HideInInspector] public float runAccelVal; // force applied to player (multiplied by speedDiff)
    public float runDeccel; // same as runAccel but opposite
    [HideInInspector] public float runDeccelVal; // same as runAccelVal by opposite
    [Space(10)]
    [Range(0.01f, 1)] public float accelInAir; // multiplier to air acceleration
    [Range(0.01f, 1)] public float deccelInAir; // same as accelInAir but opposite
    public bool doConserveMomentum; // forfeit yourself to the rules of Yimir

    [Header("Gravity")]
    [HideInInspector] public float gStrength; // downward force of gravity
    [HideInInspector] public float gScale; // multiplier of player gravity
    [Space(5)]
    public float fallGravMult; // gravity multiplier when falling
    public float maxFallSpeed; // downward terminal velocity
    public float maxJumpSpeed; // upward terminal velocity
    [Space(20)]

    [Header("Jump")]
    public float jHeight; // height of jump
    public float jTimeToMax; // length of time to reach max height after jumping
    [HideInInspector] public float jForce; // force applied on jump
    public float partialJumpGravMult; // gravity multiplier when player releases jump
    [Range(0f, 1)] public float jHangGravMult; // reduces gravity when near max height
    public float jHangTime; // time in which player is in hang time
    [Space(5)]
    public float jHangAccel; // acceleration in hang time
    public float jHangMaxSpeed; // max speed in hang time
    [Range(0.01f, 0.5f)] public float jBuffTime; // time before hitting ground where your jump still counts
    [Range(0.01f, 0.5f)] public float coyoteTime; // time after falling off ledge in which you can still jump

    [Header("Wall Jump")]
    [Range(0f, 1.5f)]public float wallJTime;
    public Vector2 wallJForce;
    public bool turnOnWall;

    [Space(20)]

    [Header("Dash")]
    public int dashUses;
    public int dashSpeed; 
    public float dashSleepTime; // time for game freeze on dash
    [Space(5)]
    public float dashAttackTime;
    [Space(5)]
    public float dashEndTime; // time after drag to idle
    public Vector2 dashEndSpeed; // slows down player after dash
    [Range(0f,1f)] public float dashEndRunLerp; // slows player input while dashing
    [Space(5)]
    public float dashCD; // time before next use of dash
    [Space(5)]
    [Range(0.01f, 0.5f)] public float dashInputBufferTime;





    
     
    private void OnValidate()
    {
        // calculate movement speeds
        runAccelVal = (50 * runAccel) / runMaxSpeed;
        runDeccelVal = (50 * runDeccel) / runMaxSpeed;
        
        // calculate strength of gravity
        gStrength = -(2 * jHeight) / (jTimeToMax * jTimeToMax);

        // scale of gravity on rigid body
        gScale = gStrength / Physics2D.gravity.y;

        // calculate upward force of jump
        jForce = Mathf.Abs(gStrength) * jTimeToMax;
         

        // Clamp takes the parameters (value, min, max) as input and returns result between min and max
        #region Variable Ranges
        runAccel = Mathf.Clamp(runAccel, 0.01f, runMaxSpeed);
        runDeccel = Mathf.Clamp(runDeccel, 0.01f, runMaxSpeed);
        #endregion
    }

}
