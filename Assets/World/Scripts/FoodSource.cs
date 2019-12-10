using UnityEngine;

public class FoodSource : MonoBehaviour, IConsumable
{
    [SerializeField] private float health;
    [SerializeField] private int range;

    private float initialHealth;

    void Start()
    {
        initialHealth = health;
    }

    public bool Consume(float value)
    {
        health -= value;

        return health <= 0;
    }

    public void Reset()
    {
        health = initialHealth;
        gameObject.transform.localPosition = new Vector3(Random.Range(-1f, 1f) * range,
                                                        gameObject.transform.localPosition.y,
                                                        Random.Range(-1f, 1f) * range);
    }
}
