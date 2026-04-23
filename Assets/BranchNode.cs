using UnityEngine;

public class BranchNode : Node
{
    [SerializeField] private NodeValue fallbackCondition = NodeValue.FromBool(false);
    [SerializeField] private Transform inputPoint;
    [SerializeField] private Transform conditionInputPoint;
    [SerializeField] private Transform trueOutputPoint;
    [SerializeField] private Transform falseOutputPoint;

    public override void Execute(string inputPort, NodeFlowContext context)
    {
        if (!BeginExecution(ref context))
        {
            return;
        }

        var condition = ReadValue("Condition", fallbackCondition, context);
        Emit(condition.AsBool() ? "True" : "False", context);
    }

    public override NodeValue Evaluate(string outputPort, NodeFlowContext context)
    {
        if (!BeginEvaluation(ref context))
        {
            return fallbackCondition;
        }

        return ReadValue("Condition", fallbackCondition, context);
    }

    public override Transform GetInputAnchor(string port)
    {
        return NormalizePort(port, FlowInPort) switch
        {
            FlowInPort => ResolveAnchor(inputPoint),
            "Condition" => ResolveAnchor(conditionInputPoint),
            _ => null
        };
    }

    public override Transform GetOutputAnchor(string port)
    {
        return NormalizePort(port, FlowOutPort) switch
        {
            "True" => ResolveAnchor(trueOutputPoint),
            "False" => ResolveAnchor(falseOutputPoint),
            _ => null
        };
    }
}
