using UnityEngine;
using UnityEngine.InputSystem;

public class Car : MonoBehaviour {
    public Rigidbody rigid;
    public WheelCollider wheel1, wheel2, wheel3, wheel4;
    public float driveSpeed, steerSpeed;
    private float goingSpeed;

    public float accSpeed, dccSpeed;

    private float XInput, YInput, CYInput, PYInput;
    private bool CYPositive, PYPositive, IsInputting, IsBraking;

    private InputAction MoveAction;
    
    private void Awake() {
        MoveAction = InputSystem.actions.FindAction("Move");
    }

    void Update() {
        // Inputs
        XInput = MoveAction.ReadValue<Vector2>().x;
        YInput = MoveAction.ReadValue<Vector2>().y;
    }

    void FixedUpdate() {
        // Check for input, and compare current vs past
        if (YInput >= 0.25f ^ YInput <= -0.25f) {
            IsInputting = true;
            CYInput = YInput;
            CYPositive = CYInput > 0;
            PYPositive = PYInput > 0;
            
            PYInput = CYInput;
            // * Reference C1
        } else {
            IsInputting = false;
        }
        
        // Acceleration/Speed/Direction Handling
        if (IsInputting & CYPositive & !PYPositive) {
            goingSpeed = 0f;
            rigid.AddRelativeForce(Vector3.forward * driveSpeed);
            wheel1.brakeTorque = goingSpeed;
            wheel2.brakeTorque = goingSpeed;
            wheel3.brakeTorque = goingSpeed;
            wheel4.brakeTorque = goingSpeed;
            IsBraking = true;
            
        } else if (IsInputting & !CYPositive & PYPositive) {
            goingSpeed = 0f;
            rigid.AddRelativeForce(Vector3.back * driveSpeed);
            wheel1.brakeTorque = goingSpeed;
            wheel2.brakeTorque = goingSpeed;
            wheel3.brakeTorque = goingSpeed;
            wheel4.brakeTorque = goingSpeed;
            IsBraking = true;
            
        } else if (IsInputting) {
            goingSpeed += YInput * (driveSpeed / accSpeed);
            IsBraking = false;

        } else {
            goingSpeed = 0f;
            IsBraking = true;

        }
        
        goingSpeed = Mathf.Clamp(goingSpeed, -driveSpeed, driveSpeed);
        
        // Driving
        float motor = goingSpeed;

        if (!IsBraking) {
            wheel3.motorTorque = motor;
            wheel4.motorTorque = motor;
        } else { 
            // should only run 1 frame
            wheel3.brakeTorque = motor;
            wheel4.brakeTorque = motor;
        }
        
        wheel1.steerAngle = steerSpeed * XInput;
        wheel2.steerAngle = steerSpeed * XInput;
    }
}


/*
 * *** Notes ***
 *
 * * C1:
 * if CYPositive is true, and PYPositive is false, player is moving forwards after going backward -> CASE 1
 * if CYPositive is false, and PYPositive is true, player is moving backward after going forward  -> CASE 2
 * if both are true or false, then the player has not released inputs and is continuing movement  -> CASE 3
 * if IsInputting is false, then the player will just slow                                        -> CASE 4
 *
 * check is conducted frame-by-frame, assuming the player has a valid YInput.
 * the player cannot hold both, because this will not trigger the check (^ -> XOR)
 * the player cannot hold none, because then neither is >/< 0.25f
 * *
 */