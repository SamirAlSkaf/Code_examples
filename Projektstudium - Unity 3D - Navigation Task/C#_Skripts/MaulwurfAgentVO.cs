using System.Collections.Generic;
using UnityEngine;

public class MaulwurfAgentVO : Agent
{
    #region Start
    public RaycastHit TargetPoint;
    public RaycastHit NextPoint;

    //initializing Target- and NextPoint to ForwardHit
    void Start()
    {
        Physics.Raycast(this.transform.position, this.transform.forward, out TargetPoint, MaxDistance);
        Physics.Raycast(this.transform.position, this.transform.forward, out NextPoint, MaxDistance);
    }
    #endregion

    private void Update()
    {

        //draw the rays one per frame
        //Debug.DrawRay(AgentOrigin, RaycastHitForwardEnd - AgentOrigin, Color.green);
        //Debug.DrawRay(AgentOrigin, RaycastHitBackwardEnd - AgentOrigin, Color.green);
        //Debug.DrawRay(AgentOrigin, RaycastHitLeftEnd - AgentOrigin, Color.green);
        //Debug.DrawRay(AgentOrigin, RaycastHitRightEnd - AgentOrigin, Color.green);
        //Debug.DrawRay(AgentOrigin, RaycastHitFrontRightEnd - AgentOrigin, Color.green);
        //Debug.DrawRay(AgentOrigin, RaycastHitFrontLeftEnd - AgentOrigin, Color.green);

        DistanceToTargetPoint = Vector3.Distance(this.transform.position, TargetPoint.transform.position);
    }

    #region Reset
    public Transform Target;
    public Transform Spawn;

    public override void AgentReset()
    {

        // Move the agent back to spawn
        this.transform.position = Spawn.position;
        this.transform.rotation = Spawn.rotation;
        Physics.Raycast(this.transform.position, this.transform.forward, out TargetPoint, MaxDistance);
        Physics.Raycast(this.transform.position, this.transform.forward, out NextPoint, MaxDistance);

    }
    #endregion

    #region CollectData
    List<float> observation = new List<float>();
    public float MaxDistance = 5.0f;
    public Vector3 AgentOrigin;

    //everything is public - stupid but works ¯\_(ツ)_/¯ (only one instance)
    public float DistanceForward = 5.0f;
    public float DistanceBackward = 5.0f;
    public float DistanceLeft = 5.0f;
    public float DistanceRight = 5.0f;
    public float DistanceFrontRight = 5.0f;
    public float DistanceFrontLeft = 5.0f;

    public RaycastHit RaycastHitForward;
    public RaycastHit RaycastHitBackward;
    public RaycastHit RaycastHitLeft;
    public RaycastHit RaycastHitRight;
    public RaycastHit RaycastHitFrontRight;
    public RaycastHit RaycastHitFrontLeft;

    public Vector3 RaycastHitForwardEnd;
    public Vector3 RaycastHitBackwardEnd;
    public Vector3 RaycastHitRightEnd;
    public Vector3 RaycastHitLeftEnd;
    public Vector3 RaycastHitFrontRightEnd;
    public Vector3 RaycastHitFrontLeftEnd;

    public Vector3 AgentForward;
    public Vector3 AgentBackward;
    public Vector3 AgentLeft;
    public Vector3 AgentRight;
    public Vector3 AgentFrontRight;
    public Vector3 AgentFrontLeft;

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
        //AddVectorObs(DistanceForward / 5);
        //AddVectorObs(DistanceBackward / 5);
        //AddVectorObs(DistanceLeft / 5);
        //AddVectorObs(DistanceRight / 5);
        //AddVectorObs(DistanceFrontRight / 5);
        //AddVectorObs(DistanceFrontLeft / 5);

        //AddVectorObs(DistanceToTargetPoint / 5);
    }
    #endregion

    #region Action
    public float speed = 0.02f;
    public float DistanceToTargetPoint = float.MaxValue;
    public float PreviousDistancetoTargetPoint = float.MaxValue;

    public override void AgentAction(float[] vectorAction, string textAction)
    {

        // Rewards
        float distanceToTarget = Vector3.Distance(this.transform.position, Target.transform.position);

        //Reached target
        //if (distanceToTarget < 0.4f)
        //{
        //    Done();
        //    AddReward(15.0f);
        //}

        //Move towards the farest point
        if (TargetPoint.transform != null)
            DistanceToTargetPoint = Vector3.Distance(this.transform.position, TargetPoint.transform.position);

        if (DistanceToTargetPoint < PreviousDistancetoTargetPoint)
            AddReward(0.15f);

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

        // Crashed into wall -> checked via distance
        //if (DistanceForward < 0.3f)
        //{
        //    Done();
        //    AddReward(-2.0f);
        //}
        //if (DistanceBackward < 0.3f)
        //{
        //    Done();
        //    AddReward(-2.0f);
        //}
        //if (DistanceLeft < 0.15f)
        //{
        //    Done();
        //    AddReward(-2.0f);
        //}
        //if (DistanceRight < 0.15f)
        //{
        //    Done();
        //    AddReward(-2.0f);
        //}

        // Actions, size = 2
        float controlSignalMovement;
        Vector3 controlSignalRotation = Vector3.zero;

        controlSignalMovement = Mathf.Clamp(vectorAction[0], -1, 1);
        controlSignalRotation.y = Mathf.Clamp(vectorAction[1], -1, 1);

        this.transform.eulerAngles += controlSignalRotation * 3;
        this.transform.position += this.transform.forward * controlSignalMovement * speed;
    }

    //private void OnCollisionEnter(Collision collision)
    //{
    //    if (collision.gameObject.name == "finish")
    //    {
    //        Done();
    //        AddReward(15.0f);
    //    }
    //    else
    //    {
    //        Done();
    //        AddReward(-2.0f);
    //    }

    //}

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.name == "finish")
        {
            Done();
            AddReward(15.0f);
        }
        else
        {
            Done();
            AddReward(-2.0f);
        }

    }
    #endregion
}
