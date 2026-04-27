using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderFlow.Application.Behaviors
{
    public class PerformanceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest
    {
        private readonly Stopwatch _timer;
        private readonly ILogger<TRequest> _logger;

        public PerformanceBehavior(ILogger<TRequest> logger)
        {
            _logger = logger;
            _timer = new Stopwatch();
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            _timer.Start();

            var response = await next();

            var elapsedMilliseconds = _timer.ElapsedMilliseconds;
         
            if (elapsedMilliseconds > 500)
            {
                var requestName = typeof(TRequest).Name;

                _logger.LogWarning(
                    "OrderFlow Long Running Request: {Name} ({ElapsedMilliseconds} milliseconds) {@Request}",
                    requestName, elapsedMilliseconds, request);
            }
            return response;
        }
    }
}