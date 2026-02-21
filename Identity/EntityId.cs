using System;
using NanoidDotNet;

namespace SolidCqrsFramework.Identity;

public readonly struct EntityId<T>
{
    private const int IdLength = 12;

    private const string Alphabet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    public string Value { get; }

    private EntityId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("ID value cannot be null or empty.", "value");
        }

        Value = value;
    }

    public static EntityId<T> New(string? prefix = null)
    {
        string text = Nanoid.Generate("0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ", 12);
        return new EntityId<T>(string.IsNullOrWhiteSpace(prefix) ? text : (prefix + "-" + text));
    }

    public static EntityId<T> From(string value)
    {
        return new EntityId<T>(value);
    }

    public override string ToString()
    {
        return Value;
    }

    public override bool Equals(object? obj)
    {
        if (obj is EntityId<T> entityId)
        {
            return Value == entityId.Value;
        }

        return false;
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public static implicit operator string(EntityId<T> id)
    {
        return id.Value;
    }

    public static explicit operator EntityId<T>(string value)
    {
        return From(value);
    }
}
