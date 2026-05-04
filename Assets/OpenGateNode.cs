using UnityEngine;

public class GateNode : Node
{
    [SerializeField] private Transform inputPoint;
    [SerializeField] private Transform outputPoint;
    [SerializeField] private Animator gateAnimator;
    [SerializeField] private float xpReward = 25f;
    private bool hasGivenXP = false;

    public override void Execute(string inputPort, NodeFlowContext context)
    {
        if (!BeginExecution(ref context))
        {
            return;
        }

        if (XPController.instance != null && !hasGivenXP)
        {
            hasGivenXP = true;
            XPController.instance.AddXp(xpReward);

        }

        gateAnimator.SetTrigger("OpenDoor");

        Emit(FlowOutPort, context);

    }

    public override Transform GetInputAnchor(string port)
    {
        return NormalizePort(port, FlowInPort) switch
        {
            FlowInPort => ResolveAnchor(inputPoint),
            _ => null
        };
    }

    public override Transform GetOutputAnchor(string port)
    {
        return NormalizePort(port, FlowOutPort) switch
        {
            FlowOutPort => ResolveAnchor(outputPoint),
            _ => null
        };
    }
}