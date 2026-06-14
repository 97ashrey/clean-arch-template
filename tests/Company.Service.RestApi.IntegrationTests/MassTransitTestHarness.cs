using MassTransit;
using MassTransit.Testing;
using System;

namespace Company.Service.RestApi.IntegrationTests;

public class MassTransitTestHarness : IAsyncLifetime
{
    public IPublishEndpoint PublishEndpoint => _harness.Bus;

    private readonly InMemoryTestHarness _harness;

    public MassTransitTestHarness()
    {
        _harness = new InMemoryTestHarness();
    }


    public async Task InitializeAsync()
    {
        await _harness.Start();
    }

    public async Task DisposeAsync()
    {
        await _harness.Stop();
    }

    public async Task<bool> Published<T>() where T : class
    {
        return await _harness.Published.Any<T>();
    }
}