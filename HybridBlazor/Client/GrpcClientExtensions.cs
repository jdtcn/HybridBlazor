using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Grpc.Core.Interceptors;
using ProtoBuf.Grpc.Client;

namespace HybridBlazor.Client
{
    public static class GrpcClientExtensions
    {
        public static IServiceCollection AddGrpcClient<T>(this IServiceCollection services) where T : class =>
            services.AddTransient(sp =>
            {
                var interceptor = sp.GetService<GrpcClientInterceptor>();
                var httpHandler = sp.GetService<HttpClientHandler>();
                var httpClient = sp.GetService<HttpClient>();

                var handler = new Grpc.Net.Client.Web.GrpcWebHandler(
                    Grpc.Net.Client.Web.GrpcWebMode.GrpcWeb,
                    httpHandler ?? new HttpClientHandler());

                var channel = Grpc.Net.Client.GrpcChannel.ForAddress(
                    httpClient.BaseAddress,
                    new Grpc.Net.Client.GrpcChannelOptions()
                    {
                        HttpHandler = handler
                    });

                var invoker = channel.Intercept(interceptor);

                return GrpcClientFactory.CreateGrpcService<T>(invoker);
            });
    }
}
