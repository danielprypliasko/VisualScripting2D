using UnityEngine;

public class LogNode : Node
{
    [SerializeField] private string prefix;
    [SerializeField] private NodeValue fallbackValue = NodeValue.Flow;
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
        var message = string.IsNullOrWhiteSpace(prefix) ? value.ToString() : $"{prefix}{value}";
        Debug.Log(message, this);
        Emit(FlowOutPort, context);
    }

    public override NodeValue Evaluate(string outputPort, NodeFlowContext context)
    {
        if (!BeginEvaluation(ref context))
        {
            return fallbackValue;
        }

        return ReadValue(ValuePort, fallbackValue, context);
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
