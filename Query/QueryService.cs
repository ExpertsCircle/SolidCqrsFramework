using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace SolidCqrsFramework.Query
{

    public interface IQueryService
    {
        Task<TResult> ExecuteQuery<TResult>(IQuerySpec<TResult> querySpec);
    }

    public class QueryService : IQueryService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<QueryService> _logger;
        private readonly ConcurrentDictionary<Type, Func<IServiceProvider, object>> _dict;

        public QueryService(IServiceProvider serviceProvider, ILogger<QueryService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _dict = new ConcurrentDictionary<Type, Func<IServiceProvider, object>>();
        }

        public async Task<TResult> ExecuteQuery<TResult>(IQuerySpec<TResult> querySpec)
        {
            _logger.LogInformationWithObject("Handling query in QueryService", new { QueryType = querySpec.GetType() });

            object handler = null;
            try
            {
                using var scope = _serviceProvider.CreateScope();
                handler = GetHandler(querySpec, scope.ServiceProvider);
                var result = await (Task<TResult>)((dynamic)handler).Handle((dynamic)querySpec);
                return result;
            }
            catch (Exception e)
            {
                _logger.LogErrorWithObject(e, "Query exception occurred during handling of query",
                    new { QueryType = querySpec.GetType(), HandlerType = handler?.GetType() });

                throw; // Decide if you want to wrap it in a custom exception
            }
        }

        private object GetHandler<TResult>(IQuerySpec<TResult> querySpec, IServiceProvider scopedProvider)
        {
            _logger.LogInformationWithObject("Handlers in dictionary", new { Count = _dict.Keys.Count, Keys = _dict.Keys });

            var queryType = querySpec.GetType();
            var handlerType = typeof(IQueryHandler<,>).MakeGenericType(queryType, typeof(TResult));

            // Cache a factory function that resolves handlers
            var handlerFactory = _dict.GetOrAdd(queryType, type =>
            {
                return provider => provider.GetRequiredService(handlerType);
            });

            return handlerFactory(scopedProvider);
        }
    }
}
