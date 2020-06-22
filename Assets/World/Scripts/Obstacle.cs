using UnityEngine;

public class Obstacle : MonoBehaviour
{
    public float range;
    private float yPos;

    private void Start()
    {
        yPos = transform.localPosition.y;
    }

    public void Reset()
    {
        transform.localPosition = new Vector3(Random.Range(-1f, 1f) * range, yPos, Random.Range(-1f, 1f) * range);
    }
}
