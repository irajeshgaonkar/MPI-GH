using HCA.Core.Services;
using Xunit;

namespace HCA.Api.Tests;

public class IdentityLinkIdSelectorTests
{
    private const string CallingSourceSystem = "wadshs.verificationhub";

    [Fact]
    public void Select_ReturnsLinkId_WhenSourceIsTheCallingSourceSystem()
    {
        var content = QueryResult("own-link", CallingSourceSystem);

        var linkId = IdentityLinkIdSelector.Select(content, CallingSourceSystem, []);

        Assert.Equal("own-link", linkId);
    }

    [Fact]
    public void Select_ReturnsLinkId_WhenSourceIsAnActiveLinkIdShare()
    {
        var content = QueryResult("shared-link", "providerone");

        var linkId = IdentityLinkIdSelector.Select(content, CallingSourceSystem, ["providerone"]);

        Assert.Equal("shared-link", linkId);
    }

    [Fact]
    public void Select_ReturnsNull_WhenShareIsNotInTheLinkIdList()
    {
        var content = QueryResult("hidden-link", "providerone");

        var linkId = IdentityLinkIdSelector.Select(content, CallingSourceSystem, []);

        Assert.Null(linkId);
    }

    [Fact]
    public void Select_ReturnsNull_WhenCallingSourceSystemIsOnlyTheParentPrefix()
    {
        var content = QueryResult("child-link", "wadshs.verificationhub");

        var linkId = IdentityLinkIdSelector.Select(content, "wadshs", ["providerone"]);

        Assert.Null(linkId);
    }

    [Fact]
    public void Select_ReturnsNull_WhenNothingMatches()
    {
        var content = QueryResult("other-link", "aces");

        var linkId = IdentityLinkIdSelector.Select(content, CallingSourceSystem, ["providerone"]);

        Assert.Null(linkId);
    }

    [Fact]
    public void Select_ReturnsLinkId_WhenAnyGroupedSourceIsVisible()
    {
        var content = """
            {
              "linkId": "same-link",
              "identityGroupedBySource": [
                { "source": { "name": "esd" } },
                { "source": { "name": "providerone" } }
              ]
            }
            """;

        var linkId = IdentityLinkIdSelector.Select(content, CallingSourceSystem, ["providerone"]);

        Assert.Equal("same-link", linkId);
    }

    [Fact]
    public void Select_ReturnsNull_WhenLinkIdIsMissing()
    {
        var content = """
            {
              "identityGroupedBySource": [{ "source": { "name": "wadshs.verificationhub" } }]
            }
            """;

        var linkId = IdentityLinkIdSelector.Select(content, CallingSourceSystem, []);

        Assert.Null(linkId);
    }

    [Fact]
    public void Select_ReturnsNull_WhenContentIsEmpty()
    {
        var linkId = IdentityLinkIdSelector.Select("{}", CallingSourceSystem, []);

        Assert.Null(linkId);
    }

    private static string QueryResult(string linkId, string sourceName) =>
        $$"""
        {
          "linkId": "{{linkId}}",
          "identityGroupedBySource": [{ "source": { "name": "{{sourceName}}" } }]
        }
        """;
}
