using MLAgents;
using System;
using UnityEngine;
using System.Linq;
using Barracuda;

public class AnimalAgent : Agent
{
    private BambooAcademy m_Academy;
    private Vector3 maxVelocity;
    private bool reachedBoundary;
    private Rigidbody rBody;
    private IConsumable[] TargetScripts;
    private float initialEnergy;
    private bool hitFoodSource;
    private GameObject currentTarget;
    
    public float speed = 10f;
    public FoodSource[] Targets;
    public float energy;

    void Start()
    {
        m_Academy = FindObjectOfType<BambooAcademy>();
        rBody = GetComponent<Rigidbody>();
        maxVelocity = new Vector3(speed, 0f, speed);
        reachedBoundary = false;
        hitFoodSource = false;
        initialEnergy = energy;

        // get scripts from targets
        TargetScripts = new IConsumable[Targets.Length];
        for (int i = 0; i < Targets.Length; i++)
        {
            TargetScripts[i] = Targets[i].GetComponent<IConsumable>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "wall")
        {
            reachedBoundary = true;
            SubtractReward(0.1f);
            Done();
        }
        else if (other.gameObject.tag == "food" || other.gameObject.tag == "badFood")
        {
            hitFoodSource = true;
            currentTarget = other.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "food" || other.gameObject.tag == "badFood")
        {
            hitFoodSource = false;
            currentTarget = null;
        }
    }

    public override void AgentReset()
    {
        // reset agent position
        if (this.transform.localPosition.y < 0 || reachedBoundary == true || IsMaxStepReached() || energy <= 0)
        {
            // If the Agent fell, zero its momentum
            this.rBody.angularVelocity = Vector3.zero;
            this.rBody.velocity = Vector3.zero;
            this.transform.localPosition = new Vector3(0, transform.localPosition.y, 0);
        }

        // reset target
        foreach(var target in TargetScripts)
        {
            target.Reset();
        }

        energy = initialEnergy;
        reachedBoundary = false;
        hitFoodSource = false;
        currentTarget = null;
    }

    private void ResetTarget()
    {
        var foodSourceAmount = m_Academy.
        for (int i = 0; i < m_Academy.)
    }

    public override void CollectObservations()
    {
        // Target and Agent positions
        foreach(var target in Targets)
        {
            float[] targetArr = new float[4] 
            {
                target.transform.localPosition.x,
                target.transform.localPosition.y,
                target.transform.localPosition.z,
                Convert.ToSingle(target.IsConsumed)
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
                Debug.Log($"Reward: {GetReward()}");
                Done();
            }

            // Move
            Move(vectorAction);

            // Action
            Action(vectorAction);

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
        if (hitFoodSource)
        {
            bool isConsumed = currentTarget.gameObject.GetComponent<IConsumable>().Consume(0.1f);

            if (currentTarget.tag == "food")
            {
                energy += 3;
                AddReward(0.3f);

                if (energy > initialEnergy)
                {
                    energy = initialEnergy;
                }
            }
            else
            {
                SubtractReward(0.2f);
            }

            if (isConsumed)
            {
                if (Targets.Where(x => x.tag == "food").All(x => x.IsConsumed == true))
                {
                    Debug.Log($"Reward: {GetReward()}");
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
        action[2] = Input.GetKey(KeyCode.E) ? 1.0f : 0.0f;
        return action;
    }

    private void SubtractReward(float value)
    {
        //if (GetReward() > 0.1f)
        //{
        //    AddReward(value * -1);
        //}

        //if (GetReward() < 0.1f)
        //{
        //    SetReward(-0.1f);
        //}

        AddReward(value * -1);
    }
}
