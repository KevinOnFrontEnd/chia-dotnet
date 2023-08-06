﻿using System;
using System.IO;
using System.Threading;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace chia.dotnet.tests.Core;

public class ChiaDotNetFixture : IDisposable
{
    public IHost TestHost { get; }
    internal CancellationTokenSource _cts;
    private string OriginService => "unit_tests";

    public ChiaDotNetFixture()
    {
        try
        {
            _cts = new CancellationTokenSource(55000);
            TestHost = CreateHostBuilder().Build();
            TestHost.Start();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
            Environment.Exit(1);
        }
    }

    public IHostBuilder? CreateHostBuilder()
    {
        return new HostBuilder().ConfigureWebHost(webhost =>
        {
            webhost.ConfigureAppConfiguration((hostContext, configurationBuilder) =>
            {
                configurationBuilder.SetBasePath(Directory.GetCurrentDirectory());
                configurationBuilder.AddJsonFile("testingappsettings.json", false);
                configurationBuilder.AddEnvironmentVariables("PREFIX_");
                configurationBuilder.AddUserSecrets<ChiaDotNetFixture>(true);
            });

            webhost.ConfigureServices((hostContext, services) =>
            {
                try
                {
                    //bind settings from secrets/environment/appsettings
                    var daemonConfig = new Endpoint();
                    var fullNode = new Endpoint();
                    hostContext.Configuration.GetSection("daemon").Bind(daemonConfig);
                    hostContext.Configuration.GetSection("fullnode").Bind(fullNode);

                    // Get all endpoints
                    var daemonEndpointInfo = GetEndpointInfo(daemonConfig);
                    var fullNodeEndpointInfo = GetEndpointInfo(fullNode);

                    var wssClient = new WebSocketRpcClient(daemonEndpointInfo);
                    var cts = new CancellationTokenSource(120000);

                    //
                    wssClient.Connect(cts.Token).GetAwaiter().GetResult();
                    var daemon = new DaemonProxy(wssClient, OriginService);
                    daemon.RegisterService(cts.Token);
                    var nodeRpcClient = new FullNodeProxy(wssClient, OriginService);
                    var farmerRpcProxy = new FarmerProxy(wssClient, OriginService);

                    //register test dependencies 
                    services.AddSingleton<DaemonProxy>(daemon);
                    services.AddSingleton<FullNodeProxy>(nodeRpcClient);
                    services.AddSingleton<WebSocketRpcClient>(wssClient);
                    services.AddSingleton<FarmerProxy>(farmerRpcProxy);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    Environment.Exit(1);
                }
            });

            webhost.UseTestServer();
            webhost.Configure(app =>
            {
                app.Run(async ctx =>
                {
                    await ctx.Response.WriteAsync("TestHost HttpServer Started");
                });
            });
        });


    }

    private EndpointInfo GetEndpointInfo(Endpoint ep) =>
        new EndpointInfo() { KeyPath = ep.KeyPath, CertPath = ep.CertPath, Uri = new Uri(ep.Uri) };
    
    public void Dispose()
    {
        TestHost?.Dispose();
    }
}
