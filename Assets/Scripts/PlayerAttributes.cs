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

    [Header("Jump")]
    public float jHeight; // height of jump
    public float jTimeToMax; // length of time to reach max height after jumping
    [HideInInspector] public float jforce; // force applied on jump
    public float partialJumpGravMult; // gravity multiplier when player releases jump
    [Range(0f, 1)] public float jHangGravMult; // reduces gravity when near max height
    public float jHangTime; // time in which player is in hang time
    [space(5)]
    public float jHangAccel; // acceleration in hang time
    public float jHangMaxSpeed; // max speed in hang time
    
     
    private void OnValidate()
    {
        // calculate movement speeds
        runAccelVal = (50 * runAccel) / runMaxSpeed;
        runDeccelVal = (50 * runDeccel) / runMaxSpeed;
        

        // Clamp takes the parameters (value, min, max) as input and returns result between min and max
        #region Variable Ranges
        runAccel = Mathf.Clamp(runAccel, 0.01f, runMaxSpeed);
        runDeccel = Mathf.Clamp(runDeccel, 0.01f, runMaxSpeed);
        #endregion
    }

}
