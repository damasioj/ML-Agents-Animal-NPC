using System;
using System.Collections.Generic;
using System.Linq;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class AnimalAgent : Agent
{
    private bool hitBoundary;
    private bool isKilled;
    private Rigidbody rBody;
    private Vector3 previousPosition;
    private Animator animator;
    private float y; // sometimes the agent bugs and "flies", this is to reset it
    private bool isDoneCalled;
    private bool startedConsumption;
    private float currentEnergy;

    public event EventHandler EpisodeReset;
    public event EventHandler TaskDone;
    public float acceleration;
    public float minEnergy;
    public float maxEnergy;

    // target data
    private bool isAtTarget;
    private FoodTarget target;
    private readonly object targetLock = new object();

    // enemy
    [SerializeField] Enemy enemy;

    void Start()
    {
        isDoneCalled = false;
        rBody = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        hitBoundary = false;
        y = transform.position.y;
        startedConsumption = false;
    }

    void FixedUpdate()
    {
        if (!isDoneCalled)
        {
            // check if agent died or hit a boundary and reset episode
            if (isKilled)
            {
                isDoneCalled = true;
                animator.SetInteger("AnimIndex", 2);
                animator.SetTrigger("Next");
                EndEpisode();
            }
        }

        if (rBody.position.y > y + 5)
        {
            rBody.position = new Vector3(rBody.position.x, y, rBody.position.z);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        switch (other.tag)
        {
            case "wall":
                if (!hitBoundary && !isDoneCalled)
                {
                    isDoneCalled = true;
                    hitBoundary = true;
                    SubtractReward(0.1f);
                    Debug.Log($"Hit boundary.");
                    Debug.Log($"Current Reward: {GetCumulativeReward()}");
                    EndEpisode();
                }
                break;
            case "food":
                if (other.gameObject.Equals(target.gameObject))
                {
                    isAtTarget = true;
                }
                break;
            case "enemy":
                if (!isKilled)
                {
                    isKilled = true;
                    SubtractReward(0.2f);
                    Debug.Log($"Died.");
                    Debug.Log($"Current Reward: {GetCumulativeReward()}");
                }
                break;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("food"))
        {
            isAtTarget = false;
        }
    }

    private void OnTaskDone()
    {
        TaskDone?.Invoke(this, EventArgs.Empty);
    }

    public override void OnEpisodeBegin()
    {
        lock (targetLock)
        {
            OnEpisodeReset();

            // reset agent
            if (hitBoundary || currentEnergy <= 0 || isKilled)
            {
                rBody.angularVelocity = Vector3.zero;
                rBody.velocity = Vector3.zero;
                transform.position = new Vector3(0, y, 0);
                currentEnergy = UnityEngine.Random.Range(minEnergy, maxEnergy);
                enemy.Reset();
                isKilled = false;
            }

            hitBoundary = false;
            isAtTarget = false;
            isDoneCalled = false;
            startedConsumption = false;
        }
    }

    private void OnEpisodeReset()
    {
        EpisodeReset?.Invoke(this, EventArgs.Empty);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        lock (targetLock)
        {
            if (target is object)
            {
                // target
                sensor.AddObservation(target.transform.position.x); // 2
                sensor.AddObservation(target.transform.position.y);
                sensor.AddObservation(isAtTarget); // 1
                sensor.AddObservation(target.hp); // 1
            }
            else
            {
                //sensor.AddObservation(Vector3.zero);
                sensor.AddObservation(0f);
                sensor.AddObservation(0f);
                sensor.AddObservation(false);
                sensor.AddObservation(0f);
            }

            // agent
            sensor.AddObservation(transform.position.x); // 1
            sensor.AddObservation(transform.position.z); // 1
            sensor.AddObservation(rBody.velocity.x); // 1
            sensor.AddObservation(rBody.velocity.z); // 1
            sensor.AddObservation(currentEnergy); // 1

            // enemy
            sensor.AddObservation(enemy.transform.position.x); // 1
            sensor.AddObservation(enemy.transform.position.z); // 1
            sensor.AddObservation(enemy.Velocity); // 3
        }
    }

    public override void OnActionReceived(float[] vectorAction)
    {
        // Animal died
        if (currentEnergy <= 0 && !isDoneCalled)
        {
            isDoneCalled = true;
            SubtractReward(0.1f);
            Debug.Log($"Current Reward: {GetCumulativeReward()}");
            EndEpisode();
        }

        // Move
        Move(vectorAction);

        // Action (eat)
        if (isAtTarget)
        {
            TryConsume();
        }

        currentEnergy--;

        enemy.UpdateEnemy();
    }

    private void TryConsume()
    {
        // if agent is at target, consume it
        if (isAtTarget && target is IConsumable cons)
        {
            if (!cons.IsConsumed)
            {
                //Debug.Log("Consuming ...");
                if (!startedConsumption)
                {
                    AddReward(0.75f);
                    startedConsumption = true;
                    Debug.Log($"Current Reward: {GetCumulativeReward()}");
                }

                float targetEnergy = target.Consume(1);
                currentEnergy += targetEnergy;

                if (currentEnergy > maxEnergy)
                {
                    currentEnergy = maxEnergy;
                }

                if (cons.IsConsumed)
                {
                    AddReward(0.75f);
                    isAtTarget = false;
                    OnTaskDone();
                    Debug.Log($"Current Reward: {GetCumulativeReward()}");
                }
            }
        }
    }

    private void Move(float[] vectorAction)
    {
        Vector3 controlSignal = Vector3.zero;
        controlSignal.x = vectorAction[0];
        controlSignal.z = vectorAction[1];

        // agent is idle
        if (controlSignal.x == 0 && controlSignal.z == 0)
        {
            rBody.angularVelocity = Vector3.zero;
            rBody.velocity = Vector3.zero;
            if (animator.GetInteger("AnimIndex") != 0)
            {
                animator.SetInteger("AnimIndex", 0);
                animator.SetTrigger("Next");
            }
        }
        else // agent is moving
        {
            var rBody = GetComponent<Rigidbody>();
            var scale = gameObject.transform.localScale.x;

            if (rBody is object)
            {
                rBody.AddForce(new Vector3(controlSignal.x * acceleration * scale, 0, controlSignal.z * acceleration * scale));
            }

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
            previousPosition = transform.position;

            if (animator.GetInteger("AnimIndex") != 1)
            {
                animator.SetInteger("AnimIndex", 1);
                animator.SetTrigger("Next");
            }
        }
    }

    /// <summary>
    /// Allows the agent to reset their current target.
    /// The logic for the next target is purposefully left for the agent; the environment only provides data.
    /// </summary>
    /// <param name="targets"></param>
    public void UpdateTarget(IEnumerable<BaseTarget> targets)
    {
        target = targets.FirstOrDefault(t => t.IsValid && t is FoodTarget) as FoodTarget;

        if (target is null)
        {
            isDoneCalled = true;
            EndEpisode();
        }

        Debug.Log($"Current target: {target.name}");
        startedConsumption = false;
    }

    public override void Heuristic(float[] actions)
    {
        actions[0] = Input.GetAxis("Horizontal");
        actions[1] = Input.GetAxis("Vertical");
    }

    private void SubtractReward(float value)
    {
        AddReward(value * -1);
    }
}
