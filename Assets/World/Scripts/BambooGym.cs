using UnityEngine;

public class BambooGym : MonoBehaviour
{
    TextMesh energyValue;
    AnimalAgent agent;

    void Start()
    {
        energyValue = gameObject.GetComponentInChildren<TextMesh>();
        agent = gameObject.GetComponentInChildren<AnimalAgent>();
    }

    private void FixedUpdate()
    {
        energyValue.text = agent.energy.ToString();
    }
}
