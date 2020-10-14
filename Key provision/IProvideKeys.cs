using System;

namespace SolidCqrsFramework.Key_provision
{
    public interface IProvideKeys<out TKey> where TKey : IEquatable<TKey>
    {
        TKey Get(string typeName);
    }
}