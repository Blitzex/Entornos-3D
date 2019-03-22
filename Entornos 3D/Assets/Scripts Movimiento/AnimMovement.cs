using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class AnimMovement : MonoBehaviour
{
    #region Variables
    public float rotationSpeed = 4f;
    public float rotationThreshold = 0.3f;
    public float degreesToTurn = 180f;

    public float height;

    public bool turn180;
    public bool mirrorIdle;
    public bool grounded;
    public bool canJump;
    

    [Header("Animator Parameters")]
    public string motionParam = "motion";
    public string mirrorIdleParam = "mirrorIdle";
    public string turn180Param = "turn180";
    public string jumpParam = "Jumping";
    public string groundParam = "isGrounded";

    [Header("Animation Smoothing")]
    [Range(0, 1f)]
    public float StartAnimTime = 0.3f;
    [Range(0, 1f)]
    public float StopAnimTime = 0.15f;

    [Header("Physics control")]
    [SerializeField]
    public float gravity = 14.0f;
    public float horizontalSpeed;
    private float _velocityY;
    private float Speed;

    private Vector3 Direction;
    private CharacterController characterController;
    private Animator animator;
    #endregion

    #region IkVariables

    private Vector3 rightFootPosition, leftFootPosition, leftFootIkPosition, rightFootIkPosition;
    private Quaternion leftFootIkRotation, rightFootIkRotation;
    private float lastPelvisPositionY, lastRightFootPositionY, lastLeftFootPositionY;

    [Header("Feet Grounder")]
    public bool enableFeetIk = true;
    [Range(0, 2)] [SerializeField] private float heightFromGroundRaycast = 1.14f;
    [Range(0, 2)] [SerializeField] private float raycastDownDistance = 1.5f;
    [SerializeField] private LayerMask environmentLayer;
    [SerializeField] private float pelvisOffset = 0f;
    [Range(0, 1)] [SerializeField] private float pelvisUpAndDownSpeed = 0.28f;
    [Range(0, 1)] [SerializeField] private float feetToIkPositionSpeed = 0.5f;

    public string leftFootAnimVariableName = "LeftFootCurve";
    public string rightFootAnimVariableName = "RightFootCurve";

    public bool useProIkFeature = false;
    public bool showSolverDebug = true;
    #endregion

    //public Transform terrain;
    //public Transform feetGroundCheck;
    //public float distance;
    private Vector3 jumpVector = Vector3.zero;
    public float jumpSpeed = 8.0F;
    // Start is called before the first frame update
    void Start()
    {
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    }

    public void moveCharacter(float hInput, float vInput, Camera cam, bool jump, bool dash)
    {

        if (grounded == true)
        {

            SomethingInFront();
            //distanceFromTheGround();

            #region ground movement
            //Calculate the input magnitude
            Speed = new Vector2(hInput, vInput).normalized.sqrMagnitude;

            //Dash only if character has reached maxSped (animator)
            if (Speed == 1 && dash)
            {
                Speed = 2;
            }

            //Physically move player
            if (Speed > rotationThreshold)
            {
                animator.SetFloat(motionParam, Speed, StartAnimTime, Time.deltaTime);
                Vector3 forward = cam.transform.forward;
                Vector3 right = cam.transform.right;

                forward.y = 0f;
                right.y = 0f;

                forward.Normalize();
                right.Normalize();

                //Rotation character
                Direction = forward * vInput + right * hInput;

                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(Direction),
                                      rotationSpeed * Time.deltaTime);
                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                if (Vector3.Angle(transform.forward, Direction) >= degreesToTurn)
                {
                    turn180 = true;
                }
                else
                {
                    turn180 = false;
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(Direction), rotationSpeed * Time.deltaTime);
                }

                //180 turning
                animator.SetBool(turn180Param, turn180);

                //move character
                animator.SetFloat(motionParam, Speed, StartAnimTime, Time.deltaTime);

            }

            else if (Speed < rotationThreshold)
            {
                animator.SetBool(mirrorIdleParam, mirrorIdle);
                animator.SetFloat(motionParam, Speed, StopAnimTime, Time.deltaTime);
            }


            #endregion
            canJump = true;
            _velocityY = -gravity * Time.deltaTime;
            if (jump && canJump)
            {
                //jumpVector.y = jumpSpeed;
                animator.SetTrigger(jumpParam);
            }
        }

        else
        {
            canJump = false;
            _velocityY -= gravity * Time.deltaTime;
            //jumpVector.y -= gravity * Time.deltaTime;
        }
        characterController.Move(jumpVector * Time.deltaTime);

        animator.SetBool(groundParam, grounded);
        //_velocityY -= gravity * Time.deltaTime;
    }

    //void distanceFromTheGround()
    //{
    //    distance = terrain.position.y - feetGroundCheck.position.y;

    //}

    private void SomethingInFront()
    {
        Vector3 horizontalVelocity = characterController.velocity;
        horizontalVelocity = new Vector2(characterController.velocity.x, characterController.velocity.z);
        horizontalSpeed = horizontalVelocity.magnitude * 0.5f;

        if (horizontalSpeed <= 0.5f)
        {
            Speed = 0;
            animator.SetFloat(motionParam, Speed);
        }


    }


    #region FeetGrounding

    private void FixedUpdate()
    {
        if (enableFeetIk == false) { return; }
        if (animator == null) { return; }

        AdjustFeetTarget(ref rightFootPosition, HumanBodyBones.RightFoot);
        AdjustFeetTarget(ref leftFootPosition, HumanBodyBones.LeftFoot);

        //find and raycast to the ground to find positions
        FeetPositionSolver(rightFootPosition, ref rightFootIkPosition, ref rightFootIkRotation); // handle the solver for right foot
        FeetPositionSolver(leftFootPosition, ref leftFootIkPosition, ref leftFootIkRotation); //handle the solver for the left foot

    }

    /// <summary>
    /// updating the adjustFeetTarget method and find the position of each ins our solver position.
    /// </summary>
    private void OnAnimatorIK(int layerIndex)
    {
        #region mirrorIdle
        if (Speed < rotationThreshold) return;

        float distanceToLeftFoot = Vector3.Distance(transform.position, animator.GetIKPosition(AvatarIKGoal.LeftFoot));
        float distanceToRightFoot = Vector3.Distance(transform.position, animator.GetIKPosition(AvatarIKGoal.RightFoot));


        //right foot in front
        if (distanceToRightFoot > distanceToLeftFoot)
        {
            mirrorIdle = true;
        }
        //R foot behind
        else { mirrorIdle = false; }
        #endregion

        if (enableFeetIk == false) { return; }
        if (animator == null) { return; }

        MovePelvisHeight();

        //right foot ik position and rotation -- utilise the pro features in here
        animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1);

        if (useProIkFeature)
        {
            animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, animator.GetFloat(rightFootAnimVariableName));
        }

        MoveFeetToIkPoint(AvatarIKGoal.RightFoot, rightFootIkPosition, rightFootIkRotation, ref lastRightFootPositionY);

        //left foot ik position and rotation -- utilise the pro features in here
        animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1);

        if (useProIkFeature)
        {
            animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, animator.GetFloat(leftFootAnimVariableName));
        }

        MoveFeetToIkPoint(AvatarIKGoal.LeftFoot, leftFootIkPosition, leftFootIkRotation, ref lastLeftFootPositionY);
    }
    #endregion

    #region FeetGroundingMethods

    /// <summary>
    /// Moves the feet to ik point.
    /// </summary>
    /// <param name="foot">Foot.</param>
    /// <param name="positionIkHolder">Position ik holder.</param>
    /// <param name="rotationIkHolder">Rotation ik holder.</param>
    /// <param name="lastFootPositionY">Last foot position y.</param>
    void MoveFeetToIkPoint(AvatarIKGoal foot, Vector3 positionIkHolder, Quaternion rotationIkHolder, ref float lastFootPositionY)
    {
        Vector3 targetIkPosition = animator.GetIKPosition(foot);

        if (positionIkHolder != Vector3.zero)
        {
            targetIkPosition = transform.InverseTransformPoint(targetIkPosition);
            positionIkHolder = transform.InverseTransformPoint(positionIkHolder);

            float yVariable = Mathf.Lerp(lastFootPositionY, positionIkHolder.y, feetToIkPositionSpeed);
            targetIkPosition.y += yVariable;

            lastFootPositionY = yVariable;

            targetIkPosition = transform.TransformPoint(targetIkPosition);

            animator.SetIKRotation(foot, rotationIkHolder);
        }

        animator.SetIKPosition(foot, targetIkPosition);
    }
    /// <summary>
    /// Moves the height of the pelvis.
    /// </summary>
    private void MovePelvisHeight()
    {

        if (rightFootIkPosition == Vector3.zero || leftFootIkPosition == Vector3.zero || lastPelvisPositionY == 0)
        {
            lastPelvisPositionY = animator.bodyPosition.y;
            return;
        }

        float lOffsetPosition = leftFootIkPosition.y - transform.position.y;
        float rOffsetPosition = rightFootIkPosition.y - transform.position.y;

        float totalOffset = (lOffsetPosition < rOffsetPosition) ? lOffsetPosition : rOffsetPosition;

        Vector3 newPelvisPosition = animator.bodyPosition + Vector3.up * totalOffset;

        newPelvisPosition.y = Mathf.Lerp(lastPelvisPositionY, newPelvisPosition.y, pelvisUpAndDownSpeed);

        animator.bodyPosition = newPelvisPosition;

        lastPelvisPositionY = animator.bodyPosition.y;

    }

    /// <summary>
    /// We are locating the Feet position via a Raycast and then Solving
    /// </summary>
    /// <param name="fromSkyPosition">From sky position.</param>
    /// <param name="feetIkPositions">Feet ik positions.</param>
    /// <param name="feetIkRotations">Feet ik rotations.</param>
    private void FeetPositionSolver(Vector3 fromSkyPosition, ref Vector3 feetIkPositions, ref Quaternion feetIkRotations)
    {
        //raycast handling section 
        RaycastHit feetOutHit;

        if (showSolverDebug)
            Debug.DrawLine(fromSkyPosition, fromSkyPosition + Vector3.down * (raycastDownDistance + heightFromGroundRaycast), Color.red);

        if (Physics.Raycast(fromSkyPosition, Vector3.down, out feetOutHit, raycastDownDistance + heightFromGroundRaycast, environmentLayer))
        {
            //finding our feet ik positions from the sky position
            feetIkPositions = fromSkyPosition;
            feetIkPositions.y = feetOutHit.point.y + pelvisOffset;
            feetIkRotations = Quaternion.FromToRotation(Vector3.up, feetOutHit.normal) * transform.rotation;

            height = feetOutHit.distance;
            grounded = true;
            return;
        }
        else { grounded = false;}

        feetIkPositions = Vector3.zero; //it didn't work :(


    }
    /// <summary>
    /// Adjusts the feet target.
    /// </summary>
    /// <param name="feetPositions">Feet positions.</param>
    /// <param name="foot">Foot.</param>
    private void AdjustFeetTarget(ref Vector3 feetPositions, HumanBodyBones foot)
    {
        feetPositions = animator.GetBoneTransform(foot).position;
        feetPositions.y = transform.position.y + heightFromGroundRaycast;

    }

    #endregion




}
