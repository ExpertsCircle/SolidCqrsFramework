using System;
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
        private readonly Dictionary<Type, object> _dict;

        public QueryService(IServiceProvider serviceProvider, ILogger<QueryService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _dict = new Dictionary<Type, object>();
        }

        public async Task<TResult> ExecuteQuery<TResult>(IQuerySpec<TResult> querySpec)
        {
            _logger.LogTraceWithObject("Handling query in QueryService", new { QueryType = querySpec.GetType()});
            object handler = null;
            try
            {
                handler = GetHandler(querySpec);
                var result = await (Task<TResult>)((dynamic)handler).Handle((dynamic)querySpec);
                return result;
            }
            catch (Exception e)
            {
                _logger.LogErrorWithObject(e, "Query exception occurred during handling of query",
                    new { QueryType = querySpec.GetType(), HandlerType = handler?.GetType() });

                throw; //TODO: Decide if throwing is required
            }
        }

        private object GetHandler<TResult>(IQuerySpec<TResult> querySpec)
        {

            _logger.LogTraceWithObject("Handlers in dictionary", new {Count = _dict.Keys.Count, Keys = _dict.Keys});
            
            
            if (_dict.ContainsKey(querySpec.GetType()))
            {
                return _dict[querySpec.GetType()];
            }

            var handlerType = typeof(IQueryHandler<,>).MakeGenericType(querySpec.GetType(), typeof(TResult));
            var handler = _serviceProvider.GetRequiredService(handlerType);

            if (!_dict.ContainsKey(querySpec.GetType()))
            {
                _dict.Add(querySpec.GetType(), handler);
            }

            return handler;
        }
    }
}
