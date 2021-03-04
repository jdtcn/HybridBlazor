using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HybridBlazor.Server
{
    public class GrpcExceptionInterceptor : Interceptor
    {
        private readonly ILogger<GrpcExceptionInterceptor> _logger;

        public GrpcExceptionInterceptor(ILogger<GrpcExceptionInterceptor> logger)
        {
            _logger = logger;
        }

        public async override Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
            TRequest request,
            ServerCallContext context,
            UnaryServerMethod<TRequest, TResponse> continuation)
        {
            LogCall(context);
            try
            {
                return await continuation(request, context);
            }
            catch (RpcException)
            {
                throw;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"An error occured when calling {context.Method}");
                throw new RpcException(new Status(StatusCode.Internal, e.Message, e));
            }
        }

        public override async Task ServerStreamingServerHandler<TRequest, TResponse>(
            TRequest request,
            IServerStreamWriter<TResponse> responseStream,
            ServerCallContext context,
            ServerStreamingServerMethod<TRequest, TResponse> continuation)
        {
            try
            {
                await continuation(request, responseStream, context);
            }
            catch (TaskCanceledException)
            {
                // Ignore this exception
            }
        }

        private void LogCall(ServerCallContext context)
        {
            var httpContext = context.GetHttpContext();
            _logger.LogDebug($"Starting call. Request: {httpContext.Request.Path}");
        }
    }
}
