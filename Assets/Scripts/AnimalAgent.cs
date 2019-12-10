using MLAgents;
using System;
using UnityEngine;

public class AnimalAgent : Agent
{    
    private Vector3 maxVelocity;
    private bool reachedBoundary;
    private Rigidbody rBody;
    private IConsumable TargetScript;
    private float initialEnergy;
    
    public float speed = 10;
    public Transform Target;
    public float energy;

    void Start()
    {
        rBody = GetComponent<Rigidbody>();
        maxVelocity = new Vector3(10f, 0f, 10f);
        reachedBoundary = false;
        TargetScript = Target.gameObject.GetComponent<IConsumable>();
        initialEnergy = energy;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.name == "Boundary")
        {
            reachedBoundary = true;
            Done();
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
            energy = initialEnergy;
        }

        // reset target
        TargetScript.Reset();

        reachedBoundary = false;
    }

    public override void CollectObservations()
    {
        // Target and Agent positions
        AddVectorObs(Target.localPosition);
        AddVectorObs(this.transform.localPosition);

        // Agent velocity
        AddVectorObs(rBody.velocity.x);
        AddVectorObs(rBody.velocity.z);

        // Energy
        AddVectorObs(energy);
    }
    
    public override void AgentAction(float[] vectorAction)
    {
        // Add reward based on energy
        AddReward(0.1f);

        // Animal died
        if (energy <= 0)
        {
            SetReward(-1f);
            Done();
        }

        // Move
        Move(vectorAction);

        // Action
        Action(vectorAction);        

        // Fell off platform
        if (this.transform.localPosition.y < 0)
        {
            AddReward(-0.1f);
            Done();
        }

        energy--;
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
        float action = vectorAction[2];

        if (action == 1)
        {
            ConsumeFood();
        }
    }

    private void ConsumeFood()
    {
        float distanceToTarget = Vector3.Distance(this.transform.localPosition,
                                                  Target.localPosition);

        // Reached target
        if (distanceToTarget < 4f)
        {
            bool isConsumed = TargetScript.Consume(Time.deltaTime);
            energy += 2;

            if (energy > initialEnergy)
            {
                energy = initialEnergy;
            }

            if (isConsumed)
                Done();
        }
    }

    public override float[] Heuristic()
    {
        var action = new float[3];
        action[0] = Input.GetAxis("Horizontal");
        action[1] = Input.GetAxis("Vertical");
        action[2] = Convert.ToSingle(Input.GetKey(KeyCode.E));
        return action;
    }
}
