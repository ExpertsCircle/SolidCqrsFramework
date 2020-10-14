using System;

namespace SolidCqrsFramework.Key_provision
{
    public class GuidKeyProvider : IProvideKeys<Guid>
    {
        public Guid Get(string typeName)
        {
            return Guid.NewGuid();
        }
    }
}