using UnityEngine;

public class FoodTarget : BaseTarget, IConsumable
{
    [SerializeField] private float health;
    [SerializeField] private float range;

    public bool IsConsumed { get; private set; }
    public bool IsGoodConsumable
    {
        get
        {
            return CompareTag("food");
        }
    }

    private float hp;
    private float yPos;

    void Start()
    {
        hp = health;
        yPos = gameObject.transform.position.y;
    }

    public bool Consume(float value)
    {
        hp -= value;

        if (hp <= 0)
        {
            IsConsumed = true;
            //gameObject.transform.position = new Vector3(99f, 99f, 99f);
            return true;
        }

        return false;
    }

    public override void Reset()
    {
        hp = health;
        IsConsumed = false;
        gameObject.transform.position = new Vector3(Random.Range(-1f, 1f) * range,
                                                        yPos,
                                                        Random.Range(-1f, 1f) * range);
        TargetHit = false;
    }
}
