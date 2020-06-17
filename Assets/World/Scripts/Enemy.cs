using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private AnimalAgent agent;
    [SerializeField] private float range;
    [SerializeField] private float maxSpeed;
    [SerializeField] private float speedMultiplier;
    private Rigidbody rBody;

    public Vector3 Location
    {
        get
        {
            return transform.localPosition;
        }
        private set
        {
            transform.localPosition = value;
        }
    }

    public Vector3 Velocity => rBody.velocity;
    
    // Start is called before the first frame update
    void Start()
    {
        rBody = GetComponent<Rigidbody>();
        Location = new Vector3(Random.Range(-1f, 1f) * range,
                                Location.y,
                                Random.Range(-1f, 1f) * range);
    }

    // Update is called once per frame
    void Update()
    {
        UpdateLocation();
    }

    public void Reset()
    {
        Location = new Vector3(Random.Range(-1f, 1f) * range,
                                Location.y,
                                Random.Range(-1f, 1f) * range);
        rBody.velocity = Vector3.zero;
        rBody.angularVelocity = Vector3.zero;
    }

    void UpdateLocation()
    {
        float x, z;

        if (agent.transform.localPosition.x > Location.x)
        {
            x = speedMultiplier;
        }
        else
        {
            x = -speedMultiplier;
        }

        if (agent.transform.localPosition.z > Location.z)
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
        x = x > maxSpeed ? 0 : x;
        z = z > maxSpeed ? 0 : z;
        
        if (x != 0 && z != 0)
        {
            rBody.AddForce(new Vector3(x, 0, z));
        }
    }
}
