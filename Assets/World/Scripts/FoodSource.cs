using UnityEngine;

public class FoodSource : MonoBehaviour, IConsumable
{
    [SerializeField] private float health;
    [SerializeField] private float range;

    public bool IsConsumed { get; private set; }

    private float initialHealth;
    private float yPos;

    void Start()
    {
        initialHealth = health;
        yPos = gameObject.transform.localPosition.y;
    }

    public bool Consume(float value)
    {
        health -= value;

        if (health <= 0)
        {
            IsConsumed = true;
            gameObject.transform.localPosition = new Vector3(99f, 99f, 99f);
            return true;
        }

        return false;
    }

    public void Reset()
    {
        health = initialHealth;
        IsConsumed = false;
        gameObject.transform.localPosition = new Vector3(Random.Range(-1f, 1f) * range,
                                                        yPos,
                                                        Random.Range(-1f, 1f) * range);
    }
}
