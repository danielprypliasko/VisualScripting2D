using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct NodeGraphVariable
{
    public string name;
    public NodeValue value;
}

public sealed class NodeFlowContext
{
    private int remainingSteps;

    public NodeGraph Graph { get; }
    public int RemainingSteps => remainingSteps;
    public bool HasSteps => remainingSteps > 0;

    public NodeFlowContext(NodeGraph graph, int maxSteps)
    {
        Graph = graph;
        remainingSteps = Mathf.Max(1, maxSteps);
    }

    public bool TryStep()
    {
        if (remainingSteps <= 0)
        {
            return false;
        }

        remainingSteps--;
        return true;
    }
}

public class NodeGraph : MonoBehaviour
{
    [SerializeField] private int maxStepsPerDispatch = 256;
    [SerializeField] private List<NodeGraphVariable> variables = new();

    private readonly Dictionary<string, int> variableIndices = new(StringComparer.OrdinalIgnoreCase);

    private bool indexDirty = true;

    public int MaxStepsPerDispatch => Mathf.Max(1, maxStepsPerDispatch);

    private void OnEnable()
    {
        indexDirty = true;
    }

    private void OnValidate()
    {
        indexDirty = true;
    }

    public NodeFlowContext CreateContext()
    {
        return new NodeFlowContext(this, MaxStepsPerDispatch);
    }

    public NodeValue Get(string name, NodeValue fallback)
    {
        return TryGet(name, out var value) ? value : fallback;
    }

    public bool TryGet(string name, out NodeValue value)
    {
        value = default;

        if (string.IsNullOrWhiteSpace(name))
        {
            return false;
        }

        EnsureIndex();

        if (!variableIndices.TryGetValue(name, out var index))
        {
            return false;
        }

        value = variables[index].value;
        return true;
    }

    public void Set(string name, NodeValue value)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return;
        }

        EnsureIndex();

        if (variableIndices.TryGetValue(name, out var index))
        {
            var entry = variables[index];
            entry.value = value;
            variables[index] = entry;
            return;
        }

        variables.Add(new NodeGraphVariable
        {
            name = name,
            value = value
        });
        indexDirty = true;
    }

    private void EnsureIndex()
    {
        if (!indexDirty)
        {
            return;
        }

        variableIndices.Clear();

        for (var i = 0; i < variables.Count; i++)
        {
            var name = variables[i].name;

            if (string.IsNullOrWhiteSpace(name))
            {
                continue;
            }

            variableIndices[name] = i;
        }

        indexDirty = false;
    }
}
