using UnityEngine;

public abstract class BaseTarget : MonoBehaviour
{
    public bool TargetHit { get; protected set; }

    private void Awake()
    {
        TargetHit = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        TargetHit = true;
    }

    public virtual Vector3 Location
    {
        get
        {
            return gameObject.transform.position;
        }
        private set
        {
            gameObject.transform.position = value;
        }
    }

    public abstract void Reset();
}
