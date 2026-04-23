using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public enum WireKind
{
    Flow,
    Value
}

[AddComponentMenu("")]
[ExecuteAlways]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class Wire : MonoBehaviour
{
    [SerializeField] private Node source;
    [SerializeField, HideInInspector] private WireKind kind;
    [SerializeField, HideInInspector] private string sourcePort;
    [SerializeField] private Node target;
    [SerializeField, HideInInspector] private string targetPort;
    [SerializeField] private float width = 0.18f;

    private static Material wireMaterial;
    private static readonly Color FlowIdleColor = new(1f, 0.85f, 0.35f, 1f);
    private static readonly Color FlowActiveColor = new(1f, 0.95f, 0.65f, 1f);
    private static readonly Color ValueIdleColor = new(0.65f, 0.8f, 1f, 1f);
    private static readonly Color ValueActiveColor = new(0.85f, 0.95f, 1f, 1f);

    private const float CurveStrength = 0.35f;
    private const float MinHandleLength = 0.5f;
    private const float Flatness = 0.08f;
    private const int MaxSubdivisions = 6;
    private const float MaxJoinScale = 2f;
    private const int SortingOrderOffset = -1;
    private const float PulseDecay = 6f;
    private const float PulseWidthScale = 1.35f;

    private readonly List<Vector3> curvePoints = new();

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private Mesh wireMesh;
    private Node registeredFlowSource;
    private Node registeredValueTarget;
    private float pulse;

    public WireKind Kind => GetWireKind();

    public Node Source
    {
        get => source;
        set
        {
            source = value;
            RefreshRegistration();
            RebuildMesh();
        }
    }

    public string SourcePort => GetSourcePort();

    public Node Target
    {
        get => target;
        set
        {
            target = value;
            RefreshRegistration();
            RebuildMesh();
        }
    }

    public string TargetPort => GetTargetPort();

    protected virtual WireKind GetWireKind()
    {
        return kind;
    }

    protected virtual string GetSourcePort()
    {
        return Node.NormalizePort(sourcePort, GetWireKind() == WireKind.Value ? Node.ValuePort : Node.FlowOutPort);
    }

    protected virtual string GetTargetPort()
    {
        return Node.NormalizePort(targetPort, GetWireKind() == WireKind.Value ? Node.ValuePort : Node.FlowInPort);
    }

    protected virtual Color GetIdleColor()
    {
        return GetWireKind() == WireKind.Value ? ValueIdleColor : FlowIdleColor;
    }

    protected virtual Color GetActiveColor()
    {
        return GetWireKind() == WireKind.Value ? ValueActiveColor : FlowActiveColor;
    }

    private void Awake()
    {
        EnsureSetup();
    }

    private void OnEnable()
    {
        EnsureSetup();
        RefreshRegistration();
        RebuildMesh();
    }

    private void OnDisable()
    {
        ClearRegistration();
    }

    private void OnDestroy()
    {
        if (wireMesh == null)
        {
            return;
        }

        if (Application.isPlaying)
        {
            Destroy(wireMesh);
        }
        else
        {
            DestroyImmediate(wireMesh);
        }
    }

    private void OnValidate()
    {
        EnsureSetup();
        RefreshRegistration();
        RebuildMesh();
    }

    private void LateUpdate()
    {
        if (pulse > 0f)
        {
            var deltaTime = Application.isPlaying ? Time.deltaTime : 0.02f;
            pulse = Mathf.MoveTowards(pulse, 0f, PulseDecay * deltaTime);
        }

        RebuildMesh();
    }

    public bool IsFlowOutputFrom(Node node, string outputPort)
    {
        return GetWireKind() == WireKind.Flow &&
               source == node &&
               Node.PortMatches(SourcePort, outputPort, Node.FlowOutPort, Node.FlowOutPort);
    }

    public bool IsValueInputFor(Node node, string inputPort)
    {
        return GetWireKind() == WireKind.Value &&
               target == node &&
               Node.PortMatches(TargetPort, inputPort, Node.ValuePort, Node.ValuePort);
    }

    public void TransmitFlow(Node sender, NodeFlowContext context)
    {
        if (GetWireKind() != WireKind.Flow)
        {
            return;
        }

        if (source != null && sender != null && sender != source)
        {
            return;
        }

        pulse = 1f;
        RebuildMesh();

        if (Application.isPlaying && target != null)
        {
            target.Execute(TargetPort, context);
        }
    }

    public NodeValue ReadValue(Node requester, NodeFlowContext context)
    {
        if (GetWireKind() != WireKind.Value)
        {
            return NodeValue.Flow;
        }

        if (target != null && requester != null && requester != target)
        {
            return NodeValue.Flow;
        }

        pulse = 1f;
        RebuildMesh();
        return source != null ? source.Evaluate(SourcePort, context) : NodeValue.Flow;
    }

    public void RefreshRegistration()
    {
        ClearRegistration();

        if (GetWireKind() == WireKind.Flow && source != null)
        {
            source.RegisterOutgoingFlow(this);
            registeredFlowSource = source;
        }

        if (GetWireKind() == WireKind.Value && target != null)
        {
            target.RegisterIncomingValue(this);
            registeredValueTarget = target;
        }
    }

    private void ClearRegistration()
    {
        if (registeredFlowSource != null)
        {
            registeredFlowSource.UnregisterOutgoingFlow(this);
            registeredFlowSource = null;
        }

        if (registeredValueTarget != null)
        {
            registeredValueTarget.UnregisterIncomingValue(this);
            registeredValueTarget = null;
        }
    }

    private void EnsureSetup()
    {
        meshFilter ??= GetComponent<MeshFilter>();
        meshRenderer ??= GetComponent<MeshRenderer>();

        if (wireMesh == null)
        {
            wireMesh = new Mesh
            {
                name = "Wire Mesh",
                hideFlags = HideFlags.HideAndDontSave
            };
            wireMesh.MarkDynamic();
        }

        if (meshFilter.sharedMesh != wireMesh)
        {
            meshFilter.sharedMesh = wireMesh;
        }

        var material = GetWireMaterial();

        if (material != null && meshRenderer.sharedMaterial != material)
        {
            meshRenderer.sharedMaterial = material;
        }

        meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
        meshRenderer.receiveShadows = false;
        meshRenderer.lightProbeUsage = LightProbeUsage.Off;
        meshRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
        meshRenderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
        meshRenderer.allowOcclusionWhenDynamic = false;

        if (TryGetComponent<SpriteRenderer>(out var spriteRenderer))
        {
            spriteRenderer.enabled = false;
        }
    }

    private void RebuildMesh()
    {
        EnsureSetup();
        wireMesh.Clear();

        if (source == null || target == null)
        {
            meshRenderer.enabled = false;
            return;
        }

        var startPoint = source.GetOutputAnchor(SourcePort);
        var endPoint = target.GetInputAnchor(TargetPort);

        if (startPoint == null || endPoint == null)
        {
            meshRenderer.enabled = false;
            return;
        }

        var start = startPoint.position;
        var end = endPoint.position;
        var distance = Vector2.Distance(start, end);

        if (distance <= 0.001f)
        {
            meshRenderer.enabled = false;
            return;
        }

        var handleLength = Mathf.Max(MinHandleLength, distance * CurveStrength);
        var controlA = start + GetOutDirection(startPoint) * handleLength;
        var controlB = end + GetInDirection(endPoint) * handleLength;

        curvePoints.Clear();
        curvePoints.Add(start);
        SubdivideCurve(start, controlA, controlB, end, 0);

        if (curvePoints.Count < 2)
        {
            meshRenderer.enabled = false;
            return;
        }

        BuildRibbonMesh(curvePoints);
        UpdateSorting();
        meshRenderer.enabled = true;
    }

    private void SubdivideCurve(Vector3 a, Vector3 b, Vector3 c, Vector3 d, int depth)
    {
        if (depth >= Mathf.Max(1, MaxSubdivisions) || IsFlatEnough(a, b, c, d))
        {
            curvePoints.Add(d);
            return;
        }

        var ab = (a + b) * 0.5f;
        var bc = (b + c) * 0.5f;
        var cd = (c + d) * 0.5f;
        var abc = (ab + bc) * 0.5f;
        var bcd = (bc + cd) * 0.5f;
        var mid = (abc + bcd) * 0.5f;

        SubdivideCurve(a, ab, abc, mid, depth + 1);
        SubdivideCurve(mid, bcd, cd, d, depth + 1);
    }

    private bool IsFlatEnough(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
    {
        var threshold = Flatness * Flatness;
        return DistanceToLineSqr(b, a, d) <= threshold && DistanceToLineSqr(c, a, d) <= threshold;
    }

    private static float DistanceToLineSqr(Vector3 point, Vector3 a, Vector3 b)
    {
        var line = b - a;

        if (line.sqrMagnitude <= 0.000001f)
        {
            return (point - a).sqrMagnitude;
        }

        var projection = Vector3.Dot(point - a, line) / line.sqrMagnitude;
        var closest = a + line * Mathf.Clamp01(projection);
        return (point - closest).sqrMagnitude;
    }

    private void BuildRibbonMesh(List<Vector3> points)
    {
        var vertexCount = points.Count * 2;
        var vertices = new Vector3[vertexCount];
        var triangles = new int[(points.Count - 1) * 6];
        var colors = new Color[vertexCount];
        var pulseStrength = Mathf.Clamp01(pulse);
        var color = Color.Lerp(GetIdleColor(), GetActiveColor(), pulseStrength);
        var halfWidth = width * Mathf.Lerp(1f, PulseWidthScale, pulseStrength) * 0.5f;
        var worldToLocal = transform.worldToLocalMatrix;

        for (var i = 0; i < points.Count; i++)
        {
            var offset = CalculateOffset(points, i, halfWidth);
            var vertexIndex = i * 2;

            vertices[vertexIndex] = worldToLocal.MultiplyPoint3x4(points[i] - offset);
            vertices[vertexIndex + 1] = worldToLocal.MultiplyPoint3x4(points[i] + offset);
            colors[vertexIndex] = color;
            colors[vertexIndex + 1] = color;
        }

        for (var i = 0; i < points.Count - 1; i++)
        {
            var vertexIndex = i * 2;
            var triangleIndex = i * 6;

            triangles[triangleIndex] = vertexIndex;
            triangles[triangleIndex + 1] = vertexIndex + 1;
            triangles[triangleIndex + 2] = vertexIndex + 2;
            triangles[triangleIndex + 3] = vertexIndex + 1;
            triangles[triangleIndex + 4] = vertexIndex + 3;
            triangles[triangleIndex + 5] = vertexIndex + 2;
        }

        wireMesh.vertices = vertices;
        wireMesh.triangles = triangles;
        wireMesh.colors = colors;
        wireMesh.RecalculateBounds();
    }

    private void UpdateSorting()
    {
        var sourceRenderer = source != null ? source.GetComponent<Renderer>() : null;
        var targetRenderer = target != null ? target.GetComponent<Renderer>() : null;

        if (sourceRenderer == null && targetRenderer == null)
        {
            meshRenderer.sortingOrder = SortingOrderOffset;
            return;
        }

        var referenceRenderer = sourceRenderer != null ? sourceRenderer : targetRenderer;
        var referenceOrder = referenceRenderer.sortingOrder;

        if (targetRenderer != null && targetRenderer.sortingLayerID == referenceRenderer.sortingLayerID)
        {
            referenceOrder = Mathf.Min(referenceOrder, targetRenderer.sortingOrder);
        }

        meshRenderer.sortingLayerID = referenceRenderer.sortingLayerID;
        meshRenderer.sortingOrder = referenceOrder + SortingOrderOffset;
    }

    private Vector3 CalculateOffset(IReadOnlyList<Vector3> points, int index, float halfWidth)
    {
        var previousDirection = GetSegmentDirection(points, Mathf.Max(0, index - 1), index);
        var nextDirection = GetSegmentDirection(points, index, Mathf.Min(points.Count - 1, index + 1));

        if (index == 0)
        {
            return GetPerpendicular(nextDirection) * halfWidth;
        }

        if (index == points.Count - 1)
        {
            return GetPerpendicular(previousDirection) * halfWidth;
        }

        var tangent = (previousDirection + nextDirection).normalized;

        if (tangent.sqrMagnitude <= 0.000001f)
        {
            return GetPerpendicular(nextDirection) * halfWidth;
        }

        var miter = GetPerpendicular(tangent);
        var normal = GetPerpendicular(previousDirection);
        var dot = Vector3.Dot(miter, normal);

        if (Mathf.Abs(dot) <= 0.0001f)
        {
            return normal * halfWidth;
        }

        var length = Mathf.Clamp(halfWidth / dot, -halfWidth * MaxJoinScale, halfWidth * MaxJoinScale);
        return miter * length;
    }

    private static Vector3 GetSegmentDirection(IReadOnlyList<Vector3> points, int from, int to)
    {
        var direction = points[to] - points[from];
        direction.z = 0f;

        if (direction.sqrMagnitude <= 0.000001f)
        {
            return Vector3.right;
        }

        return direction.normalized;
    }

    private static Vector3 GetPerpendicular(Vector3 direction)
    {
        return new Vector3(-direction.y, direction.x, 0f).normalized;
    }

    private static Vector3 GetOutDirection(Transform point)
    {
        var direction = point != null ? point.right : Vector3.right;
        direction.z = 0f;
        return direction.sqrMagnitude <= 0.000001f ? Vector3.right : direction.normalized;
    }

    private static Vector3 GetInDirection(Transform point)
    {
        var direction = point != null ? -point.right : Vector3.left;
        direction.z = 0f;
        return direction.sqrMagnitude <= 0.000001f ? Vector3.left : direction.normalized;
    }

    private static Material GetWireMaterial()
    {
        if (wireMaterial != null)
        {
            return wireMaterial;
        }

        var shader = Shader.Find("Sprites/Default");

        if (shader == null)
        {
            shader = Shader.Find("Hidden/Internal-Colored");
        }

        if (shader == null)
        {
            return null;
        }

        wireMaterial = new Material(shader)
        {
            hideFlags = HideFlags.HideAndDontSave,
            renderQueue = (int)RenderQueue.Transparent
        };

        if (wireMaterial.HasProperty("_SrcBlend"))
        {
            wireMaterial.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
        }

        if (wireMaterial.HasProperty("_DstBlend"))
        {
            wireMaterial.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
        }

        if (wireMaterial.HasProperty("_Cull"))
        {
            wireMaterial.SetInt("_Cull", (int)CullMode.Off);
        }

        if (wireMaterial.HasProperty("_ZWrite"))
        {
            wireMaterial.SetInt("_ZWrite", 0);
        }

        if (wireMaterial.HasProperty("_ZTest"))
        {
            wireMaterial.SetInt("_ZTest", (int)CompareFunction.LessEqual);
        }

        return wireMaterial;
    }
}
