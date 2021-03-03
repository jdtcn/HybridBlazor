using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Grpc.Core;
using Grpc.Core.Interceptors;

namespace HybridBlazor.Client
{
    public class GrpcClientInterceptor : Interceptor
    {
        private readonly ILogger<GrpcClientInterceptor> logger;

        public GrpcClientInterceptor(ILogger<GrpcClientInterceptor> logger)
        {
            this.logger = logger;
        }

        public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(
            TRequest request,
            ClientInterceptorContext<TRequest, TResponse> context,
            AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            LogCall(context.Method);

            var call = continuation(request, context);

            return new AsyncUnaryCall<TResponse>(HandleResponse(call.ResponseAsync), call.ResponseHeadersAsync, call.GetStatus, call.GetTrailers, call.Dispose);
        }


        public override AsyncClientStreamingCall<TRequest, TResponse> AsyncClientStreamingCall<TRequest, TResponse>(
            ClientInterceptorContext<TRequest, TResponse> context,
            AsyncClientStreamingCallContinuation<TRequest, TResponse> continuation)
        {
            LogCall(context.Method);

            var call = continuation(context);

            return new AsyncClientStreamingCall<TRequest, TResponse>(call.RequestStream, call.ResponseAsync, call.ResponseHeadersAsync, call.GetStatus, call.GetTrailers, call.Dispose);
        }

        public override AsyncServerStreamingCall<TResponse> AsyncServerStreamingCall<TRequest, TResponse>(
            TRequest request,
            ClientInterceptorContext<TRequest, TResponse> context,
            AsyncServerStreamingCallContinuation<TRequest, TResponse> continuation)
        {
            LogCall(context.Method);

            var call = continuation(request, context);

            return new AsyncServerStreamingCall<TResponse>(call.ResponseStream, call.ResponseHeadersAsync, call.GetStatus, call.GetTrailers, call.Dispose);
        }

        public override AsyncDuplexStreamingCall<TRequest, TResponse> AsyncDuplexStreamingCall<TRequest, TResponse>(
            ClientInterceptorContext<TRequest, TResponse> context,
            AsyncDuplexStreamingCallContinuation<TRequest, TResponse> continuation)
        {
            LogCall(context.Method);

            var call = continuation(context);

            return new AsyncDuplexStreamingCall<TRequest, TResponse>(call.RequestStream, call.ResponseStream, call.ResponseHeadersAsync, call.GetStatus, call.GetTrailers, call.Dispose);
        }

        private async Task<TResponse> HandleResponse<TResponse>(Task<TResponse> t)
        {
            try
            {
                var response = await t;
                logger.LogDebug($"Response received: {response}");
                return response;
            }
            catch (RpcException ex)
            {
                logger.LogError($"Call error: {ex.Message}");
                
                // A nice place to catch all exceptions in order to show some popup notification

                throw;
            }
        }

        private void LogCall<TRequest, TResponse>(Method<TRequest, TResponse> method) where TRequest : class where TResponse : class
        {
            logger.LogDebug($"Starting call. Type: {method.Type}. Request: {typeof(TRequest)}. Response: {typeof(TResponse)}");
        }
    }
}