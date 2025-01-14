﻿using Celestial;
using PlayerLogic;
using TMPro;
using UnityEngine;

// I don't like a lot of inspector fields here, but I'm not sure how to do it better. Don't like initializing everything from the constructor either
namespace PlayerTools
{
    /**
     * Class that gives the player an ability to lock onto celestial bodies and get a lot of useful information like
     * relative speed, distance, relative acceleration, celestial body's name
     */
    public class SpaceNavigator : MonoBehaviour
    {
        // Cursors
        [SerializeField] private GameObject suggestCursor;
        [SerializeField] private GameObject lockCursor;

        // Suggest cursor arcs
        [SerializeField] private GameObject suggesterCursorTopRightArc;
        [SerializeField] private GameObject suggesterCursorTopLeftArc;
        [SerializeField] private GameObject suggesterCursorBottomRightArc;
        [SerializeField] private GameObject suggesterCursorBottomLeftArc;

        // Lock cursor arcs
        [SerializeField] private GameObject lockCursorTopRightArc;
        [SerializeField] private GameObject lockCursorTopLeftArc;
        [SerializeField] private GameObject lockCursorBottomRightArc;
        [SerializeField] private GameObject lockCursorBottomLeftArc;

        // Lock Arrow masks
        [SerializeField] private GameObject topVelocityArrowMask;
        [SerializeField] private GameObject rightVelocityArrowMask;
        [SerializeField] private GameObject bottomVelocityArrowMask;
        [SerializeField] private GameObject leftVelocityArrowMask;

        // Lock cursor velocity arrows
        [SerializeField] private GameObject topVelocityArrow;
        [SerializeField] private GameObject rightVelocityArrow;
        [SerializeField] private GameObject bottomVelocityArrow;
        [SerializeField] private GameObject leftVelocityArrow;

        // Lock information
        [SerializeField] private TextMeshProUGUI lockInformation;

        // Linked objects
        [SerializeField] private Player player;

        private CelestialBody lockedCelestialBody;
        private float previousDistance;

        private void Update()
        {
            ProcessUserInput();
        }

        private void FixedUpdate()
        {
            if (!IsLocked())
            {
                TryToSuggestCelestialBody();
            }
            else
            {
                UpdateLock();
            }
        }

        private void ProcessUserInput()
        {
            bool mouseLeftClicked = Input.GetMouseButtonDown(0);
            if (!mouseLeftClicked)
            {
                return;
            }

            if (HasSuggestedPlanet() && !IsLocked())
            {
                Lock();
            }
            else if (IsLocked())
            {
                Unlock();
            }
        }

        private void Lock()
        {
            lockedCelestialBody = GetSuggestedCelestialBody();
            UpdateLock();
            suggestCursor.SetActive(false);
        }

        private void Unlock()
        {
            lockCursor.SetActive(false);
            lockedCelestialBody = null;
        }

        private bool IsLocked()
        {
            return lockedCelestialBody != null;
        }

        private bool HasSuggestedPlanet()
        {
            return suggestCursor.activeSelf;
        }

        private void TryToSuggestCelestialBody()
        {
            CelestialBody suggestedCelestialBody = GetSuggestedCelestialBody();

            if (suggestedCelestialBody != null)
            {
                UpdateCursorCoordinates(suggestedCelestialBody);
                UpdateCursorSize(suggestedCelestialBody);
                suggestCursor.SetActive(true);
            }
            else
            {
                suggestCursor.SetActive(false);
            }
        }

        private void UpdateCursorSize(CelestialBody celestialBody)
        {
            float coordinateAddition = GetCursorPositionAddition(celestialBody);

            if (IsLocked())
            {
                lockCursorTopRightArc.transform.localPosition = new Vector3(coordinateAddition, coordinateAddition, 0);
                lockCursorTopLeftArc.transform.localPosition = new Vector3(-coordinateAddition, coordinateAddition, 0);
                lockCursorBottomLeftArc.transform.localPosition = new Vector3(-coordinateAddition, -coordinateAddition, 0);
                lockCursorBottomRightArc.transform.localPosition = new Vector3(coordinateAddition, -coordinateAddition, 0);
            }
            else
            {
                suggesterCursorTopRightArc.transform.localPosition = new Vector3(coordinateAddition, coordinateAddition, 0);
                suggesterCursorTopLeftArc.transform.localPosition = new Vector3(-coordinateAddition, coordinateAddition, 0);
                suggesterCursorBottomLeftArc.transform.localPosition = new Vector3(-coordinateAddition, -coordinateAddition, 0);
                suggesterCursorBottomRightArc.transform.localPosition = new Vector3(coordinateAddition, -coordinateAddition, 0);
            }

            float velocityArrowCoordinate = 1165 + coordinateAddition;
            topVelocityArrowMask.transform.localPosition = new Vector3(0, velocityArrowCoordinate, 0);
            leftVelocityArrowMask.transform.localPosition = new Vector3(-velocityArrowCoordinate, 0, 0);
            bottomVelocityArrowMask.transform.localPosition = new Vector3(0, -velocityArrowCoordinate, 0);
            rightVelocityArrowMask.transform.localPosition = new Vector3(velocityArrowCoordinate, 0, 0);

            float lockInformationX = 120f + coordinateAddition;
            lockInformation.transform.localPosition = new Vector3(lockInformationX, 40f, 0f);
        }

