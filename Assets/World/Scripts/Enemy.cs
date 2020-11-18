using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private AnimalAgent agent;
    [SerializeField] private float range;
    [SerializeField] private float maxSpeed;
    [SerializeField] private float speedMultiplier;
    private Rigidbody rBody;

    public Vector3 Velocity => rBody.velocity;
    
    // Start is called before the first frame update
    void Start()
    {
        rBody = GetComponent<Rigidbody>();
        Reset();
    }

    // Update is called once per frame
    public void UpdateEnemy()
    {
        UpdateLocation();
    }

    public void Reset()
    {
        transform.localPosition = new Vector3(Random.Range(-1f, 1f) * range,
                                            transform.localPosition.y,
                                            Random.Range(-1f, 1f) * range);
        rBody.velocity = Vector3.zero;
        rBody.angularVelocity = Vector3.zero;
    }

    void UpdateLocation()
    {
        float x, z;

        if (agent.transform.localPosition.x > transform.localPosition.x)
        {
            x = speedMultiplier;
        }
        else
        {
            x = -speedMultiplier;
        }

        if (agent.transform.localPosition.z > transform.localPosition.z)
        {
            z = speedMultiplier;
        }
        else
        {
            z = -speedMultiplier;
        }

        Move(x, z);
    }

    void Move(float x, float z)
    {
        x = (rBody.velocity.x > maxSpeed || rBody.velocity.x < maxSpeed * - 1) ? 0 : x;
        z = (rBody.velocity.z > maxSpeed || rBody.velocity.z < maxSpeed * - 1) ? 0 : z;

        if (x != 0 && z != 0)
        {
            rBody.velocity += new Vector3(x, 0, z);
        }
    }
}
