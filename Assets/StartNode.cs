using System.Collections;
using UnityEngine;

public class StartNode : Node
{
    [SerializeField] private bool fireOnStart = true;
    [SerializeField] private bool loop;
    [SerializeField] private float interval = 1f;
    [SerializeField] private Transform outputPoint;

    private Coroutine pulseRoutine;

    protected override void OnEnable()
    {
        base.OnEnable();
        RestartPulseLoop();
    }

    private void OnDisable()
    {
        StopPulseLoop();
    }

    public override void Execute(string inputPort, NodeFlowContext context)
    {
    }

    public override NodeValue Evaluate(string outputPort, NodeFlowContext context)
    {
        return NodeValue.Flow;
    }

    public override Transform GetInputAnchor(string port)
    {
        return null;
    }

    public override Transform GetOutputAnchor(string port)
    {
        return NormalizePort(port, FlowOutPort) == FlowOutPort ? ResolveAnchor(outputPoint) : null;
    }

    [ContextMenu("Pulse")]
    public void Pulse()
    {
        Pulse(CreateContext());
    }

    private void RestartPulseLoop()
    {
        StopPulseLoop();

        if (!Application.isPlaying || !fireOnStart)
        {
            return;
        }

        pulseRoutine = StartCoroutine(PulseLoop());
    }

    private void StopPulseLoop()
    {
        if (pulseRoutine == null)
        {
            return;
        }

        StopCoroutine(pulseRoutine);
        pulseRoutine = null;
    }

    private IEnumerator PulseLoop()
    {
        yield return null;

        var delay = Mathf.Max(0.02f, interval);

        while (enabled)
        {
            Pulse(CreateContext());

            if (!loop)
            {
                break;
            }

            yield return new WaitForSeconds(delay);
        }

        pulseRoutine = null;
    }

    private void Pulse(NodeFlowContext context)
    {
        PulseVisual();
        Emit(FlowOutPort, context);
    }
}
