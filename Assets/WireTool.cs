using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class SimpleWireTool : MonoBehaviour
{
    public enum ToolMode { Flow, Value, Delete }

    public Wire flowTemplate;
    public ValueWire valueTemplate;

    public float interactDistance = 3f;
    public ToolMode currentMode = ToolMode.Flow;

    public TextMeshProUGUI modeDisplay;

    private Node firstNode;
    private Wire activeWire;

    void Start()
    {
        UpdateUI(); // Set initial UI text
    }

    void Update()
    { 

        // Sets what tool to use
        if (Keyboard.current.digit1Key.wasPressedThisFrame) { 
            SetMode(ToolMode.Flow); }
        if (Keyboard.current.digit2Key.wasPressedThisFrame) { 
            SetMode(ToolMode.Value); }
        if (Keyboard.current.digit3Key.wasPressedThisFrame) { 
            SetMode(ToolMode.Delete); }

        if (Keyboard.current.fKey.wasPressedThisFrame)
        {
            if (currentMode == ToolMode.Delete) TryDeleteNearestWire();
            else TryHandleWiring(currentMode == ToolMode.Value);
        }
    }

    private void SetMode(ToolMode newMode)
    {
        currentMode = newMode;
        CancelCurrentPlacement();
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (modeDisplay == null) return;

        // The menu string with the (Equipped) tag
        string flowLine = "1) Flow Wire Tool" + (currentMode == ToolMode.Flow ? " (Equipped)" : "");
        string valueLine = "2) Value Wire Tool" + (currentMode == ToolMode.Value ? " (Equipped)" : "");
        string deleteLine = "3) Delete Tool" + (currentMode == ToolMode.Delete ? " (Equipped)" : "");

        modeDisplay.text = $"Press 1,2 and 3 to change tools\nF key to use!\n{flowLine}\n{valueLine}\n{deleteLine}";
    }

    private void TryHandleWiring(bool isValueMode)
    {
        Node closestNode = GetClosestNode();
        if (closestNode == null) return;

        if (!CanNodeHandleWire(closestNode, isValueMode))
        {
            Debug.LogWarning("Node can't handle this wire type");
            return;
        }

        if (IsPortOccupied(closestNode, isValueMode, firstNode == null)) return;

        if (firstNode == null)
        {
            Wire template = isValueMode ? valueTemplate : flowTemplate;
            if (template == null) return;

            firstNode = closestNode;
            activeWire = Instantiate(template);
            activeWire.gameObject.SetActive(true);
            activeWire.transform.position = new Vector3(activeWire.transform.position.x, activeWire.transform.position.y, 0f);
            activeWire.Source = firstNode;
        }
        else
        {
            if (closestNode == firstNode) return;
            activeWire.Target = closestNode;
            firstNode = null;
            activeWire = null;
        }
    }

    private void TryDeleteNearestWire()
    {
        Wire[] allWires = Object.FindObjectsByType<Wire>(FindObjectsSortMode.None);
        Wire targetWire = null;
        float minDistance = interactDistance;

        foreach (var w in allWires)
        {
            float dist = 999f;
            if (w.Source != null) dist = Mathf.Min(dist, Vector2.Distance(transform.position, w.Source.transform.position));
            if (w.Target != null) dist = Mathf.Min(dist, Vector2.Distance(transform.position, w.Target.transform.position));

            if (dist < minDistance) { 
                minDistance = dist; targetWire = w; 
            }
        }

        if (targetWire != null) Destroy(targetWire.gameObject);
    }

    private void CancelCurrentPlacement()
    {
        if (activeWire != null) Destroy(activeWire.gameObject);
        firstNode = null;
        activeWire = null;
    }

    private bool IsPortOccupied(Node node, bool isValueWire, bool checkingSource)
    {
        Wire[] existingWires = Object.FindObjectsByType<Wire>(FindObjectsSortMode.None);
        WireKind targetKind = isValueWire ? WireKind.Value : WireKind.Flow;
        foreach (var w in existingWires)
        {
            if (w.Kind != targetKind) continue;
            if (checkingSource && w.Source == node) return true;
            if (!checkingSource && w.Target == node) return true;
        }
        return false;
    }

    private bool CanNodeHandleWire(Node node, bool isValueWire)
    {
        if (isValueWire)
            return node.GetOutputAnchor(Node.ValuePort) != null || node.GetInputAnchor(Node.ValuePort) != null;

        return node.GetOutputAnchor(Node.FlowOutPort) != null || node.GetInputAnchor(Node.FlowInPort) != null;
    }

    private Node GetClosestNode()
    {
        Node[] allNodes = Object.FindObjectsByType<Node>(FindObjectsSortMode.None);
        Node closest = null;
        float minDistance = interactDistance;

        foreach (Node n in allNodes)
        {
            float dist = Vector2.Distance(transform.position, n.transform.position);
            if (dist < minDistance) { minDistance = dist; closest = n; }
        }
        return closest;
    }
}