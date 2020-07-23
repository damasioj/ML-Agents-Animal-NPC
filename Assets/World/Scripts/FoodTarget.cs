using UnityEngine;

public class FoodTarget : BaseTarget, IConsumable
{
    [SerializeField] private float health;
    [SerializeField] private float range;
    
    [HideInInspector] public float hp;
    private float[] yPos;

    public bool IsConsumed => hp <= 0;
    public override bool IsValid => !IsConsumed;

    void Start()
    {
        hp = health;
        yPos = new float[] { 6f, -20f }; // just used to randomize Y value
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
        transform.localPosition = new Vector3(Random.Range(-1f, 1f) * range, Random.Range(yPos[0], yPos[1]), Random.Range(-1f, 1f) * range);
        TargetHit = false;
    }
}