        private float GetCursorPositionAddition(CelestialBody celestialBody)
        {
            float distanceDiff = Mathf.Abs((celestialBody.rigidbody.position - player.rigidbody.position).magnitude);
            float addition = 90000f / distanceDiff * celestialBody.radius / 4f;

            float minSizeValue = 6f * celestialBody.radius;
            if (addition < minSizeValue)
            {
                addition = minSizeValue;
            }

            return addition;
        }

        private CelestialBody GetSuggestedCelestialBody()
        {
            Transform cachedCameraTransform = player.camera.transform;

            // Try to find something at the cursor
            RaycastHit[] raycastHits = UnityEngine.Physics.RaycastAll(cachedCameraTransform.position, cachedCameraTransform.forward, player.camera.farClipPlane);
            if (raycastHits.Length == 0)
            {
                return null;
            }

            foreach (RaycastHit raycastHit in raycastHits)
            {
                // We only need celestialBody
                CelestialBody celestialBody = raycastHit.transform.GetComponent<CelestialBody>();
                if (celestialBody != null)
                {
                    return celestialBody;
                }
            }

            return null;
        }

        private void UpdateLock()
        {
            // Without this cursor will be displayed when is it behind the camera
            if (IsLockedObjectBehindTheCamera())
            {
                lockCursor.SetActive(false);
                return;
            }
            else
            {
                lockCursor.SetActive(true);
            }

            UpdateCursorCoordinates(lockedCelestialBody);
            UpdateCursorSize(lockedCelestialBody);
            UpdateLockText();
            UpdateVelocityArrows();
        }

        private void UpdateCursorCoordinates(CelestialBody celestialBody)
        {
            Vector3 worldToScreenPoint = GetCelestialBodyOnScreenCoordinates(celestialBody);
            transform.position = worldToScreenPoint;
        }

        private Vector3 GetCelestialBodyOnScreenCoordinates(CelestialBody celestialBody)
        {
            Vector3 positionOnScreen = player.camera.WorldToScreenPoint(celestialBody.rigidbody.position);

            positionOnScreen.z = positionOnScreen.z > 0 ? 1 : -1; // If Z coordinate is too big, we don't see the ui. And we need to keep the minus in order to understand if ui is at the back

            return positionOnScreen;
        }

        private bool IsLockedObjectBehindTheCamera()
        {
            Vector3 celestialBodyCoordinates = GetCelestialBodyOnScreenCoordinates(lockedCelestialBody);

            return celestialBodyCoordinates.z < 0;
        }

        private void UpdateLockText()
        {
            string celestialBodyName = lockedCelestialBody.name;
            string distanceToCelestialBody = GetDistanceToCelestialBodyText();
            string velocityToCelestialBody = GetVelocityMagnitudeToCelestialBodyText();

            lockInformation.text = $"{celestialBodyName}\n{distanceToCelestialBody}\n{velocityToCelestialBody}";
        }

        private void UpdateVelocityArrows()
        {
            Vector3 velocityDifference = player.rigidbody.velocity - lockedCelestialBody.rigidbody.velocity;

            Vector3 playerEulerAngles = player.transform.rotation.eulerAngles;
            velocityDifference = Quaternion.Euler(0, 0, -playerEulerAngles.z) * velocityDifference;

            rightVelocityArrow.transform.localPosition = new Vector3(GetArrowLocalPosition(velocityDifference.x), 0f, 0f);
            leftVelocityArrow.transform.localPosition = new Vector3(GetArrowLocalPosition(-velocityDifference.x), 0f, 0f);
            topVelocityArrow.transform.localPosition = new Vector3(GetArrowLocalPosition(velocityDifference.y), 0f, 0f);
            bottomVelocityArrow.transform.localPosition = new Vector3(GetArrowLocalPosition(-velocityDifference.y), 0f, 0f);
        }

        /**
         * Arrow moving is implemented with a mask where arrow totally hidden by mask and gets revealed slowly
         */
        private static float GetArrowLocalPosition(float velocityCoordinateDifference)
        {
            const int nothingIsViewedValue = -100;
            const int scaleFactor = 100;

            float value = velocityCoordinateDifference / scaleFactor;

            return nothingIsViewedValue + value;
        }

        private string GetDistanceToCelestialBodyText()
        {
            float metersFromPlayerToCelestialBody = (player.rigidbody.position - lockedCelestialBody.rigidbody.position).magnitude;

            if (metersFromPlayerToCelestialBody > 5000)
            {
                return (metersFromPlayerToCelestialBody / 1000).ToString("####0") + "km";
            }
            else
            {
                return metersFromPlayerToCelestialBody.ToString("####0") + "m";
            }
        }

        private string GetVelocityMagnitudeToCelestialBodyText()
        {
            float velocityToCelestialBody = (player.rigidbody.velocity - lockedCelestialBody.rigidbody.velocity).magnitude;

            float currentDistance = (player.rigidbody.position - lockedCelestialBody.rigidbody.position).magnitude;
            string velocitySign = previousDistance < currentDistance ? "-" : "";
            previousDistance = currentDistance;

            return $"{velocitySign}{velocityToCelestialBody:####0}m/s";
        }
    }
}
