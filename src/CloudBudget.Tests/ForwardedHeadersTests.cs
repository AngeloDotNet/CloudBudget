using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace CloudBudget.Tests;

public class ForwardedHeadersTests
{
    [Fact]
    public async Task When_XForwardedFor_IsPresent_And_KnownProxy_IsLoopback_RemoteIpIsForwardedAsync()
    {
        // Build a minimal test host that uses UseForwardedHeaders and returns remote IP
        var builder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddRouting();
            })
            .Configure(app =>
            {
                // configure KnownProxies = loopback
                var options = new ForwardedHeadersOptions
                {
                    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
                };

                options.KnownProxies.Clear();
                options.KnownProxies.Add(IPAddress.Loopback); // 127.0.0.1

                app.UseForwardedHeaders(options);
                app.UseRouting();

                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapGet("/test/ip", async context =>
                    {
                        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "";
                        await context.Response.WriteAsJsonAsync(new { RemoteIp = ip });
                    });
                });
            });

        using var server = new TestServer(builder);
        using var client = server.CreateClient();

        // Simulate nginx (proxy) by sending X-Forwarded-For header from loopback client
        var request = new HttpRequestMessage(HttpMethod.Get, "/test/ip");
        request.Headers.Add("X-Forwarded-For", "203.0.113.45");

        var resp = await client.SendAsync(request, TestContext.Current.CancellationToken);
        resp.EnsureSuccessStatusCode();

        var json = await resp.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var doc = JsonDocument.Parse(json);
        var remoteIp = doc.RootElement.GetProperty("RemoteIp").GetString();

        Assert.Equal("203.0.113.45", remoteIp);
    }
}