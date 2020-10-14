using System.Threading.Tasks;

namespace SolidCqrsFramework.Query
{
    public interface IQueryHandler<in TQuery, TResult> where TQuery : IQuerySpec<TResult>
    {
        Task<TResult> Handle(TQuery query);
    }
}
