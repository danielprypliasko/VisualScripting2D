using Unity.VisualScripting;
using UnityEngine;

public class GateNode : Node
{
    [SerializeField] private Transform inputPoint;
    [SerializeField] private Transform outputPoint;
    [SerializeField] private Animator gateAnimator;

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
            Emit(FlowOutPort, context);
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
            FlowOutPort => ResolveAnchor(outputPoint),
            _ => null
        };
    }
}