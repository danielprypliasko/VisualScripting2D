using System;
using System.Globalization;
using UnityEngine;

public enum NodeValueKind
{
    Flow,
    Number,
    Bool,
    String
}

[Serializable]
public struct NodeValue
{
    [SerializeField] private NodeValueKind kind;
    [SerializeField] private float number;
    [SerializeField] private bool boolean;
    [SerializeField] private string text;

    public NodeValueKind Kind => kind;
    public bool IsFlow => kind == NodeValueKind.Flow;

    public static NodeValue Flow => new(NodeValueKind.Flow, 0f, false, string.Empty);

    public static NodeValue FromNumber(float value)
    {
        return new NodeValue(NodeValueKind.Number, value, false, string.Empty);
    }

    public static NodeValue FromBool(bool value)
    {
        return new NodeValue(NodeValueKind.Bool, 0f, value, string.Empty);
    }

    public static NodeValue FromString(string value)
    {
        return new NodeValue(NodeValueKind.String, 0f, false, value ?? string.Empty);
    }

    public float AsNumber(float fallback = 0f)
    {
        return kind switch
        {
            NodeValueKind.Number => number,
            NodeValueKind.Bool => boolean ? 1f : 0f,
            NodeValueKind.String => TryParseNumber(text, out var parsed) ? parsed : fallback,
            _ => fallback
        };
    }

    public int AsInt(int fallback = 0)
    {
        return Mathf.RoundToInt(AsNumber(fallback));
    }

    public bool AsBool(bool fallback = false)
    {
        return kind switch
        {
            NodeValueKind.Bool => boolean,
            NodeValueKind.Number => !Mathf.Approximately(number, 0f),
            NodeValueKind.String => ParseStringBool(text),
            NodeValueKind.Flow => true,
            _ => fallback
        };
    }

    public string AsString(string fallback = "")
    {
        return kind switch
        {
            NodeValueKind.Number => number.ToString(),
            NodeValueKind.Bool => boolean.ToString(),
            NodeValueKind.String => text ?? string.Empty,
            NodeValueKind.Flow => fallback,
            _ => fallback
        };
    }

    public bool ValueEquals(NodeValue other)
    {
        if (kind == NodeValueKind.String || other.kind == NodeValueKind.String)
        {
            return string.Equals(AsString(), other.AsString(), StringComparison.Ordinal);
        }

        if (kind == NodeValueKind.Bool || other.kind == NodeValueKind.Bool)
        {
            return AsBool() == other.AsBool();
        }

        if (kind == NodeValueKind.Flow || other.kind == NodeValueKind.Flow)
        {
            return kind == other.kind;
        }

        return Mathf.Approximately(AsNumber(), other.AsNumber());
    }

    public bool TryGetNumber(out float value)
    {
        value = number;
        return kind == NodeValueKind.Number;
    }

    public bool TryGetBool(out bool value)
    {
        value = boolean;
        return kind == NodeValueKind.Bool;
    }

    public bool TryGetString(out string value)
    {
        value = text ?? string.Empty;
        return kind == NodeValueKind.String;
    }

    public override string ToString()
    {
        return kind switch
        {
            NodeValueKind.Number => number.ToString(),
            NodeValueKind.Bool => boolean.ToString(),
            NodeValueKind.String => text ?? string.Empty,
            _ => "Flow"
        };
    }

    private NodeValue(NodeValueKind kind, float number, bool boolean, string text)
    {
        this.kind = kind;
        this.number = number;
        this.boolean = boolean;
        this.text = text;
    }

    private static bool ParseStringBool(string value)
    {
        var trimmed = value?.Trim();

        if (string.IsNullOrEmpty(trimmed))
        {
            return false;
        }

        if (bool.TryParse(trimmed, out var parsedBool))
        {
            return parsedBool;
        }

        switch (trimmed.ToLowerInvariant())
        {
            case "yes":
            case "on":
                return true;
            case "no":
            case "off":
                return false;
        }

        if (TryParseNumber(trimmed, out var parsedNumber))
        {
            return !Mathf.Approximately(parsedNumber, 0f);
        }

        return true;
    }

    private static bool TryParseNumber(string value, out float parsed)
    {
        return float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out parsed) ||
               float.TryParse(value, NumberStyles.Float, CultureInfo.CurrentCulture, out parsed);
    }
}
