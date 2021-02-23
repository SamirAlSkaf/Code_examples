using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;
using MLAgents;

public class MaulwurfThreeDimensional : Agent
{
    private Rigidbody agentRb;
    private RayPerception rayPer;

    public bool useVectorObs;
    public Transform Target;
    public Transform Spawn;

    public float turnSpeed;

    private List<float> vectorObs;

    private RaycastHit targetPoint;
    private float previousDistanceToTargetPoint;

    private List<RaycastHit> rewardHits;
    private List<RaycastHit> infoHits;

    private List<Vector3> rewardDirections;
    private List<Vector3> infoDirections;

    private List<float> rewardHitDistances;
    private List<float> infoHitDistances;

    private float distanceToLastTargetPoint;
    private float lastDistanceToLastTargetPoint;

    //private float[] tags;

    public const float rayDistance = 10f;


    public override void InitializeAgent()
    {

        base.InitializeAgent();
        agentRb = GetComponent<Rigidbody>();
        rayPer = GetComponent<RayPerception>();

        rewardDirections = new List<Vector3>();
        infoDirections = new List<Vector3>();

        rewardHitDistances = new List<float>();
        infoHitDistances = new List<float>();

        getDirections();

        //Physics.Raycast(this.transform.position, this.transform.forward, out currentFarestPoint, rayDistance);
        //Physics.Raycast(this.transform.position, this.transform.forward, out lastFarestPoint, rayDistance);

        getObservations();

    }

    public void getObservations()
    {
        RaycastHit hit;
        rewardHits = new List<RaycastHit>();
        infoHits = new List<RaycastHit>();

        foreach (Vector3 direction in rewardDirections)
            if (Physics.Raycast(this.transform.position, direction, out hit, rayDistance))
            {
                rewardHits.Add(hit);
                //Debug.DrawRay(this.transform.position, direction, Color.green);
            }

        foreach (Vector3 direction in infoDirections)
            if (Physics.Raycast(this.transform.position, direction, out hit, rayDistance))
            {
                infoHits.Add(hit);
                //Debug.DrawRay(this.transform.position, direction, Color.red);

            }

        if (rewardHits != null)
            rewardHitDistances = rewardHits.Select(x => x.distance).ToList();


        //var rewardHitDistancesArray = rewardHits.Select(x => x.distance).ToArray();
        //for (int i = 0; i < rewardHitDistancesArray.Length; i++)
        //{
        //    rewardHitDistancesAndTags = new float[rewardHitDistancesArray.Length * 2];
        //    rewardHitDistancesAndTags[i * 2] = rewardHitDistancesArray[i];
        //    if (rewardHits.ToArray()[i].collider.gameObject.CompareTag("target"))
        //        rewardHitDistancesAndTags[(i * 2) + 1] = 1f;
        //}

        if (infoHits != null)
            infoHitDistances = infoHits.Select(x => x.distance).ToList();

        //tags = new float[rewardHitDistances.ToArray().Length];
        //for (int i = 0; i < rewardHitDistances.ToArray().Length; i++)
        //    if (rewardHits.ToArray()[i].collider.gameObject.CompareTag("target"))
        //        tags[i] = 1f;
    }

    public void getDirections()
    {
        rewardDirections.Clear();
        infoDirections.Clear();

        //if (tags != null)
        //    for (int i = 0; i < tags.Length; i++)
        //        tags[i] = 0;

        //if (rewardHitDistancesAndTags != null)
        //    for (int i = 0; i < rewardHitDistancesAndTags.Length; i++)
        //        rewardHitDistancesAndTags[i] = 0;

        infoDirections.Add(-this.transform.right);
        infoDirections.Add(this.transform.right);
        infoDirections.Add(this.transform.up);
        infoDirections.Add(-this.transform.up);

        rewardDirections.Add(this.transform.forward);
        rewardDirections.Add(this.transform.right + this.transform.forward);
        rewardDirections.Add(-this.transform.right + this.transform.forward);
        rewardDirections.Add(this.transform.up + this.transform.forward);
        rewardDirections.Add(-this.transform.up + this.transform.forward);
    }

    public override void CollectObservations()
    {
        if (useVectorObs)
        {

            //Debug.Log(rewardHitDistances.Length);
            AddVectorObs(rewardHitDistances.ToArray());
            AddVectorObs(infoHitDistances.ToArray());

            //AddVectorObs(rewardHitDistancesAndTags);

            //AddVectorObs(tags);
            //Debug.Log(tags.Length);

        }
    }

    //public void MoveAgent(float[] vectorAction)
    //{

    //    // Define Agent Actions
    //    Vector3 controlSignalMovement = Vector3.zero;
    //    Vector3 controlSignalRotation = Vector3.zero;

    //    #region Action Move
    //    int actionMove = Mathf.FloorToInt(vectorAction[0]);
    //    const int Forwards = 1;
    //    const int Backwards = 2;

    //    if (actionMove == Forwards) controlSignalMovement = this.transform.forward * 1;
    //    if (actionMove == Backwards) controlSignalMovement = this.transform.forward * -1;
    //    #endregion

    //    #region Action Pan
    //    int actionPan = Mathf.FloorToInt(vectorAction[1]);
    //    const int Left = 1;
    //    const int Right = 2;

