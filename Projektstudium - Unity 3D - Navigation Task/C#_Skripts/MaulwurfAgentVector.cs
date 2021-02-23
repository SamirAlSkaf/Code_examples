using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;
using MLAgents;

public class MaulwurfAgentVector : Agent
{

    private Rigidbody agentRb;
    private RayPerception rayPer;
    public bool useVectorObs;
    public Transform Target;
    public Transform Spawn;

    public override void InitializeAgent()
    {
        base.InitializeAgent();
        agentRb = GetComponent<Rigidbody>();
        rayPer = GetComponent<RayPerception>();
    }

    public override void CollectObservations()
    {
        if (useVectorObs)
        {
            const float rayDistance = 5f;
            float[] rayAngles = { 20f, 90f, 160f, 45f, 135f, 70f, 110f };

            string[] detectableObjects = { "wall", "target" };

            AddVectorObs(rayPer.Perceive(rayDistance, rayAngles, detectableObjects, 0f, 0f));
            AddVectorObs(transform.InverseTransformDirection(agentRb.velocity));
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
        AddReward(-50f / agentParameters.maxStep);
        //AddReward(Vector3.Scale(agentRb.velocity, agentRb.transform.forward).x + Vector3.Scale(agentRb.velocity, agentRb.transform.forward).y + Vector3.Scale(agentRb.velocity, agentRb.transform.forward).z);
        //Debug.Log(Vector3.Scale(agentRb.velocity, agentRb.transform.forward).x + Vector3.Scale(agentRb.velocity, agentRb.transform.forward).y + Vector3.Scale(agentRb.velocity, agentRb.transform.forward).z);
        //Debug.Log(agentRb.velocity.sqrMagnitude / 2);
        //AddReward(agentRb.velocity.sqrMagnitude / 15);
        MoveAgent(vectorAction);
    }

    public override void AgentReset()
    {
        agentRb.velocity = Vector3.zero;
        this.transform.rotation = Spawn.rotation;
        this.transform.position = Spawn.position;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("target"))
        {
            SetReward(30f);
            Done();
        }
        else if (collision.gameObject.CompareTag("wall"))
        {
            SetReward(-0.5f);
        }
    }

    public override void AgentOnDone()
    {

    }
}
