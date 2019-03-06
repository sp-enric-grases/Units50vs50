using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.UI;

namespace SocialPoint.Tools
{
    public class CameraOrbit_v2 : MonoBehaviour
    {
        #region CONSTANTS & VARIABLES

        const int MAX_ANGLE = 360;
        const int SCALE_FACTOR_SENSIBILITY = 1000;
        const float ZOOM_THRESHOLD = 10f;
        const float ZOOM_SENSIBILITY = 100;
        const float PAN_SENSIBILITY = 10;

        public enum TypeOfAction { None, Orbit, Zoom, Pan }
        public Camera alternativeCamera;
        public bool useAlternativeTarget;
        public Transform alternativeTarget;
        public Vector3 targetPosition;
        public float distance = 5.0f;
        public Vector2 limitX = new Vector2(0, 360);
        public Vector2 limitY = new Vector2(-85, 85);
        public Vector2 limitCam = new Vector2(5, 500);
        public float sensibility = 3.5f;
        public bool activeSmoothness = true;
        public float deacceleration = 2;
        public float zoomSensibility = 0.3f;
        public float panSensibility = 0.5f;
        public bool automaticMovement = false;
        public float timeToStartMoving = 10;
        public float speedMovement = 0.5f;
        public enum Direction { Left, Right, Random }
        public Direction direction = Direction.Random;

        private Camera mainCamera;
        private Transform mainTarget;
        private bool cameraIsValid = false;
        private float rotationYAxis = 0.0f;
        private float rotationXAxis = 0.0f;
        private float velocityX = 0.0f;
        private float velocityY = 0.0f;
        private TypeOfAction typeOfAction = TypeOfAction.None;
        float counter = 0;
        bool directionIsSetted = false;
        float dir;


        public Text aaa,bbb, ccc;

        #endregion

        void Start()
        {
            CheckForCamera();

            if (cameraIsValid)
            {
                Vector3 angles = mainCamera.transform.eulerAngles;
                rotationYAxis = angles.y;
                rotationXAxis = angles.x;

                CheckForTarget();
            }
        }

        private void CheckForCamera()
        {
            if (alternativeCamera == null)
                if (GetComponent<Camera>() == null)
                {
                    Debug.LogError("Component '<b>Camera</b>' not found in this GameObject. Please, assign a camera into <b>Alternative Camera</b> variable or add a <b>Camera</b> component.");
                    return;
                }
                else mainCamera = GetComponent<Camera>();
            else mainCamera = alternativeCamera;

            if (!mainCamera.CompareTag("MainCamera"))
            {
                Debug.LogError("The camera has to be tagged as '<b>MainCamera</b>'");
                return;
            }

            cameraIsValid = true;
        }

        private void CheckForTarget()
        {
            if (useAlternativeTarget)   CheckTarget();
            else                        CreateTarget();
        }

        private void CreateTarget()
        {
            Debug.Log("<color=cyan>[CAMERA ORBIT]</color> '<b>Target</b>' doesn't exist. Creating a target automatically...");
            GameObject target = new GameObject("target");
            target.transform.position = targetPosition;
            RotationConstraint rotConst = target.AddComponent<RotationConstraint>();
            SettingConstrintProperties(target.GetComponent<RotationConstraint>());

            mainTarget = target.transform;
        }

        private void CheckTarget()
        {
            if (alternativeTarget.GetComponent<RotationConstraint>() == null)
            {
                Debug.Log("<color=cyan>[CAMERA ORBIT]</color> There is no component '<b>RotationConstraint</b>' attached to the target.\nCreating the component automatically...");
                alternativeTarget.rotation = Quaternion.identity;
                RotationConstraint rotConst = alternativeTarget.gameObject.AddComponent<RotationConstraint>();
            }

            SettingConstrintProperties(alternativeTarget.GetComponent<RotationConstraint>());
            mainTarget = alternativeTarget.transform;
        }

