namespace A2A.V0_3Compat.UnitTests;

public class A2AClientFactoryTests : IDisposable
{
    public void Dispose()
    {
        A2AClientFactory.Unregister(ProtocolBindingNames.JsonRpc, "0.3");
        GC.SuppressFinalize(this);
    }

    [Fact]
    public void Create_WithV10Card_ReturnsA2AClient()
    {
        var cardJson = """
        {
            "name": "Modern Agent",
            "description": "A v1.0 agent",
            "supportedInterfaces": [
                {
                    "protocolBinding": "JSONRPC",
                    "protocolVersion": "1.0",
                    "url": "http://localhost/a2a"
                }
            ]
        }
        """;

        var client = A2AClientFactory.Create(cardJson, new Uri("http://localhost"));

        Assert.IsType<A2A.A2AClient>(client);
    }

    [Fact]
    public void Create_WithV03Card_AndRegisteredBinding_ReturnsV03Adapter()
    {
        V03CompatClientFactory.RegisterWithMainFactory();

        var cardJson = """
        {
            "name": "Legacy Agent",
            "description": "A legacy agent",
            "url": "http://localhost/a2a",
            "protocolVersion": "0.3",
            "capabilities": { "streaming": true },
            "skills": []
        }
        """;

        var client = A2AClientFactory.Create(cardJson, new Uri("http://localhost"));

        Assert.IsNotType<A2A.A2AClient>(client);
        Assert.IsAssignableFrom<IA2AClient>(client);
    }

    [Fact]
    public void Create_WithV03Card_NoRegisteredBinding_Throws()
    {
        // Ensure no v0.3 binding is registered
        A2AClientFactory.Unregister(ProtocolBindingNames.JsonRpc, "0.3");

        var cardJson = """
        {
            "name": "Legacy Agent",
            "description": "A legacy agent",
            "url": "http://localhost/a2a",
            "protocolVersion": "0.3",
            "capabilities": {},
            "skills": []
        }
        """;

        Assert.Throws<A2AException>(() =>
            A2AClientFactory.Create(cardJson, new Uri("http://localhost")));
    }

    [Fact]
    public void Create_V03Card_NormalizesVersionWithPatch()
    {
        V03CompatClientFactory.RegisterWithMainFactory();

        // protocolVersion "0.3.0" should normalize to "0.3" and match
        var cardJson = """
        {
            "name": "Legacy Agent",
            "description": "A legacy agent",
            "url": "http://localhost/a2a",
            "protocolVersion": "0.3.0",
            "capabilities": {},
            "skills": []
        }
        """;

        var client = A2AClientFactory.Create(cardJson, new Uri("http://localhost"));

        Assert.IsAssignableFrom<IA2AClient>(client);
        Assert.IsNotType<A2A.A2AClient>(client);
    }

    [Fact]
    public void Unregister_RemovesBinding_ReturnsTrue()
    {
        V03CompatClientFactory.RegisterWithMainFactory();

        var removed = A2AClientFactory.Unregister(ProtocolBindingNames.JsonRpc, "0.3");

        Assert.True(removed);
    }

    [Fact]
    public void Unregister_NonExistentBinding_ReturnsFalse()
    {
        var removed = A2AClientFactory.Unregister("NONEXISTENT", "9.9");

        Assert.False(removed);
    }
}

public class V03CompatClientFactoryTests
{
    [Fact]
    public void Create_WithUrl_ReturnsV03Adapter()
    {
        var client = V03CompatClientFactory.Create(new Uri("http://localhost/a2a"));

        Assert.IsAssignableFrom<IA2AClient>(client);
        Assert.IsNotType<A2A.A2AClient>(client);
    }

    [Fact]
    public void Create_WithJson_UsesUrlFromCard()
    {
        var cardJson = """
        {
            "name": "Legacy Agent",
            "url": "http://agent-host/a2a",
            "protocolVersion": "0.3"
        }
        """;

        var client = V03CompatClientFactory.Create(cardJson, new Uri("http://fallback-host"));

        Assert.IsAssignableFrom<IA2AClient>(client);
        Assert.IsNotType<A2A.A2AClient>(client);
    }

    [Fact]
    public void Create_WithJson_NoUrlInCard_UsesBaseUrl()
    {
        var cardJson = """
        {
            "name": "Legacy Agent",
            "protocolVersion": "0.3"
        }
        """;

        var client = V03CompatClientFactory.Create(cardJson, new Uri("http://fallback-host/a2a"));

        Assert.IsAssignableFrom<IA2AClient>(client);
    }
}
