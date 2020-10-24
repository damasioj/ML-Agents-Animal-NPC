using System;
using System.Collections.Generic;
using System.Linq;
using Unity.MLAgents;
using UnityEngine;

public class BambooAcademy : MonoBehaviour
{
    private AnimalAgent agent;
    private List<BaseTarget> targets;
    private List<Obstacle> obstacles;

    private void Awake()
    {
        Academy.Instance.OnEnvironmentReset += EnvironmentReset;
    }

    void Start()
    {
        agent = gameObject.GetComponentInChildren<AnimalAgent>();
        agent.EpisodeReset += EnvironmentReset;
        agent.TaskDone += UpdateTask;
        targets = gameObject.GetComponentsInChildren<BaseTarget>().ToList();
        obstacles = gameObject.GetComponentsInChildren<Obstacle>().ToList();
    }

    public void EnvironmentReset()
    {
        targets.ForEach(t => t.Reset());
        obstacles.ForEach(o => o.Reset());
        agent.UpdateTarget(targets);
    }

    public void EnvironmentReset(object sender, EventArgs e)
    {
        EnvironmentReset();
    }

    public void UpdateTask(object sender, EventArgs e)
    {
        if (sender is AnimalAgent animalAgent)
        {
            animalAgent.UpdateTarget(targets);
        }
    }
}
