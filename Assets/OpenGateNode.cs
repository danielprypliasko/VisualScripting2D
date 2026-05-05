using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class GateNode : Node
{
    [SerializeField] private Transform inputPoint;
    [SerializeField] private Animator gateAnimator;
    [SerializeField] private float xpReward = 25f;
    private bool hasGivenXP = false;

    public override void Execute(string inputPort, NodeFlowContext context)
    {
        if (!BeginExecution(ref context))
        {
            return;
        }

        bool canOpen = context.Graph.Get("value", NodeValue.FromBool(false)).AsBool();

        if (canOpen)
        {
            gateAnimator.SetTrigger("OpenDoor");
            gateAnimator.SetTrigger("ActivateMagnet");


            if (XPController.instance != null && !hasGivenXP)
            {
                hasGivenXP = true;
                XPController.instance.AddXp(xpReward);

            }
        }
        ;

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
            _ => null
        };
    }
}