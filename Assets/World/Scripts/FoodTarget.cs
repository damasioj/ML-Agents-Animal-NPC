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
    [SerializeField] private float positionRange;
    /// <summary>
    /// The target's minimum acceptable scale. This is used only to reduce bias during training.
    /// </summary>
    [SerializeField] private float minScale;
    /// <summary>
    /// The target's maximum acceptable scale. This is used only to reduce bias during training.
    /// </summary>
    [SerializeField] private float maxScale;
    
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
        float scale = Random.Range(minScale, maxScale);

        hp = Random.Range(10f, healthRange);
        transform.localPosition = new Vector3(Random.Range(-1f, 1f) * positionRange, Random.Range(yPos[0], yPos[1]), Random.Range(-1f, 1f) * positionRange);        
        transform.localScale = new Vector3(scale, transform.localScale.y, scale);
        TargetHit = false;
    }
}
