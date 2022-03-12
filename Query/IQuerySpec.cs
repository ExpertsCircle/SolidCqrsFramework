using System.Threading;

namespace SolidCqrsFramework.Query
{
    public interface IQuerySpec<out T>
    {
        public CancellationToken CancellationToken { get; set; }
    }
}
