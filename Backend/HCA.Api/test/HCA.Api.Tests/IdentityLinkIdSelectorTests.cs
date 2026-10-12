using HCA.Core.Services;
using Xunit;

namespace HCA.Api.Tests;

public class IdentityLinkIdSelectorTests
{
    private const string Caller = "wadshs.verificationhub";

    [Fact]
    public void Select_ReturnsLinkId_WhenSourceIsTheCaller()
    {
        var content = SearchResult("own-link", Caller);

        var linkId = IdentityLinkIdSelector.Select(content, Caller, []);

        Assert.Equal("own-link", linkId);
    }

    [Fact]
    public void Select_ReturnsLinkId_WhenSourceIsAnActiveLinkIdShare()
    {
        var content = SearchResult("shared-link", "providerone");

        var linkId = IdentityLinkIdSelector.Select(content, Caller, ["providerone"]);

        Assert.Equal("shared-link", linkId);
    }

    [Fact]
    public void Select_ReturnsNull_WhenShareIsNotInTheLinkIdList()
    {
        var content = SearchResult("hidden-link", "providerone");

        var linkId = IdentityLinkIdSelector.Select(content, Caller, []);

        Assert.Null(linkId);
    }

    [Fact]
    public void Select_ReturnsNull_WhenCallerIsOnlyTheParentPrefix()
    {
        var content = SearchResult("child-link", "wadshs.verificationhub");

        var linkId = IdentityLinkIdSelector.Select(content, "wadshs", ["providerone"]);

        Assert.Null(linkId);
    }

    [Fact]
    public void Select_ReturnsNull_WhenNothingMatches()
    {
        var content = SearchResult("other-link", "aces");

        var linkId = IdentityLinkIdSelector.Select(content, Caller, ["providerone"]);

        Assert.Null(linkId);
    }

    [Fact]
    public void Select_ReturnsNull_WhenTwoDifferentLinkIdsAreVisible()
    {
        var content = """
            {
              "searchResults": [
                {
                  "linkId": "first-link",
                  "identityGroupedBySource": [{ "source": { "name": "wadshs.verificationhub" } }]
                },
                {
                  "linkId": "second-link",
                  "identityGroupedBySource": [{ "source": { "name": "providerone" } }]
                }
              ]
            }
            """;

        var linkId = IdentityLinkIdSelector.Select(content, Caller, ["providerone"]);

        Assert.Null(linkId);
    }

    [Fact]
    public void Select_ReturnsLinkId_WhenTheSameLinkIdIsVisibleTwice()
    {
        var content = """
            {
              "searchResults": [
                {
                  "linkId": "same-link",
                  "identityGroupedBySource": [{ "source": { "name": "wadshs.verificationhub" } }]
                },
                {
                  "linkId": "same-link",
                  "identityGroupedBySource": [{ "source": { "name": "providerone" } }]
                }
              ]
            }
            """;

        var linkId = IdentityLinkIdSelector.Select(content, Caller, ["providerone"]);

        Assert.Equal("same-link", linkId);
    }

    private static string SearchResult(string linkId, string sourceName) =>
        $$"""
        {
          "searchResults": [
            {
              "linkId": "{{linkId}}",
              "identityGroupedBySource": [{ "source": { "name": "{{sourceName}}" } }]
            }
          ]
        }
        """;
}
