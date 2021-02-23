using UnityEngine;
using MLAgents;

public class MaulwurfAgentV2_5 : Agent
{
    #region Properties
    //Start
    private RaycastHit TargetPoint;
    private RaycastHit NextPoint;

    //Reset
    public Transform Target;
    public Transform Spawn;

    //CollectData + Orientation
    public float MaxDistance = 5.0f;
    private Vector3 AgentOrigin;

    private float DistanceForward = 5.0f;
    private float DistanceBackward = 5.0f;
    private float DistanceLeft = 5.0f;
    private float DistanceRight = 5.0f;
    private float DistanceFrontRight = 5.0f;
    private float DistanceFrontLeft = 5.0f;

    private RaycastHit RaycastHitForward;
    private RaycastHit RaycastHitBackward;
    private RaycastHit RaycastHitLeft;
    private RaycastHit RaycastHitRight;
    private RaycastHit RaycastHitFrontRight;
    private RaycastHit RaycastHitFrontLeft;

    private Vector3 RaycastHitForwardEnd;
    private Vector3 RaycastHitBackwardEnd;
    private Vector3 RaycastHitRightEnd;
    private Vector3 RaycastHitLeftEnd;
    private Vector3 RaycastHitFrontRightEnd;
    private Vector3 RaycastHitFrontLeftEnd;

    private Vector3 AgentForward;
    private Vector3 AgentBackward;
    private Vector3 AgentLeft;
    private Vector3 AgentRight;
    private Vector3 AgentFrontRight;
    private Vector3 AgentFrontLeft;

    //Action
    public float speed = 0.02f;
    private float DistanceToTargetPoint = float.MaxValue;
    private float PreviousDistancetoTargetPoint = float.MaxValue;
    #endregion

    //initializing Target- and NextPoint to ForwardHit
    void Start()
    {
        Physics.Raycast(this.transform.position, this.transform.forward, out TargetPoint, MaxDistance);
        Physics.Raycast(this.transform.position, this.transform.forward, out NextPoint, MaxDistance);
    }

    private void Update()
    {

        //draw the rays one per frame
        Debug.DrawRay(AgentOrigin, RaycastHitForwardEnd - AgentOrigin, Color.green);
        Debug.DrawRay(AgentOrigin, RaycastHitBackwardEnd - AgentOrigin, Color.green);
        Debug.DrawRay(AgentOrigin, RaycastHitLeftEnd - AgentOrigin, Color.green);
        Debug.DrawRay(AgentOrigin, RaycastHitRightEnd - AgentOrigin, Color.green);
        Debug.DrawRay(AgentOrigin, RaycastHitFrontRightEnd - AgentOrigin, Color.green);
        Debug.DrawRay(AgentOrigin, RaycastHitFrontLeftEnd - AgentOrigin, Color.green);

        DistanceToTargetPoint = Vector3.Distance(this.transform.position, TargetPoint.transform.position);
    }

    public override void AgentReset()
    {

        // Move the agent back to spawn
        this.transform.position = Spawn.position;
        this.transform.rotation = Spawn.rotation;
        Physics.Raycast(this.transform.position, this.transform.forward, out TargetPoint, MaxDistance);
        Physics.Raycast(this.transform.position, this.transform.forward, out NextPoint, MaxDistance);

    }

