using UnityEngine;

public class SetVariableNode : Node
{
    [SerializeField] private string variableName = "value";
    [SerializeField] private NodeValue fallbackValue = NodeValue.FromNumber(0f);
    [SerializeField] private Transform inputPoint;
    [SerializeField] private Transform outputPoint;
    [SerializeField] private Transform valueInputPoint;

    public override void Execute(string inputPort, NodeFlowContext context)
    {
        if (!BeginExecution(ref context))
        {
            return;
        }

        var value = ReadValue(ValuePort, fallbackValue, context);
        var resolvedGraph = context.Graph ?? ResolveGraph();

        if (resolvedGraph != null)
        {
            resolvedGraph.Set(variableName, value);
        }

        Emit(FlowOutPort, context);
    }

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
        return NormalizePort(port, FlowInPort) switch
        {
            FlowInPort => ResolveAnchor(inputPoint),
            ValuePort => ResolveAnchor(valueInputPoint),
            _ => null
        };
    }

    public override Transform GetOutputAnchor(string port)
    {
        return NormalizePort(port, FlowOutPort) switch
        {
            FlowOutPort => ResolveAnchor(outputPoint),
            ValuePort => ResolveAnchor(outputPoint),
            _ => null
        };
    }
}
