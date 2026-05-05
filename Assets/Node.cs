using System;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    public const string FlowInPort = "In";
    public const string FlowOutPort = "Out";
    public const string ValuePort = "Value";

    [SerializeField] private NodeGraph graph;
    [SerializeField] private float activeScale = 1.15f;
    [SerializeField] private float pulseDecay = 4f;
    [SerializeField] private Color idleColor = Color.white;
    [SerializeField] private Color activeColor = new(0.35f, 1f, 0.55f, 1f);
    [SerializeField, Tooltip("Prevents player tools from moving or editing this node.")] private bool pinned;

    private readonly List<Wire> outgoingFlowWires = new();
    private readonly List<Wire> incomingValueWires = new();

    private SpriteRenderer spriteRenderer;
    private Vector3 baseScale;
    private float pulse;

    public bool IsPinned => pinned;

    protected virtual void Awake()
    {
        EnsureSetup();
        RefreshVisual();
    }

    protected virtual void OnEnable()
    {
        EnsureSetup();
        RefreshVisual();
    }

    protected virtual void OnValidate()
    {
        EnsureSetup();
        RefreshVisual();
    }

    protected virtual void Update()
    {
        if (!Application.isPlaying)
        {
            RefreshVisual();
            return;
        }

        if (pulse <= 0f)
        {
            return;
        }

        pulse = Mathf.MoveTowards(pulse, 0f, pulseDecay * Time.deltaTime);
        RefreshVisual();
    }

    public virtual void Execute(string inputPort, NodeFlowContext context)
    {
        if (!BeginExecution(ref context))
        {
            return;
        }

        Emit(FlowOutPort, context);
    }

    public virtual NodeValue Evaluate(string outputPort, NodeFlowContext context)
    {
        if (!BeginEvaluation(ref context))
        {
            return NodeValue.Flow;
        }

        return ReadValue(ValuePort, NodeValue.Flow, context);
    }

    [ContextMenu("Flow")]
    public void Flow()
    {
        Execute(FlowInPort, CreateContext());
    }

    public void Emit(string outputPort, NodeFlowContext context)
    {
        context ??= CreateContext();
        var normalizedPort = NormalizePort(outputPort, FlowOutPort);

        for (var i = outgoingFlowWires.Count - 1; i >= 0; i--)
        {
            var wire = outgoingFlowWires[i];

            if (wire == null)
            {
                outgoingFlowWires.RemoveAt(i);
                continue;
            }

            if (!wire.IsFlowOutputFrom(this, normalizedPort))
            {
                continue;
            }

            wire.TransmitFlow(this, context);
        }
    }

    public NodeValue ReadValue(string inputPort, NodeValue fallback, NodeFlowContext context)
    {
        var normalizedPort = NormalizePort(inputPort, ValuePort);

        for (var i = incomingValueWires.Count - 1; i >= 0; i--)
        {
            var wire = incomingValueWires[i];

            if (wire == null)
            {
                incomingValueWires.RemoveAt(i);
                continue;
            }

            if (!wire.IsValueInputFor(this, normalizedPort))
            {
                continue;
            }

            return wire.ReadValue(this, context);
        }

        return fallback;
    }

    public virtual Transform GetInputAnchor(string port)
    {
        return NormalizePort(port, FlowInPort) switch
        {
            FlowInPort => transform,
            ValuePort => transform,
            _ => null
        };
    }

    public virtual Transform GetOutputAnchor(string port)
    {
        return NormalizePort(port, FlowOutPort) switch
        {
            FlowOutPort => transform,
            ValuePort => transform,
            _ => null
        };
    }

    public NodeGraph ResolveGraph()
    {
        if (graph != null)
        {
            return graph;
        }

        graph = GetComponent<NodeGraph>();

        if (graph != null)
        {
            return graph;
        }

        graph = GetComponentInParent<NodeGraph>();

        if (graph != null)
        {
            return graph;
        }

        return graph;
    }

    public void RegisterOutgoingFlow(Wire wire)
    {
        if (wire == null || outgoingFlowWires.Contains(wire))
        {
            return;
        }

        outgoingFlowWires.Add(wire);
    }

    public void UnregisterOutgoingFlow(Wire wire)
    {
        if (wire == null)
        {
            return;
        }

        outgoingFlowWires.Remove(wire);
    }

    public void RegisterIncomingValue(Wire wire)
    {
        if (wire == null || incomingValueWires.Contains(wire))
        {
            return;
        }

        incomingValueWires.Add(wire);
    }

    public void UnregisterIncomingValue(Wire wire)
    {
        if (wire == null)
        {
            return;
        }

        incomingValueWires.Remove(wire);
    }

    private void EnsureSetup()
    {
        spriteRenderer ??= GetComponent<SpriteRenderer>();

        if (!Application.isPlaying || baseScale == Vector3.zero)
        {
            baseScale = transform.localScale;
        }
    }

    protected NodeFlowContext CreateContext()
    {
        var resolvedGraph = ResolveGraph();
        return resolvedGraph != null ? resolvedGraph.CreateContext() : new NodeFlowContext(null, 256);
    }

    protected bool BeginExecution(ref NodeFlowContext context)
    {
        context ??= CreateContext();

        if (!context.TryStep())
        {
            return false;
        }

        PulseVisual();
        return true;
    }

    protected bool BeginEvaluation(ref NodeFlowContext context)
    {
        context ??= CreateContext();

        if (!context.TryStep())
        {
            return false;
        }

        PulseVisual();
        return true;
    }

    protected void PulseVisual()
    {
        pulse = 1f;
        RefreshVisual();
    }

    protected Transform ResolveAnchor(Transform anchor)
    {
        return anchor != null ? anchor : transform;
    }

    public static string NormalizePort(string port, string fallback)
    {
        return string.IsNullOrWhiteSpace(port) ? fallback : port.Trim();
    }

    public static bool PortMatches(string a, string b, string fallbackA, string fallbackB)
    {
        return string.Equals(NormalizePort(a, fallbackA), NormalizePort(b, fallbackB), StringComparison.OrdinalIgnoreCase);
    }

    private void RefreshVisual()
    {
        var strength = Mathf.Clamp01(pulse);

        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.Lerp(idleColor, activeColor, strength);
        }

        if (baseScale != Vector3.zero)
        {
            transform.localScale = baseScale * Mathf.Lerp(1f, activeScale, strength);
        }
    }
}
