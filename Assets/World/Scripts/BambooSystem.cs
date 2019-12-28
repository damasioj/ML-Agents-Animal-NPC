using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BambooSystem : MonoBehaviour
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
