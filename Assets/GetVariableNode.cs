using UnityEngine;

public class GetVariableNode : Node
{
    [SerializeField] private string variableName = "value";
    [SerializeField] private NodeValue fallbackValue = NodeValue.FromNumber(0f);
    [SerializeField] private Transform outputPoint;

    public override NodeValue Evaluate(string outputPort, NodeFlowContext context)
    {
        if (!BeginEvaluation(ref context))
        {
            return fallbackValue;
        }

        var resolvedGraph = context?.Graph ?? ResolveGraph();
        return resolvedGraph != null ? resolvedGraph.Get(variableName, fallbackValue) : fallbackValue;
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
