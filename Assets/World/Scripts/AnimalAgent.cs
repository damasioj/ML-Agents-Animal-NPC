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
    
    public float speed;
    public float energy;

    // target data
    private bool hitTarget;
    public BaseTarget Target;

    // enemy
    [SerializeField] Enemy enemy;

    void Start()
    {
        rBody = GetComponent<Rigidbody>();
        maxVelocity = new Vector3(speed, 0f, speed);
        hitBoundary = false;
        hitTarget = false;
        initialEnergy = energy;
    }

    void FixedUpdate()
    {
        if (hitBoundary || isKilled)
        {
            EndEpisode();
        }
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
                hitTarget = true;
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
            hitTarget = false;
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

        Target.Reset();        
        hitBoundary = false;
        hitTarget = false;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Target
        sensor.AddObservation(Target.transform.localPosition); // 3
        
        if (Target is IConsumable cons)
            sensor.AddObservation(cons.IsConsumed);

        // agent
        sensor.AddObservation(transform.localPosition); // 3
        sensor.AddObservation(rBody.velocity.x); // 1
        sensor.AddObservation(rBody.velocity.z); // 1

        // Energy
        sensor.AddObservation(energy);

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

        if (rBody.velocity.magnitude < maxVelocity.magnitude)
        {
            rBody.velocity += controlSignal * speed;
        }

        if (hitTarget)
        {
            if (Target is IConsumable cons)
            {
                bool isConsumed = cons.Consume(0.1f);

                energy += 3;
                AddReward(0.01f);

                if (energy > initialEnergy)
                {
                    energy = initialEnergy;
                }

                if (isConsumed)
                {
                    Debug.Log($"Current Reward: {GetCumulativeReward()}");
                    EndEpisode();
                }
            }
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
