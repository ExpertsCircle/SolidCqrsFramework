using System;
using System.Threading.Tasks;

namespace SolidCqrsFramework
{
    public interface IRepository<T> where T : AggregateRoot, new()
    {
        Task Save(AggregateRoot aggregate, int expectedVersion = 0);
        Task<T> GetById(Guid id);
    }
}
