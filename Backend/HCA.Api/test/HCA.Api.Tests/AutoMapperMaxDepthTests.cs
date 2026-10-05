using AutoMapper;
using AutoMapper.Internal;
using HCA.Core;
using HCA.Data.Entities;
using HCA.Models.Request;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace HCA.Api.Tests;

public class AutoMapperMaxDepthTests : IClassFixture<MapperRegistrationFixture>
{
    private const int MaxDepth = 64;
    private const int ChainLengthBeyondLimit = 200;

    private readonly MapperRegistrationFixture _fixture;

    public AutoMapperMaxDepthTests(MapperRegistrationFixture fixture) => _fixture = fixture;

    [Fact]
    public void Every_registered_map_has_the_depth_limit()
    {
        var configuration = _fixture.Provider.GetRequiredService<AutoMapper.IConfigurationProvider>();
        var typeMaps = configuration.Internal().GetAllTypeMaps().ToArray();

        Assert.Contains(typeMaps, map =>
            map.SourceType == typeof(ClientIdentityRequest) &&
            map.DestinationType == typeof(ClientIdentityRequestEntity));
        Assert.All(typeMaps, map => Assert.Equal(MaxDepth, map.MaxDepth));
    }

    [Fact]
    public void Flat_client_identity_map_still_copies_scalars()
    {
        var mapper = _fixture.Provider.GetRequiredService<IMapper>();

        var mapped = mapper.Map<ClientIdentityRequestEntity>(
            new ClientIdentityRequest { FirstName = "Ada", LastName = "Lovelace" });

        Assert.Equal("Ada", mapped.FirstName);
        Assert.Equal("Lovelace", mapped.LastName);
    }

    [Fact]
    public void Self_referential_chain_stops_at_max_depth()
    {
        var mapper = _fixture.Provider.GetRequiredService<IMapper>();
        var root = new DepthProbeNode();
        var current = root;
        for (var i = 0; i < ChainLengthBeyondLimit; i++)
        {
            current.Child = new DepthProbeNode();
            current = current.Child;
        }

        var mapped = mapper.Map<DepthProbeNode>(root);

        var depth = 0;
        for (var node = mapped; node != null; node = node.Child)
            depth++;

        Assert.Equal(MaxDepth, depth);
    }
}

public sealed class MapperRegistrationFixture : IDisposable
{
    public MapperRegistrationFixture()
    {
        Provider = new ServiceCollection().AddAutoMapper().BuildServiceProvider();
    }

    public ServiceProvider Provider { get; }

    public void Dispose() => Provider.Dispose();
}

public sealed class DepthProbeNode
{
    public DepthProbeNode? Child { get; set; }
}

public sealed class DepthProbeNodeProfile : Profile
{
    public DepthProbeNodeProfile() => CreateMap<DepthProbeNode, DepthProbeNode>();
}