    //collecting data and handing it to neural network
    public override void CollectObservations()
    {
        AgentForward = transform.forward;
        AgentBackward = -this.transform.forward;
        AgentLeft = -this.transform.right;
        AgentRight = this.transform.right;
        AgentFrontRight = this.transform.right + this.transform.forward;
        AgentFrontLeft = -this.transform.right + this.transform.forward;

        AgentOrigin = this.transform.position;

        if (Physics.Raycast(AgentOrigin, AgentForward, out RaycastHitForward, MaxDistance))
        {
            DistanceForward = RaycastHitForward.distance;
            RaycastHitForwardEnd = RaycastHitForward.point;

        }

        if (Physics.Raycast(AgentOrigin, AgentBackward, out RaycastHitBackward, MaxDistance))
        {
            DistanceBackward = RaycastHitBackward.distance;
            RaycastHitBackwardEnd = RaycastHitBackward.point;
        }

        if (Physics.Raycast(AgentOrigin, AgentLeft, out RaycastHitLeft, MaxDistance))
        {
            DistanceLeft = RaycastHitLeft.distance;
            RaycastHitLeftEnd = RaycastHitLeft.point;
        }

        if (Physics.Raycast(AgentOrigin, AgentRight, out RaycastHitRight, MaxDistance))
        {
            DistanceRight = RaycastHitRight.distance;
            RaycastHitRightEnd = RaycastHitRight.point;
        }

        if (Physics.Raycast(AgentOrigin, AgentFrontRight, out RaycastHitFrontRight, MaxDistance))
        {
            DistanceFrontRight = RaycastHitFrontRight.distance;
            RaycastHitFrontRightEnd = RaycastHitFrontRight.point;
        }

        if (Physics.Raycast(AgentOrigin, AgentFrontLeft, out RaycastHitFrontLeft, MaxDistance))
        {
            DistanceFrontLeft = RaycastHitFrontLeft.distance;
            RaycastHitFrontLeftEnd = RaycastHitFrontLeft.point;
        }

        // adding distances to observations
        AddVectorObs(DistanceForward / 5);
        AddVectorObs(DistanceBackward / 5);
        AddVectorObs(DistanceLeft / 5);
        AddVectorObs(DistanceRight / 5);
        AddVectorObs(DistanceFrontRight / 5);
        AddVectorObs(DistanceFrontLeft / 5);

        AddVectorObs(DistanceToTargetPoint / 5);
    }

    //Execute agents actions
    public override void AgentAction(float[] vectorAction, string textAction)
    {

        // Rewards
        float distanceToTarget = Vector3.Distance(this.transform.position, Target.transform.position);

        //Move towards the farest point
        if (TargetPoint.transform != null)
            DistanceToTargetPoint = Vector3.Distance(this.transform.position, TargetPoint.transform.position);

        if (DistanceToTargetPoint < PreviousDistancetoTargetPoint)
        {
            AddReward(0.15f);
            Debug.Log("Reward 0.15 for Distance");
        }

        PreviousDistancetoTargetPoint = DistanceToTargetPoint;

        if (NextPoint.transform != null)
            TargetPoint = NextPoint;

        //if (TargetPoint.transform != Target)
        //{
        if (DistanceForward > DistanceFrontLeft && DistanceForward > DistanceFrontRight)
            NextPoint = RaycastHitForward;
        if (DistanceFrontLeft > DistanceForward && DistanceFrontLeft > DistanceFrontRight)
            NextPoint = RaycastHitFrontLeft;
        if (DistanceFrontRight > DistanceForward && DistanceFrontRight > DistanceFrontLeft)
            NextPoint = RaycastHitFrontRight;
        //}
        //else
        //{
        //    NextPoint = TargetPoint;
        //}

        // Time penalty
        AddReward(-0.1f);
        Debug.Log("Reward -0.1 Time Penalty");
        // AddReward(0.05f * DistanceForward);
        // AddReward(0.02f * DistanceLeft);
        // AddReward(0.02f * DistanceRight);

        // Actions, size = 2
        float controlSignalMovement;
        Vector3 controlSignalRotation = Vector3.zero;

        controlSignalMovement = Mathf.Clamp(vectorAction[0], -1, 1);
        controlSignalRotation.y = Mathf.Clamp(vectorAction[1], -1, 1);

        Debug.Log("Control Signal Movement: " + controlSignalMovement);
        Debug.Log("Control Signal Rotation: " + controlSignalRotation.y);

        this.transform.eulerAngles += controlSignalRotation * 3;
        this.transform.position += this.transform.forward * controlSignalMovement * speed;
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.name == "finish")
        {
            Done();
            AddReward(25.0f);
            Debug.Log("Reward 25.0 for Reaching Target");
        }
        else
        {
            Done();
            AddReward(-5.0f);
            Debug.Log("Reward -5.0 for Hitting Wall");
        }

    }
}
