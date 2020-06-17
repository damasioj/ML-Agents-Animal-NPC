using System;
using System.Collections.Generic;
using System.Linq;
using Unity.MLAgents;
using UnityEngine;

public class BambooAcademy : MonoBehaviour
{
    private Academy bambooAcademy;
    private AnimalAgent agent;
    private List<BaseTarget> targets;
    private List<Obstacle> obstacles;

    private void Awake()
    {
        bambooAcademy = Academy.Instance;
        Academy.Instance.OnEnvironmentReset += EnvironmentReset;
    }

    void Start()
    {
        agent = gameObject.GetComponentInChildren<AnimalAgent>();
        agent.EpisodeReset += EnvironmentReset;
        targets = gameObject.GetComponentsInChildren<BaseTarget>().ToList();
        obstacles = gameObject.GetComponentsInChildren<Obstacle>().ToList();
    }

    public void EnvironmentReset()
    {
        targets.ForEach(t => t.Reset());
        obstacles.ForEach(o => o.Reset());
    }

    public void EnvironmentReset(object sender, EventArgs e)
    {
        EnvironmentReset();
    }
}
