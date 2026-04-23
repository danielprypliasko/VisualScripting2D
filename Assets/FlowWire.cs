using UnityEngine;

public enum FlowWireOutputPort
{
    Out,
    True,
    False,
    Body,
    Done
}

[AddComponentMenu("Visual Scripting/Flow Wire")]
public class FlowWire : Wire
{
    [SerializeField] private FlowWireOutputPort outputPort;

    protected override WireKind GetWireKind()
    {
        return WireKind.Flow;
    }

    protected override string GetSourcePort()
    {
        return outputPort.ToString();
    }

    protected override string GetTargetPort()
    {
        return Node.FlowInPort;
    }
}
