using System;
using System.Collections.Generic;

namespace SolidCqrsFramework.Key_provision
{
    public static class NextKey
    {
        private static readonly IDictionary<Type, object> KeyProviders;

        static NextKey()
        {
            KeyProviders = new Dictionary<Type, object>();
            RegisterKeyProvider(new GuidKeyProvider());
        }

        public static TKey Get<TKey>(string forTypeName) where TKey : IEquatable<TKey>
        {
            if (!KeyProviders.ContainsKey(typeof(TKey)))
                throw new Exception();

            return ((IProvideKeys<TKey>)KeyProviders[typeof(TKey)]).Get(forTypeName);
        }

        public static void RegisterKeyProvider<TKey>(IProvideKeys<TKey> provider) where TKey : IEquatable<TKey>
        {
            if (!KeyProviders.ContainsKey(typeof(TKey)))
                KeyProviders.Add(typeof(TKey), provider);
        }
    }
}
