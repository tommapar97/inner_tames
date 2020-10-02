using Common;
using PlayerTools.SpaceShipParts;
using UnityEngine;
using Universe;

namespace PlayerTools
{
    [RequireComponent(typeof(SpaceShipThrusters))]
    [RequireComponent(typeof(SpaceShipFlashlight))]
    public class SpaceShip : SpaceBody
    {
        [SerializeField] private SpaceShipAccelerationShowcase accelerationShowcase;
        private SpaceShipThrusters thrusters;

        // Player Input
        private Vector3 wantedMovement;
        private Vector3 wantedRotation;
        private bool wantsToRotateAroundForwardVector;

        // Movement
        [SerializeField] private float movementThrustersPower;
        [SerializeField] private float rotationThrustersPower;

        // Hatch
        [SerializeField] private SpaceShipHatch spaceShipHatch;
        [SerializeField] private GameObject hatchGravityField;

        // Flashlight
        public SpaceShipFlashlight flashlight;

        private new void Awake()
        {
            base.Awake();

            thrusters = GetComponent<SpaceShipThrusters>();
            flashlight = GetComponent<SpaceShipFlashlight>();
        }

        private void Update()
        {
            accelerationShowcase.UpdateAcceleration(wantedMovement);
            thrusters.Fire(wantedMovement);
        }

        private void FixedUpdate()
        {
            ApplyGravity();

            Move();
            Rotate();
        }

        public void Pilot(Vector3 playerWantedMovement, Vector2 playerWantedRotation, bool playerWantsToRotateAroundForwardVector)
        {
            wantedMovement = playerWantedMovement;
            wantedRotation = playerWantedRotation;
            wantsToRotateAroundForwardVector = playerWantsToRotateAroundForwardVector;
        }

        private void Move()
        {
            Transform cachedTransform = transform;

            // Get direction
            Vector3 motion = cachedTransform.forward * wantedMovement.x +
                             cachedTransform.up * wantedMovement.y +
                             cachedTransform.right * wantedMovement.z;

            motion *= movementThrustersPower; // Add power
            motion *= Time.deltaTime; // Add physics step

            rigidbody.AddForce(motion);
        }

        private void Rotate()
        {
            Vector2 rotation = wantedRotation; // Get direction
            rotation *= rotationThrustersPower; // Add more force
            rotation *= GameSettings.MouseSensitivity; // Apply mouse sensitivity settings
            rotation *= Time.deltaTime; // Include physics step

            rigidbody.AddTorque(transform.right * -rotation.y);

            if (wantsToRotateAroundForwardVector)
            {
                rigidbody.AddTorque(transform.forward * -rotation.x);
            }
            else
            {
                rigidbody.AddTorque(transform.up * rotation.x);
            }
        }

        public void PlayerEnteredShip()
        {
            if (spaceShipHatch.isClosed || !hatchGravityField.activeSelf)
            {
                return;
            }

            spaceShipHatch.Toggle();
        }

        public void PlayerExitedShip()
        {
            hatchGravityField.SetActive(true);
        }
    }
}