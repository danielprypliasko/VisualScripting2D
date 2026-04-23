using UnityEngine;

public class ConstantNode : Node
{
    [SerializeField] private NodeValue value = NodeValue.FromNumber(0f);
    [SerializeField] private Transform outputPoint;

    public override NodeValue Evaluate(string outputPort, NodeFlowContext context)
    {
        if (!BeginEvaluation(ref context))
        {
            return value;
        }

        return value;
    }

    public override Transform GetInputAnchor(string port)
    {
        return null;
    }

    public override Transform GetOutputAnchor(string port)
    {
        return NormalizePort(port, ValuePort) == ValuePort ? ResolveAnchor(outputPoint) : null;
    }
}
