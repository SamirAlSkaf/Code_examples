using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;
using MLAgents;

public class MaulwurfOldSystem : Agent
{

    private Rigidbody agentRb;
    private RayPerception rayPer;
    public bool useVectorObs;
    public Transform Target;
    public Transform Spawn;
    private List<float> vectorObs;
    private RaycastHit currentFarestPoint;
    private RaycastHit lastFarestPoint;
    private List<RaycastHit> rewardHits;
    private List<RaycastHit> infoHits;
    private List<Vector3> rewardDirections;
    private List<Vector3> infoDirections;
    private List<float> rewardHitDistances;
    private List<float> infoHitDistances;

    private float distanceToLastTargetPoint;
    private float lastDistanceToLastTargetPoint;

    private float[] tags;

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

        Physics.Raycast(this.transform.position, this.transform.forward, out currentFarestPoint, rayDistance);
        Physics.Raycast(this.transform.position, this.transform.forward, out lastFarestPoint, rayDistance);

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
                Debug.DrawRay(this.transform.position, direction, Color.green);
            }

        foreach (Vector3 direction in infoDirections)
            if (Physics.Raycast(this.transform.position, direction, out hit, rayDistance))
                infoHits.Add(hit);

        rewardHitDistances = rewardHits.Select(x => x.distance).ToList();
        infoHitDistances = infoHits.Select(x => x.distance).ToList();

        tags = new float[rewardHitDistances.ToArray().Length];
        for (int i = 0; i < rewardHitDistances.ToArray().Length; i++)
            if (rewardHits.ToArray()[i].collider.gameObject.CompareTag("target"))
                tags[i] = 1f;
    }

    public void getDirections()
    {
        rewardDirections.Clear();
        infoDirections.Clear();

        rewardDirections.Add(this.transform.forward);
        infoDirections.Add(-this.transform.right);
        infoDirections.Add(this.transform.right);
        rewardDirections.Add(this.transform.right + this.transform.forward);
        rewardDirections.Add(-this.transform.right + this.transform.forward);
    }

    public override void CollectObservations()
    {
        if (useVectorObs)
        {

            //Debug.Log(rewardHitDistances.Length);
            AddVectorObs(rewardHitDistances.ToArray());
            AddVectorObs(infoHitDistances.ToArray());

            AddVectorObs(tags);
            //Debug.Log(tags.Length);

        }
    }

    public void MoveAgent(float[] act)
    {
        var dirToGo = Vector3.zero;
        var rotateDir = Vector3.zero;

        if (brain.brainParameters.vectorActionSpaceType == SpaceType.continuous)
        {
            dirToGo = transform.forward * Mathf.Clamp(act[0], -1f, 1f);
            rotateDir = transform.up * Mathf.Clamp(act[1], -1f, 1f);
        }
        else
        {
            var action = Mathf.FloorToInt(act[0]);
            switch (action)
            {
                case 1:
                    dirToGo = transform.forward * 1f;
                    break;
                case 2:
                    dirToGo = transform.forward * -1f;
                    break;
                case 3:
                    rotateDir = transform.up * 1f;
                    break;
                case 4:
                    rotateDir = transform.up * -1f;
                    break;
            }
        }
        transform.Rotate(rotateDir, Time.deltaTime * 100f);
        agentRb.AddForce(dirToGo * 0.3f, ForceMode.VelocityChange);
        Debug.Log(act[0]);
    }

    public override void AgentAction(float[] vectorAction, string textAction)
    {
        AddReward(-0.1f);

        getDirections();
        getObservations();

        for (int i = 0; i < tags.Length; i++)
            Debug.Log("Tag " + i + ": " + tags[i]);

        if (lastFarestPoint.transform != null)
            distanceToLastTargetPoint = Vector3.Distance(lastFarestPoint.transform.position, this.transform.position);

        if (currentFarestPoint.distance != 0)
        {
            lastFarestPoint = currentFarestPoint;
        }
        if (rewardHits != null)
        {
            var currentFarestPointDistance = rewardHits.Max(x => x.distance);
            currentFarestPoint = rewardHits.FirstOrDefault(x => x.distance == currentFarestPointDistance);
        }

        if (distanceToLastTargetPoint < lastDistanceToLastTargetPoint)
        {
            AddReward(0.2f);
            Debug.Log("Reward 0.2 for Distance!");
        }
        else
        {
            AddReward(-0.2f);
            Debug.Log("Negaitve Reward -0.2 for Distance!");
        }

        lastDistanceToLastTargetPoint = distanceToLastTargetPoint;

        MoveAgent(vectorAction);
    }

    public override void AgentReset()
    {
        agentRb.velocity = Vector3.zero;
        this.transform.rotation = Spawn.rotation;
        this.transform.position = Spawn.position;

        Physics.Raycast(this.transform.position, this.transform.forward, out currentFarestPoint, rayDistance);
        Physics.Raycast(this.transform.position, this.transform.forward, out lastFarestPoint, rayDistance);
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

    public override void AgentOnDone()
    {

    }
}
