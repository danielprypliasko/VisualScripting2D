using UnityEngine;

public class RepeatNode : Node
{
    [SerializeField] private int fallbackCount = 3;
    [SerializeField] private bool writeIndexVariable;
    [SerializeField] private string indexVariable = "i";
    [SerializeField] private Transform inputPoint;
    [SerializeField] private Transform countInputPoint;
    [SerializeField] private Transform bodyOutputPoint;
    [SerializeField] private Transform doneOutputPoint;

    public override void Execute(string inputPort, NodeFlowContext context)
    {
        if (!BeginExecution(ref context))
        {
            return;
        }

        var countValue = ReadValue("Count", NodeValue.FromNumber(fallbackCount), context);
        var count = Mathf.Max(0, countValue.AsInt(fallbackCount));
        var resolvedGraph = context.Graph ?? ResolveGraph();

        for (var i = 0; i < count; i++)
        {
            if (!context.HasSteps)
            {
                return;
            }

            if (writeIndexVariable && resolvedGraph != null)
            {
                resolvedGraph.Set(indexVariable, NodeValue.FromNumber(i));
            }

            Emit("Body", context);
        }

        Emit("Done", context);
    }

    public override NodeValue Evaluate(string outputPort, NodeFlowContext context)
    {
        var fallback = NodeValue.FromNumber(fallbackCount);

        if (!BeginEvaluation(ref context))
        {
            return fallback;
        }

        return ReadValue("Count", fallback, context);
    }

    public override Transform GetInputAnchor(string port)
    {
        return NormalizePort(port, FlowInPort) switch
        {
            FlowInPort => ResolveAnchor(inputPoint),
            "Count" => ResolveAnchor(countInputPoint),
            _ => null
        };
    }

    public override Transform GetOutputAnchor(string port)
    {
        return NormalizePort(port, FlowOutPort) switch
        {
            "Body" => ResolveAnchor(bodyOutputPoint),
            "Done" => ResolveAnchor(doneOutputPoint),
            _ => null
        };
    }
}
