using UnityEngine;

public abstract class BaseTarget : MonoBehaviour
{
    public float x, y, z;
    public bool TargetHit { get; protected set; }
    public abstract bool IsValid { get; }
    
    private void Awake()
    {
        TargetHit = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        TargetHit = true;
    }

    public abstract void Reset();
}