    //    if (actionPan == Left) controlSignalRotation.y = -1.0f;
    //    if (actionPan == Right) controlSignalRotation.y = 1.0f;
    //    #endregion

    //    #region Action Tilt
    //    int actionTilt = Mathf.FloorToInt(vectorAction[2]);
    //    const int Up = 1;
    //    const int Down = 2;

    //    if (actionTilt == Up) controlSignalRotation.x = -1.0f;
    //    if (actionTilt == Down) controlSignalRotation.x = 1.0f;
    //    #endregion

    //    transform.Rotate(controlSignalRotation, Time.deltaTime * turnSpeed);
    //    agentRb.AddForce(controlSignalMovement * 0.3f, ForceMode.VelocityChange);

    //}

    private void handleAgentActions(float[] vectorAction)
    {
        Vector3 controlSignalMovement = Vector3.zero;
        Vector3 controlSignalRotation = Vector3.zero;

        //GameObject brain = ;

        if (GameObject.FindGameObjectsWithTag("Brain")[0].GetComponent<Brain>().brainParameters.vectorActionSpaceType == SpaceType.continuous)
        {
            #region Action Tilt
            controlSignalRotation.x = Mathf.Clamp(vectorAction[0], -1, 1);
            #endregion

            #region Action Pan
            controlSignalRotation.y = Mathf.Clamp(vectorAction[1], -1, 1);
            #endregion

            #region Action Move
            controlSignalMovement = this.transform.forward * Mathf.Clamp(vectorAction[2], -1, 1);
            #endregion
        }
        else // discrete
        {
            #region Action Tilt
            int actionTilt = Mathf.FloorToInt(vectorAction[0]);
            const int Up = 1;
            const int Down = 2;

            if (actionTilt == Up) controlSignalRotation.x = -1.0f;
            if (actionTilt == Down) controlSignalRotation.x = 1.0f;
            #endregion

            #region Action Pan
            int actionPan = Mathf.FloorToInt(vectorAction[1]);
            const int Left = 1;
            const int Right = 2;

            if (actionPan == Left) controlSignalRotation.y = -1.0f;
            if (actionPan == Right) controlSignalRotation.y = 1.0f;
            #endregion

            #region Action Move
            int actionMove = Mathf.FloorToInt(vectorAction[2]);
            const int Forwards = 1;
            const int Backwards = 2;

            if (actionMove == Forwards) controlSignalMovement = this.transform.forward * 1;
            if (actionMove == Backwards) controlSignalMovement = this.transform.forward * -1;
            #endregion
        }

        //this.transform.eulerAngles += controlSignalRotation * AgentRotationSpeed;
        //this.transform.position += controlSignalMovement * AgentMovementSpeed;

        transform.Rotate(controlSignalRotation, Time.deltaTime * turnSpeed);
        agentRb.AddForce(controlSignalMovement * 0.3f, ForceMode.VelocityChange);
    }

    public override void AgentAction(float[] vectorAction, string textAction)
    {
        AddReward(-0.2f);

        getDirections();
        getObservations();

        //for (int i = 0; i < tags.Length; i++)
        //    Debug.Log("Tag " + i + ": " + tags[i]);

        //for (int i = 1; i < rewardHitDistancesAndTags.Length; i += 2)
        //    Debug.Log("Tag " + i + ": " + rewardHitDistancesAndTags[i]);

        if (targetPoint.transform != null)
            if (previousDistanceToTargetPoint > Vector3.Distance(this.transform.position, targetPoint.transform.position))
            {
                AddReward(0.4f);
                Debug.Log("Reward 0.4 for Distance!");
            }
            else
            {
                AddReward(-0.2f);
                Debug.Log("Negaitve Reward -0.2 for Distance!");
            }

        if (rewardHits != null)
        {
            var currentFarestPointDistance = rewardHits.Max(x => x.distance);
            targetPoint = rewardHits.FirstOrDefault(x => x.distance == currentFarestPointDistance);
        }

        if (targetPoint.transform != null)
            previousDistanceToTargetPoint = Vector3.Distance(this.transform.position, targetPoint.transform.position);

        //MoveAgent(vectorAction);
        handleAgentActions(vectorAction);
    }

    public override void AgentReset()
    {
        agentRb.velocity = Vector3.zero;
        this.transform.rotation = Spawn.rotation;
        this.transform.position = Spawn.position;

        //Physics.Raycast(this.transform.position, this.transform.forward, out currentFarestPoint, rayDistance);
        //Physics.Raycast(this.transform.position, this.transform.forward, out lastFarestPoint, rayDistance);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("target"))
        {
            SetReward(30f);
            Debug.Log("Reward 30 for reaching target!");
            Done();
        }
        else if (collision.gameObject.CompareTag("wall"))
        {
            SetReward(-10f);
            Debug.Log("Reward -10 for hitting wall!");
            Done();
        }
    }

    private void OnTriggerEnter(Collider trigger)
    {
        if (trigger.CompareTag("target"))
        {
            SetReward(30f);
            Debug.Log("Reward 30 for reaching targetRoom!");
            Done();
        }
    }

    public override void AgentOnDone()
    {

    }
}
