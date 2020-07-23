﻿using System;
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
    private float initialEnergy;
    private bool raycastHit;
    private Vector3 previousPosition;
    private Animator animator;
    private int layerMask;
    private float y;
    private bool isDoneCalled;

    public event EventHandler EpisodeReset;
    public event EventHandler TaskDone;
    public float speed;
    public float acceleration;
    public float energy;

    // target data
    private bool isAtTarget;
    private FoodTarget target;

    // enemy
    [SerializeField] Enemy enemy;

    void Start()
    {
        raycastHit = false;
        isDoneCalled = false;
        rBody = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        hitBoundary = false;
        initialEnergy = energy;
        layerMask = 0 << 8;
        layerMask = ~layerMask;
        y = transform.position.y;
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

            // if agent is at target, consume it
            if (isAtTarget && target is IConsumable cons)
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

                    if (isConsumed)
                    {
                        isAtTarget = false;
                        OnTaskDone();
                    }
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
                if (!hitBoundary && !isDoneCalled)
                {
                    isDoneCalled = true;
                    hitBoundary = true;
                    SubtractReward(0.1f);
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
        OnEpisodeReset();

        // reset agent
        if (hitBoundary || energy <= 0 || isKilled)
        {
            rBody.angularVelocity = Vector3.zero;
            rBody.velocity = Vector3.zero;
            transform.position = new Vector3(0, y, 0);
            energy = initialEnergy;
            enemy.Reset();
            isKilled = false;
        }

        hitBoundary = false;
        isAtTarget = false;
        isDoneCalled = false;
        raycastHit = false;
    }

    private void OnEpisodeReset()
    {
        EpisodeReset?.Invoke(this, EventArgs.Empty);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        if (!isDoneCalled)
        {
            // target
            sensor.AddObservation(target.transform.position); // 3
            sensor.AddObservation(isAtTarget); // 1
            sensor.AddObservation(target.hp); // 1

            // agent
            sensor.AddObservation(transform.position.x); // 1
            sensor.AddObservation(transform.position.z); // 1
            sensor.AddObservation(rBody.velocity.x); // 1
            sensor.AddObservation(rBody.velocity.z); // 1
            sensor.AddObservation(raycastHit); // 1
            sensor.AddObservation(energy); // 1

            // enemy
            sensor.AddObservation(enemy.transform.position.x); // 1
            sensor.AddObservation(enemy.transform.position.z); // 1
            sensor.AddObservation(enemy.Velocity); // 3
        }
    }

    public override void OnActionReceived(float[] vectorAction)
    {
        // Animal died
        if (energy <= 0 && !isDoneCalled)
        {
            isDoneCalled = true;
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
        Vector3 controlSignal = new Vector3(vectorAction[0] * acceleration, 0, vectorAction[1] * acceleration);

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
            if (rBody.velocity.magnitude > speed)
            {
                controlSignal.x = 0;
                controlSignal.z = 0;
            }

            rBody.velocity += controlSignal;
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
