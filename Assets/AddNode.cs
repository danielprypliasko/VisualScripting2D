using UnityEngine;

public class AddNode : Node
{
    [SerializeField] private float fallbackA;
    [SerializeField] private float fallbackB;
    [SerializeField] private Transform aInputPoint;
    [SerializeField] private Transform bInputPoint;
    [SerializeField] private Transform outputPoint;

    public override NodeValue Evaluate(string outputPort, NodeFlowContext context)
    {
        var fallback = NodeValue.FromNumber(fallbackA + fallbackB);

        if (!BeginEvaluation(ref context))
        {
            return fallback;
        }

        var a = ReadValue("A", NodeValue.FromNumber(fallbackA), context).AsNumber(fallbackA);
        var b = ReadValue("B", NodeValue.FromNumber(fallbackB), context).AsNumber(fallbackB);
        return NodeValue.FromNumber(a + b);
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
