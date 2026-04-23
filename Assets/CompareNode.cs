using UnityEngine;

public enum CompareOperation
{
    Equal,
    NotEqual,
    Greater,
    GreaterOrEqual,
    Less,
    LessOrEqual
}

public class CompareNode : Node
{
    [SerializeField] private CompareOperation operation;
    [SerializeField] private NodeValue fallbackA = NodeValue.FromNumber(0f);
    [SerializeField] private NodeValue fallbackB = NodeValue.FromNumber(0f);
    [SerializeField] private Transform aInputPoint;
    [SerializeField] private Transform bInputPoint;
    [SerializeField] private Transform outputPoint;

    public override NodeValue Evaluate(string outputPort, NodeFlowContext context)
    {
        if (!BeginEvaluation(ref context))
        {
            return NodeValue.FromBool(false);
        }

        var a = ReadValue("A", fallbackA, context);
        var b = ReadValue("B", fallbackB, context);
        return NodeValue.FromBool(Compare(a, b));
    }

    private bool Compare(NodeValue a, NodeValue b)
    {
        return operation switch
        {
            CompareOperation.Equal => a.ValueEquals(b),
            CompareOperation.NotEqual => !a.ValueEquals(b),
            CompareOperation.Greater => a.AsNumber() > b.AsNumber(),
            CompareOperation.GreaterOrEqual => a.AsNumber() >= b.AsNumber(),
            CompareOperation.Less => a.AsNumber() < b.AsNumber(),
            CompareOperation.LessOrEqual => a.AsNumber() <= b.AsNumber(),
            _ => false
        };
    }

    public override Transform GetInputAnchor(string port)
    {
        return NormalizePort(port, ValuePort) switch
        {
            "A" => ResolveAnchor(aInputPoint),
            "B" => ResolveAnchor(bInputPoint),
            _ => null
        };
    }

    public override Transform GetOutputAnchor(string port)
    {
        return NormalizePort(port, ValuePort) == ValuePort ? ResolveAnchor(outputPoint) : null;
    }
}
