using UnityEngine;

public class FoodTarget : BaseTarget, IConsumable
{
    [SerializeField] private float health;
    [SerializeField] private float range;
    
    [HideInInspector] public float hp;
    private float yPos;

    public bool IsConsumed => hp <= 0;

    void Start()
    {
        hp = health;
        yPos = gameObject.transform.position.y;
    }

    public bool Consume(float value)
    {
        hp -= value;

        if (IsConsumed)
        {
            return true;
        }

        return false;
    }

    public override void Reset()
    {
        hp = health;
        transform.localPosition = new Vector3(Random.Range(-1f, 1f) * range, yPos, Random.Range(-1f, 1f) * range);
        TargetHit = false;
    }
}
