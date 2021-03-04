using System.Reflection;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Microsoft.CSharp.RuntimeBinder;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace HybridBlazor.Server
{
    /// <summary>
    /// We need this to support http connection to gRPC services
    /// </summary>
    public class ClearTextHttpMultiplexingMiddleware
    {
        private readonly ConnectionDelegate next;
        // HTTP/2 prior knowledge-mode connection preface
        private static readonly byte[] http2Preface = {
            0x50, 0x52, 0x49, 0x20, 0x2a,
            0x20, 0x48, 0x54, 0x54, 0x50,
            0x2f, 0x32, 0x2e, 0x30, 0x0d,
            0x0a, 0x0d, 0x0a, 0x53, 0x4d,
            0x0d, 0x0a, 0x0d, 0x0a }; //PRI * HTTP/2.0\r\n\r\nSM\r\n\r\n

        public ClearTextHttpMultiplexingMiddleware(ConnectionDelegate next)
        {
            this.next = next;
        }

        private static async Task<bool> HasHttp2Preface(PipeReader input)
        {
            while (true)
            {
                var result = await input.ReadAsync().ConfigureAwait(false);
                try
                {
                    int pos = 0;
                    foreach (var x in result.Buffer)
                    {
                        for (var i = 0; i < x.Span.Length && pos < http2Preface.Length; i++)
                        {
                            if (http2Preface[pos] != x.Span[i])
                            {
                                return false;
                            }

                            pos++;
                        }

                        if (pos >= http2Preface.Length)
                        {
                            return true;
                        }
                    }

                    if (result.IsCompleted) return false;
                }
                finally
                {
                    input.AdvanceTo(result.Buffer.Start);
                }
            }
        }

        private static void SetProtocols(object target, HttpProtocols protocols)
        {
            var field = target.GetType().GetField("_endpointDefaultProtocols", BindingFlags.Instance | BindingFlags.NonPublic);
            if (field == null)
            {
                // Ignore for https
                return;
            }
            field.SetValue(target, protocols);
        }

        public async Task OnConnectAsync(ConnectionContext context)
        {
            var hasHttp2Preface = await HasHttp2Preface(context.Transport.Input).ConfigureAwait(false);
            SetProtocols(next.Target, hasHttp2Preface ? HttpProtocols.Http2 : HttpProtocols.Http1);
            await next(context).ConfigureAwait(false);
        }
    }
}
