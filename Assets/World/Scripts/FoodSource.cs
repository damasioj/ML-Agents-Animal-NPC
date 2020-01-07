using UnityEngine;

public class FoodSource : MonoBehaviour, IConsumable
{
    [SerializeField] private float health;
    [SerializeField] private float range;

    public bool IsConsumed { get; private set; }
    public bool IsGoodConsumable
    {
        get
        {
            return tag == "food";
        }
    }

    private float hp;
    private float yPos;

    void Start()
    {
        hp = health;
        yPos = gameObject.transform.localPosition.y;
    }

    public bool Consume(float value)
    {
        hp -= value;

        if (hp <= 0)
        {
            IsConsumed = true;
            gameObject.transform.localPosition = new Vector3(99f, 99f, 99f);
            return true;
        }

        return false;
    }

    public void Reset()
    {
        hp = health;
        IsConsumed = false;
        gameObject.transform.localPosition = new Vector3(Random.Range(-1f, 1f) * range,
                                                        yPos,
                                                        Random.Range(-1f, 1f) * range);
    }
}
