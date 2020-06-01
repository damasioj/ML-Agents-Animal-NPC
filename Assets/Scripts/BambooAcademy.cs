using Unity.MLAgents;
using UnityEngine;

public class BambooAcademy : MonoBehaviour
{
    private Academy bambooAcedemy;
    private Agent agent;
    private FoodTarget target;

    private void Awake()
    {
        bambooAcedemy = Academy.Instance;
        Academy.Instance.OnEnvironmentReset += EnvironmentReset;
    }

    void Start()
    {
        //isFirstRun = true;
        //boundaryLimits = GetBoundaryLimits();
        agent = gameObject.GetComponentInChildren<AnimalAgent>();
        target = gameObject.GetComponentInChildren<FoodTarget>();
        //goal = gameObject.GetComponentInChildren<BaseGoal>();

        //agent.BoundaryLimits = boundaryLimits;
        //targets.ForEach(t => t.BoundaryLimits = boundaryLimits);
        //goal.goalLimits = GetGoalLimits();
        //runningRewardTotals = new Queue<float>(3);
    }

    private void FixedUpdate()
    {
        agent.RequestDecision();
        
        //if (agent.IsDoneJob)
        //{
        //    SetAgentTarget();
        //}
    }

    private void EnvironmentReset()
    {
        target.Reset();
    }
}
