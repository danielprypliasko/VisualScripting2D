using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class SimpleWireTool : MonoBehaviour
{
    public enum ToolMode { Flow, Value, Delete, PickUp }

    private static readonly string[] FlowOutputPorts = { Node.FlowOutPort, "True", "False", "Body", "Done" };
    private static readonly string[] FlowInputPorts = { Node.FlowInPort };
    private static readonly string[] ValueOutputPorts = { Node.ValuePort };
    private static readonly string[] ValueInputPorts = { Node.ValuePort, "Condition", "A", "B", "Count" };

    public Wire flowTemplate;
    public ValueWire valueTemplate;
    public float interactDistance = 3f;
    public ToolMode currentMode = ToolMode.Flow;
    public TextMeshProUGUI modeDisplay;

    private Node firstNode;
    private string firstPort;
    private Wire activeWire;
    private Node heldNode;
    private Vector2 heldNodeOffset;
    private float heldNodeZ;
    private Collider2D[] heldNodeColliders;
    private bool[] heldNodeColliderStates;

    void Start()
    {
        UpdateUI();
    }

    void Update()
    {
        if (Keyboard.current.digit1Key.wasPressedThisFrame) SetMode(ToolMode.Flow);
        if (Keyboard.current.digit2Key.wasPressedThisFrame) SetMode(ToolMode.Value);
        if (Keyboard.current.digit3Key.wasPressedThisFrame) SetMode(ToolMode.Delete);
        if (Keyboard.current.digit4Key.wasPressedThisFrame) SetMode(ToolMode.PickUp);
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            CancelCurrentPlacement();
            DropHeldNode();
        }

        if (Keyboard.current.fKey.wasPressedThisFrame)
        {
            switch (currentMode)
            {
                case ToolMode.Delete:
                    TryDeleteNearestWire();
                    break;
                case ToolMode.PickUp:
                    TryToggleHeldNode();
                    break;
                default:
                    TryHandleWiring(currentMode == ToolMode.Value);
                    break;
            }
        }

        UpdateActiveWirePreview();
        UpdateHeldNodePosition();
    }

    private void SetMode(ToolMode newMode)
    {
        currentMode = newMode;
        CancelCurrentPlacement();
        DropHeldNode();
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (modeDisplay == null) return;

        modeDisplay.richText = true;
        string mode = GetModeLabel();
        string flow = "1) Flow Wire Tool (yellow)" + (currentMode == ToolMode.Flow ? " <-" : "");
        string value = "2) Value Wire Tool (blue)" + (currentMode == ToolMode.Value ? " <-" : "");
        string delete = "3) Delete Tool" + (currentMode == ToolMode.Delete ? " <-" : "");
        string pickUp = "4) Pick Up Node Tool" + (currentMode == ToolMode.PickUp ? " <-" : "");
        string action = GetActionText();

        modeDisplay.text = $"{mode}\n{action}\nPress 1, 2, 3, or 4 to change tools\n{flow}\n{value}\n{delete}\n{pickUp}";
    }

    private string GetModeLabel()
    {
        return currentMode switch
        {
            ToolMode.Flow => "<size=125%><b>Using: <color=#FFD95A>FLOW WIRE</color></b></size>",
            ToolMode.Value => "<size=125%><b>Using: <color=#A6CCFF>VALUE WIRE</color></b></size>",
            ToolMode.Delete => "<size=125%><b>Using: <color=#FF7A7A>DELETE</color></b></size>",
            ToolMode.PickUp => "<size=125%><b>Using: <color=#9CFF8F>PICK UP NODE</color></b></size>",
            _ => "<size=125%><b>Using: WIRE TOOL</b></size>"
        };
    }

    private string GetActionText()
    {
        return currentMode switch
        {
            ToolMode.Delete => "Stand near a wire and press F to delete it.",
            ToolMode.PickUp when heldNode != null => $"Holding {heldNode.name}. Press F to drop. Esc cancels.",
            ToolMode.PickUp => "Stand near a node and press F to pick it up.",
            _ when firstNode == null => $"Stand near an output port and press F to start a {GetWireModeName()}.",
            _ => $"Placing a {GetWireModeName()} from {firstNode.name}.{firstPort}. Press Esc to cancel."
        };
    }

    private string GetWireModeName()
    {
        return currentMode == ToolMode.Value ? "VALUE wire" : "FLOW wire";
    }

    private void TryHandleWiring(bool isValueMode)
    {
        bool choosingSource = firstNode == null;
        Node closest = GetClosestNode(out string port, isValueMode, choosingSource);
        if (closest == null)
        {
            Debug.LogWarning(choosingSource ? "No output port in range" : "No input port in range");
            return;
        }

        if (IsPortOccupied(closest, isValueMode, choosingSource, port))
        {
            Debug.LogWarning($"{closest.name}.{port} is already wired");
            return;
        }

        if (firstNode == null)
        {
            Wire template = isValueMode ? valueTemplate : flowTemplate;
            if (template == null) return;

            firstNode = closest;
            firstPort = port;
            activeWire = Instantiate(template);
            activeWire.gameObject.SetActive(true);
            activeWire.transform.position = new Vector3(activeWire.transform.position.x, activeWire.transform.position.y, 0f);
            SetWirePort(activeWire, isValueMode, port, true);
            activeWire.Source = firstNode;
            activeWire.SetPreviewTarget(transform.position);
            UpdateUI();
        }
        else
        {
            if (closest == firstNode) return;

            SetWirePort(activeWire, isValueMode, port, false);
            activeWire.ClearPreviewTarget();
            activeWire.Target = closest;
            firstNode = null;
            firstPort = null;
            activeWire = null;
            UpdateUI();
        }
    }

    private void TryDeleteNearestWire()
    {
        Wire[] allWires = Object.FindObjectsByType<Wire>(FindObjectsSortMode.None);
        Wire target = null;
        float minDistance = interactDistance;

        foreach (Wire w in allWires)
        {
            if (IsPinnedForPlayer(w))
            {
                continue;
            }

            float dist = DistanceToWire(w);
            if (dist < minDistance) { minDistance = dist; target = w; }
        }

        if (target != null) Destroy(target.gameObject);
    }

    private void TryToggleHeldNode()
    {
        if (heldNode != null)
        {
            DropHeldNode();
            return;
        }

        Node node = GetClosestPickupNode();
        if (node == null)
        {
            Debug.LogWarning("No node in range");
            return;
        }

        heldNode = node;
        heldNodeOffset = node.transform.position - transform.position;
        heldNodeZ = node.transform.position.z;
        SetHeldNodeCollisions(false);
        UpdateUI();
    }

    private void CancelCurrentPlacement()
    {
        if (activeWire != null) Destroy(activeWire.gameObject);
        firstNode = null;
        firstPort = null;
        activeWire = null;
        UpdateUI();
    }

    private void UpdateActiveWirePreview()
    {
        if (activeWire == null || firstNode == null)
        {
            return;
        }

        activeWire.SetPreviewTarget(transform.position);
    }

    private void UpdateHeldNodePosition()
    {
        if (heldNode == null)
        {
            return;
        }

        Vector3 targetPosition = transform.position + (Vector3)heldNodeOffset;
        targetPosition.z = heldNodeZ;
        heldNode.transform.position = targetPosition;
    }

    private void DropHeldNode()
    {
        if (heldNode == null)
        {
            return;
        }

        UpdateHeldNodePosition();
        SetHeldNodeCollisions(true);
        heldNode = null;
        heldNodeColliders = null;
        heldNodeColliderStates = null;
        UpdateUI();
    }

    private void SetHeldNodeCollisions(bool enabled)
    {
        if (heldNode == null)
        {
            return;
        }

        if (!enabled)
        {
            heldNodeColliders = heldNode.GetComponentsInChildren<Collider2D>();
            heldNodeColliderStates = new bool[heldNodeColliders.Length];
        }

        if (heldNodeColliders == null || heldNodeColliderStates == null)
        {
            return;
        }

        for (int i = 0; i < heldNodeColliders.Length; i++)
        {
            Collider2D nodeCollider = heldNodeColliders[i];
            if (nodeCollider == null)
            {
                continue;
            }

            if (enabled)
            {
                nodeCollider.enabled = heldNodeColliderStates[i];
            }
            else
            {
                heldNodeColliderStates[i] = nodeCollider.enabled;
                nodeCollider.enabled = false;
            }
        }
    }

    private bool IsPortOccupied(Node node, bool isValueWire, bool checkingSource, string port)
    {
        if (isValueWire && checkingSource)
        {
            return false;
        }

        Wire[] wires = Object.FindObjectsByType<Wire>(FindObjectsSortMode.None);
        WireKind kind = isValueWire ? WireKind.Value : WireKind.Flow;
        string fallback = checkingSource
            ? (isValueWire ? Node.ValuePort : Node.FlowOutPort)
            : (isValueWire ? Node.ValuePort : Node.FlowInPort);

        foreach (Wire w in wires)
        {
            if (w.Kind != kind) continue;

            if (checkingSource && w.Source == node && Node.PortMatches(w.SourcePort, port, fallback, fallback))
            {
                return true;
            }

            if (!checkingSource && w.Target == node && Node.PortMatches(w.TargetPort, port, fallback, fallback))
            {
                return true;
            }
        }

        return false;
    }

    private Node GetClosestNode(out string port, bool isValueMode, bool choosingSource)
    {
        string[] ports = GetCandidatePorts(isValueMode, choosingSource);
        port = ports.Length > 0 ? ports[0] : Node.ValuePort;
        Node[] allNodes = Object.FindObjectsByType<Node>(FindObjectsSortMode.None);
        Node closest = null;
        float minDist = interactDistance;

        foreach (Node n in allNodes)
        {
            if (IsPinnedForPlayer(n))
            {
                continue;
            }

            foreach (string p in ports)
            {
                Transform anchor = choosingSource ? n.GetOutputAnchor(p) : n.GetInputAnchor(p);

                if (anchor == null) continue;

                float dist = Vector2.Distance(transform.position, anchor.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    closest = n;
                    port = p;
                }
            }
        }
        return closest;
    }

    private Node GetClosestPickupNode()
    {
        Node[] allNodes = Object.FindObjectsByType<Node>(FindObjectsSortMode.None);
        Node closest = null;
        float minDist = interactDistance;

        foreach (Node node in allNodes)
        {
            if (IsPinnedForPlayer(node))
            {
                continue;
            }

            float dist = Vector2.Distance(transform.position, node.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = node;
            }
        }

        return closest;
    }

    private static bool IsPinnedForPlayer(Node node)
    {
        return node != null && node.IsPinned;
    }

    private static bool IsPinnedForPlayer(Wire wire)
    {
        return wire != null && (wire.IsPinned || IsPinnedForPlayer(wire.Source) || IsPinnedForPlayer(wire.Target));
    }

    private static string[] GetCandidatePorts(bool isValueMode, bool choosingSource)
    {
        if (isValueMode)
        {
            return choosingSource ? ValueOutputPorts : ValueInputPorts;
        }

        return choosingSource ? FlowOutputPorts : FlowInputPorts;
    }

    private void SetWirePort(Wire wire, bool isValueWire, string port, bool isSource)
    {
        if (isValueWire)
        {
            if (!isSource && wire is ValueWire valueWire && System.Enum.TryParse(port, out ValueWireInputPort parsedValuePort))
            {
                valueWire.SetInputPort(parsedValuePort);
            }
            else if (!isSource)
            {
                wire.SetTargetPort(port);
            }

            return;
        }

        if (isSource && wire is FlowWire flowWire && System.Enum.TryParse(port, out FlowWireOutputPort parsedFlowPort))
        {
            flowWire.SetOutputPort(parsedFlowPort);
            return;
        }

        if (isSource)
        {
            wire.SetSourcePort(port);
        }
        else
        {
            wire.SetTargetPort(port);
        }
    }

    private float DistanceToWire(Wire wire)
    {
        if (wire == null) return float.PositiveInfinity;

        Transform sourceAnchor = wire.Source != null ? wire.Source.GetOutputAnchor(wire.SourcePort) : null;
        Transform targetAnchor = wire.Target != null ? wire.Target.GetInputAnchor(wire.TargetPort) : null;

        if (sourceAnchor != null && targetAnchor != null)
        {
            return DistanceToSegment(transform.position, sourceAnchor.position, targetAnchor.position);
        }

        if (sourceAnchor != null) return Vector2.Distance(transform.position, sourceAnchor.position);
        if (targetAnchor != null) return Vector2.Distance(transform.position, targetAnchor.position);
        return float.PositiveInfinity;
    }

    private static float DistanceToSegment(Vector2 point, Vector2 start, Vector2 end)
    {
        Vector2 segment = end - start;
        if (segment.sqrMagnitude <= 0.0001f)
        {
            return Vector2.Distance(point, start);
        }

        float t = Mathf.Clamp01(Vector2.Dot(point - start, segment) / segment.sqrMagnitude);
        return Vector2.Distance(point, start + segment * t);
    }
}