        private void SettingConstrintProperties(RotationConstraint rotConst)
        {
            ConstraintSource constraintSource = new ConstraintSource() { sourceTransform = mainCamera.transform, weight = 1 };
            rotConst.constraintActive = true;
            rotConst.AddSource(constraintSource);
            rotConst.rotationAxis = Axis.None;
            rotConst.rotationAxis = Axis.Y;
            rotConst.rotationOffset = Vector3.zero;
        }

        void LateUpdate()
        {
            if (!cameraIsValid) return;

            counter += Time.deltaTime;

            if (counter >= timeToStartMoving)
                MoveAutomatically();

#if UNITY_STANDALONE || UNITY_EDITOR

            OrbitCamera_StandAlone();
            PanCamera_StandAlone();
            ZoomCamera_StandAlone();

#elif UNITY_IOS || UNITY_ANDROID

            ResetCamera_Device();
            OrbitCamera_Device();
            TwoFingersBehaviour();
            OrbitCameraImplementation();

            //ccc.text = typeOfAction.ToString();
#endif
        }

        #region CONTROLS FOR STANDALONE & UNITY EDITOR

        private void OrbitCamera_StandAlone()
        {
            if (Input.GetMouseButton(0))
            {
                ResetMovement();
                UpdateMouseVelocity(new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")));
            }
        }

        private void PanCamera_StandAlone()
        {
            if (Input.GetMouseButton(2))
            {
                ResetMovement();
                mainTarget.transform.Translate(-Input.GetAxis("Mouse X"), 0, -Input.GetAxis("Mouse Y"), Space.Self);
                mainCamera.transform.Translate(-Input.GetAxis("Mouse X"), 0, -Input.GetAxis("Mouse Y"), mainTarget.transform);
            }
        }

        private void ZoomCamera_StandAlone()
        {
            distance -= (Input.mouseScrollDelta.y * zoomSensibility * distance);
            ClampNearFarCamera();
            OrbitCameraImplementation();
        }

        #endregion

        #region CONTROLS FOR IOS & ANDROID

        private void ResetCamera_Device()
        {
            if (Input.touchCount == 0) typeOfAction = TypeOfAction.None;
        }

        private void OrbitCamera_Device()
        {
            if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Moved)
            {
                ResetMovement();
                UpdateMouseVelocity(Input.GetTouch(0).deltaPosition);
                typeOfAction = TypeOfAction.Orbit;
            }
        }

        private void TwoFingersBehaviour()
        {
            if (Input.touchCount == 2)
            {
                ResetMovement();

                Touch t0 = Input.GetTouch(0);
                Touch t1 = Input.GetTouch(1);

                t0.radius = 5;
                t1.radius = 5;

                Vector2 t0PrevPosition = t0.position - t0.deltaPosition;
                Vector2 t1PrevPosition = t1.position - t1.deltaPosition;

                float distPrevPositions = (t0PrevPosition - t1PrevPosition).magnitude;
                float distPositions = (t0.position - t1.position).magnitude;
                float difference = distPrevPositions - distPositions;

                //aaa.text = distPrevPositions.ToString();// (t0.deltaPosition.x - t0.deltaPosition.y).ToString();
                //bbb.text = distPositions.ToString(); //(t1.deltaPosition.x - t1.deltaPosition.y).ToString();

                if (typeOfAction == TypeOfAction.Zoom)
                    ZoomCamera_Device(difference);
                else if (Mathf.Abs(difference) > ZOOM_THRESHOLD && typeOfAction != TypeOfAction.Pan)
                    ZoomCamera_Device(difference, ZOOM_THRESHOLD);
                else if (t0.phase == TouchPhase.Moved && t1.phase == TouchPhase.Moved)
                    PanCamera_Device(t0, t1);
                else if(t0.phase == TouchPhase.Stationary && t1.phase == TouchPhase.Stationary)
                    typeOfAction = TypeOfAction.None;
            }
        }

        private void ZoomCamera_Device(float diff, float threshold = 0)
        {
            typeOfAction = TypeOfAction.Zoom;

            distance += ((diff > 0) ? diff - threshold : diff + threshold) * zoomSensibility * distance / ZOOM_SENSIBILITY;
            ClampNearFarCamera();
        }

