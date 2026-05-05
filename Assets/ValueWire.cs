using UnityEngine;

public enum ValueWireInputPort
{
    Value,
    Condition,
    A,
    B,
    Count
}

[AddComponentMenu("Visual Scripting/Value Wire")]
public class ValueWire : Wire
{
    [SerializeField] private ValueWireInputPort inputPort = ValueWireInputPort.Value;

    public void SetInputPort(ValueWireInputPort port)
    {
        inputPort = port;
        SetTargetPort(inputPort.ToString());
    }

    protected override WireKind GetWireKind()
    {
        return WireKind.Value;
    }

    protected override string GetSourcePort()
    {
        return Node.ValuePort;
    }

    protected override string GetTargetPort()
    {
        return inputPort.ToString();
    }
}
