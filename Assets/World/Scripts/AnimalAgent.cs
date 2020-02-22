﻿using MLAgents;
using System;
using UnityEngine;
using System.Linq;
using Barracuda;

public class AnimalAgent : Agent
{
    private Vector3 maxVelocity;
    private bool reachedBoundary;
    private Rigidbody rBody;
    private float initialEnergy;
    private BehaviorParameters policy;
    
    public float speed;
    public float energy;

    // target data
    private int targetLimiter = 1; // used later for curriculum training
    private GameObject currentTarget;
    private IConsumable[] TargetScripts;
    private bool hitTarget;
    public FoodSource[] Targets;
    

    void Start()
    {
        rBody = GetComponent<Rigidbody>();
        maxVelocity = new Vector3(speed, 0f, speed);
        reachedBoundary = false;
        hitTarget = false;
        initialEnergy = energy;
        policy = GetComponent<BehaviorParameters>();

        // get scripts from targets and initialize
        TargetScripts = new IConsumable[Targets.Length];
        for (int i = 0; i < Targets.Length; i++)
        {
            TargetScripts[i] = Targets[i].GetComponent<IConsumable>();

            Targets[i].Consume(100);
            Targets[i].enabled = false;
        }

        ResetTarget();
    }

    // used to update the vector observation size dynamically
    private void UpdatePolicy()
    {
        int baseSize = 4; // refactor
        int additionalVectors = targetLimiter * 4;

        policy.brainParameters.vectorObservationSize = baseSize + additionalVectors;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsDone())
        {
            if (other.tag == "wall")
            {
                reachedBoundary = true;
                SubtractReward(0.6f);
                Done();
            }
            else if (other.gameObject.tag == "food" || other.gameObject.tag == "badFood")
            {
                hitTarget = true;
                currentTarget = other.gameObject;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "food" || other.gameObject.tag == "badFood")
        {
            hitTarget = false;
            currentTarget = null;
        }
    }

    public override void AgentReset()
    {
        Debug.Log($"Reward: {GetCumulativeReward()}");

        // reset agent position
        if (this.transform.localPosition.y < 0 || reachedBoundary == true || IsMaxStepReached() || energy <= 0)
        {
            // If the Agent fell, zero its momentum
            this.rBody.angularVelocity = Vector3.zero;
            this.rBody.velocity = Vector3.zero;
            this.transform.localPosition = new Vector3(0, transform.localPosition.y, 0);
        }

        ResetTarget();

        energy = initialEnergy;
        reachedBoundary = false;
        hitTarget = false;
        currentTarget = null;
    }

    private void ResetTarget()
    {
        //targetLimiter = (int)m_Academy.resetParameters["number_food_sources"];

        if (targetLimiter < 4)
        {
            if (GetCumulativeReward() >= 1.75f && targetLimiter == 3)
            {
                targetLimiter = 4;
            }
            else if (GetCumulativeReward() >= 1.25f)
            {
                targetLimiter = 3;
            }
            else if (GetCumulativeReward() >= 0.5f)
            {
                targetLimiter = 2;
            }
        }

        UpdatePolicy();

        for (int i = 0; i < targetLimiter; i++)
        {
            Targets[i].enabled = true;
            TargetScripts[i].Reset();
        }
    }

    public override void CollectObservations()
    {
        // Target and Agent positions
        for(int i = 0; i < targetLimiter; i++)
        {
            float[] targetArr = new float[4] 
            {
                Targets[i].transform.localPosition.x,
                Targets[i].transform.localPosition.z,
                Convert.ToSingle(Targets[i].IsConsumed),
                Convert.ToSingle(Targets[i].IsGoodConsumable)
            };

            AddVectorObs(targetArr);
        }        

        AddVectorObs(this.transform.localPosition);

        // Agent velocity
        AddVectorObs(rBody.velocity.x);
        AddVectorObs(rBody.velocity.z);

        // Energy
        AddVectorObs(energy);
    }
    
    public override void AgentAction(float[] vectorAction)
    {
        if (!IsDone())
        {
            // Animal died
            if (energy <= 0)
            {
                SubtractReward(0.5f);
                Done();
            }

            // Move
            Move(vectorAction);

            // Action
            //Action(vectorAction);

            energy--;
        }
    }

    private void Move(float[] vectorAction)
    {
        // Move Actions, size = 2
        Vector3 controlSignal = Vector3.zero;
        controlSignal.x = vectorAction[0];
        controlSignal.z = vectorAction[1];

        if (rBody.velocity.magnitude < maxVelocity.magnitude)
        {
            rBody.velocity += controlSignal * speed;
        }

        if (hitTarget)
        {
            bool isConsumed = currentTarget.gameObject.GetComponent<IConsumable>().Consume(0.1f);

            if (currentTarget.tag == "food")
            {
                energy += 3;
                AddReward(0.01f);

                if (energy > initialEnergy)
                {
                    energy = initialEnergy;
                }
            }
            else
            {
                SubtractReward(0.01f);
            }

            if (isConsumed)
            {
                if (Targets.Where(x => x.tag == "food").All(x => x.IsConsumed == true))
                {
                    Done();
                }
            }
        }
    }
    
    private void Action(float[] vectorAction)
    {
        // Action, size = 1
        int action = (int)vectorAction[2];

        if (action == 1)
        {
            ConsumeFood();
        }
    }

    private void ConsumeFood()
    {
        if (hitTarget)
        {
            bool isConsumed = currentTarget.gameObject.GetComponent<IConsumable>().Consume(0.1f);

            if (currentTarget.tag == "food")
            {
                energy += 3;
                AddReward(0.01f);

                if (energy > initialEnergy)
                {
                    energy = initialEnergy;
                }
            }
            else
            {
                SubtractReward(0.01f);
            }

            if (isConsumed)
            {
                if (Targets.Where(x => x.tag == "food").All(x => x.IsConsumed == true))
                {
                    Done();
                }
            }
        }
    }

    public override float[] Heuristic()
    {
        var action = new float[3];
        action[0] = Input.GetAxis("Horizontal");
        action[1] = Input.GetAxis("Vertical");
        //action[2] = Input.GetKey(KeyCode.E) ? 1.0f : 0.0f;
        return action;
    }

    private void SubtractReward(float value)
    {
        AddReward(value * -1);
    }
}