        private void PanCamera_Device(Touch t0, Touch t1)
        {
            typeOfAction = TypeOfAction.Pan;

            float inputX = (t0.deltaPosition.x + t1.deltaPosition.x) / 2 * Time.deltaTime * panSensibility * distance / PAN_SENSIBILITY;
            float inputY = (t0.deltaPosition.y + t1.deltaPosition.y) / 2 * Time.deltaTime * panSensibility * distance / PAN_SENSIBILITY;

            mainTarget.transform.Translate(-inputX, 0, -inputY, Space.Self);
            mainCamera.transform.Translate(-inputX, 0, -inputY, mainTarget.transform);
        }

        #endregion

        private void UpdateMouseVelocity(Vector2 pos)
        {
            float touchDeltax = pos.x / Camera.main.pixelWidth * Time.deltaTime;
            float touchDeltay = pos.y / Camera.main.pixelHeight * Time.deltaTime;

            velocityX += touchDeltax * sensibility * SCALE_FACTOR_SENSIBILITY;
            velocityY += touchDeltay * sensibility * SCALE_FACTOR_SENSIBILITY;
        }

        private void OrbitCameraImplementation()
        {
            rotationYAxis += velocityX;
            rotationXAxis -= velocityY;
            rotationYAxis = ClampAngleY(rotationYAxis, limitX.x, limitX.y);
            rotationXAxis = ClampAngleX(rotationXAxis, limitY.x, limitY.y);

            Quaternion fromRotation = Quaternion.Euler(mainCamera.transform.rotation.eulerAngles.x, mainCamera.transform.rotation.eulerAngles.y, 0);
            Quaternion toRotation = Quaternion.Euler(rotationXAxis, rotationYAxis, 0);
            Quaternion rotation = toRotation;

            Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
            Vector3 position = rotation * negDistance + mainTarget.position;

            mainCamera.transform.rotation = rotation;
            mainCamera.transform.position = position;

            velocityX = activeSmoothness ? Mathf.Lerp(velocityX, 0, Time.deltaTime * deacceleration) : 0;
            velocityY = activeSmoothness ? Mathf.Lerp(velocityY, 0, Time.deltaTime * deacceleration) : 0;
        }

        public float ClampAngleX(float angle, float min, float max)
        {
            if (angle <= -MAX_ANGLE) angle += MAX_ANGLE;
            if (angle > MAX_ANGLE) angle -= MAX_ANGLE;

            return Mathf.Clamp(angle, min, max);
        }

        public float ClampAngleY(float angle, float min, float max)
        {
            if (angle <= 0f) angle += MAX_ANGLE;
            if (angle > MAX_ANGLE) angle -= MAX_ANGLE;

            return Mathf.Clamp(angle, min, max);
        }

        private void ClampNearFarCamera()
        {
            distance = Mathf.Clamp(distance, limitCam.x, limitCam.y);
        }

        private void MoveAutomatically()
        {
            if (!automaticMovement) return;

            if (!directionIsSetted)
            {
                dir = GetDirection();
                directionIsSetted = true;
            }

            velocityX += Time.deltaTime * speedMovement * dir;
        }

        private int GetDirection()
        {
            switch (direction)
            {
                case Direction.Left: return 1;
                case Direction.Right: return -1;
                default: return (Random.Range(0, 2) * 2) - 1;
            }
        }

        private void ResetMovement()
        {
            directionIsSetted = false;
            counter = 0;
        }

        void OnDrawGizmosSelected()
        {
            Vector3 origin = alternativeCamera == null ? transform.position : alternativeCamera.transform.position;
            Vector3 end = (useAlternativeTarget && alternativeTarget != null) ? alternativeTarget.transform.position : targetPosition;

            Gizmos.color = Color.blue;
            Gizmos.DrawLine(origin, end);

            if (!useAlternativeTarget)
            {
                Gizmos.color = new Color(0.2f, 1, 1, 0.7f);
                Gizmos.DrawSphere(targetPosition, 0.3f);
            }
        }
    }
}