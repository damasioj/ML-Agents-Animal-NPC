using UnityEngine;

public class FoodTarget : BaseTarget, IConsumable
{
    /// <summary>
    /// Range of resources/health that this target has. Minimum is 10.
    /// </summary>
    [SerializeField] private float healthRange;
    /// <summary>
    /// The range at which this target can spawn in the scene. Uses localPosition.
    /// </summary>
    [SerializeField] private float range;
    
    [HideInInspector] public float hp;
    private float[] yPos;

    public bool IsConsumed => hp <= 0;
    public override bool IsValid => !IsConsumed;

    void Start()
    {
        hp = Random.Range(0f, healthRange);
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
        hp = Random.Range(10f, healthRange);
        transform.localPosition = new Vector3(Random.Range(-1f, 1f) * range, Random.Range(yPos[0], yPos[1]), Random.Range(-1f, 1f) * range);
        TargetHit = false;
    }
}
