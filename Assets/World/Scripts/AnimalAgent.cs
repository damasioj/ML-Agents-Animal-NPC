using System;
using System.Collections.Generic;
using System.Linq;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class AnimalAgent : Agent
{
    private Vector3 maxVelocity;
    private bool hitBoundary;
    private bool isKilled;
    private Rigidbody rBody;
    private float initialEnergy;
    private bool raycastHit;
    private Vector3 previousPosition;
    private Animator animator;
    private int layerMask;
    private BambooAcademy academy;

    public event EventHandler EpisodeReset;
    public float speed;
    public float acceleration;
    public float energy;

    // target data
    private BaseTarget activeTarget;
    public List<BaseTarget> targets;

    // enemy
    [SerializeField] Enemy enemy;

    void Start()
    {
        rBody = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        maxVelocity = new Vector3(speed, 0f, speed);
        hitBoundary = false;
        initialEnergy = energy;
        layerMask = 0 << 8;
        layerMask = ~layerMask;
    }

    void FixedUpdate()
    {
        // check if agent died or hit a boundary and reset episode
        if (hitBoundary || isKilled)
        {
            animator.SetInteger("AnimIndex", 2);
            animator.SetTrigger("Next");
            EndEpisode();
        }

        // if agent is at target, consume it
        if (activeTarget is IConsumable cons)
        {
            if (!cons.IsConsumed)
            {
                bool isConsumed = cons.Consume(1f);

                energy += 10;
                AddReward(0.01f);
                Debug.Log($"Current Reward: {GetCumulativeReward()}");

                if (energy > initialEnergy)
                {
                    energy = initialEnergy;
                }

                if (isConsumed && targets.Cast<FoodTarget>().All(t => t.IsConsumed))
                {
                    EndEpisode();
                }
            }
        }

        VerifyRaycast();
    }

    private void OnTriggerEnter(Collider other)
    {
        switch (other.tag)
        {
            case "wall":
                if (!hitBoundary)
                {
                    hitBoundary = true;
                    SubtractReward(0.1f);
                    Debug.Log($"Current Reward: {GetCumulativeReward()}");
                }
                break;
            case "food":
                activeTarget = other.gameObject.GetComponent<BaseTarget>();
                break;
            case "enemy":
                if (!isKilled)
                {
                    isKilled = true;
                    SubtractReward(0.2f);
                    Debug.Log($"Current Reward: {GetCumulativeReward()}");
                }
                break;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("food"))
        {
            activeTarget = null;
        }
    }

    public override void OnEpisodeBegin()
    {
        // reset agent
        if (hitBoundary || energy <= 0 || isKilled)
        {
            rBody.angularVelocity = Vector3.zero;
            rBody.velocity = Vector3.zero;
            transform.localPosition = new Vector3(0, transform.localPosition.y, 0);
            energy = initialEnergy;
            enemy.Reset();
            isKilled = false;
        }
        
        hitBoundary = false;
        activeTarget = null;
        OnEpisodeReset();
    }

    private void OnEpisodeReset()
    {
        EpisodeReset?.Invoke(this, EventArgs.Empty);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Target
        targets.ForEach(t => sensor.AddObservation(t.transform.localPosition)); // 3 * n        
        targets.Cast<FoodTarget>().ToList().ForEach(t => sensor.AddObservation(t.IsConsumed)); // 3

        // agent
        sensor.AddObservation(transform.localPosition); // 3
        sensor.AddObservation(rBody.velocity.x); // 1
        sensor.AddObservation(rBody.velocity.z); // 1
        sensor.AddObservation(raycastHit); // 1

        // Energy
        sensor.AddObservation(energy); // 1

        // enemy
        sensor.AddObservation(enemy.Location); // 3
        sensor.AddObservation(enemy.Velocity); // 3
     }

    public override void OnActionReceived(float[] vectorAction)
    {
        // Animal died
        if (energy <= 0)
        {
            SubtractReward(0.1f);
            Debug.Log($"Current Reward: {GetCumulativeReward()}");
            EndEpisode();
        }

        // Move
        Move(vectorAction);

        energy--;
    }

    private void Move(float[] vectorAction)
    {
        // Move Actions, size = 2
        Vector3 controlSignal = Vector3.zero;
        controlSignal.x = vectorAction[0];
        controlSignal.z = vectorAction[1];

        // agent is idle
        if (controlSignal.x == 0 && controlSignal.z == 0)
        {
            rBody.angularVelocity = Vector3.zero;
            rBody.velocity = Vector3.zero;
            animator.SetInteger("AnimIndex", 0);
            animator.SetTrigger("Next");
        }
        else // agent is moving
        {            
            if (rBody.velocity.magnitude > speed)
            {
                controlSignal.x = 0;
                controlSignal.z = 0;
            }

            rBody.velocity += new Vector3(controlSignal.x * acceleration, 0, controlSignal.z * acceleration);
            SetDirection();
        }
    }

    private void SetDirection()
    {
        if (transform.position != previousPosition)
        {
            var direction = (transform.position - previousPosition).normalized;
            direction.y = 0;

            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), 0.15F);
            //transform.rotation = Quaternion.LookRotation(direction);
            previousPosition = transform.position;

            if (animator.GetInteger("AnimIndex") != 1)
            {
                animator.SetInteger("AnimIndex", 1);
                animator.SetTrigger("Next");
            }
        }
    }

    private void VerifyRaycast()
    {
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out RaycastHit hit, 50f, layerMask))
        {
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.red);

            if (hit.collider.CompareTag("obstacle"))
            {
                raycastHit = true;
            }
        }
        else
        {
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * 50f, Color.white);

            raycastHit = false;
        }
    }

    public override void Heuristic(float[] actions)
    {
        actions[0] = Input.GetAxis("Horizontal");
        actions[1] = Input.GetAxis("Vertical");
        //action[2] = Input.GetKey(KeyCode.E) ? 1.0f : 0.0f;
    }

    private void SubtractReward(float value)
    {
        AddReward(value * -1);
    }
}
