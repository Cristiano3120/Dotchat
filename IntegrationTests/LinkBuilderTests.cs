using DotchatServer.src.Application.Services;

public sealed class LinkBuilderTests
{
    private UrlBuilder _sut = UrlBuilder.Create(string.Empty);

    [Fact]
    public void Build_WithOnlyBaseUrl_ReturnsBaseUrl()
    {
        string result = _sut
            .AddUrl("https://example.com")
            .Build();

        Assert.Equal("https://example.com", result);
    }

    [Fact]
    public void Build_WithRouteParam_AppendsToPath()
    {
        string result = _sut
            .AddUrl("https://example.com")
            .AddRouteParam("users")
            .AddRouteParam("42")
            .Build();

        Assert.Equal("https://example.com/users/42", result);
    }

    [Fact]
    public void Build_WithQueryParam_AppendsQueryString()
    {
        string result = _sut
            .AddUrl("https://example.com")
            .AddQueryParam("sort", "asc")
            .Build();

        Assert.Equal("https://example.com?sort=asc", result);
    }

    [Fact]
    public void Build_WithMultipleQueryParams_JoinsWithAmpersand()
    {
        string result = _sut
            .AddUrl("https://example.com")
            .AddQueryParam("sort", "asc")
            .AddQueryParam("page", "2")
            .Build();

        Assert.Equal("https://example.com?sort=asc&page=2", result);
    }

    [Fact]
    public void Build_WithRouteAndQueryParams_CorrectOrder()
    {
        string result = _sut
            .AddUrl("https://example.com")
            .AddRouteParam("users")
            .AddRouteParam("42")
            .AddQueryParam("sort", "asc")
            .Build();

        Assert.Equal("https://example.com/users/42?sort=asc", result);
    }

    [Fact]
    public void Build_WithSpecialCharsInQueryParamValue_EncodesCorrectly()
    {
        string result = _sut
            .AddUrl("https://example.com")
            .AddQueryParam("token", "abc+123/xyz==")
            .Build();

        Assert.Equal("https://example.com?token=abc%2B123%2Fxyz%3D%3D", result);
    }

    [Fact]
    public void Build_WithSpecialCharsInRouteParamValue_EncodesCorrectly()
    {
        string result = _sut
            .AddUrl("https://example.com")
            .AddRouteParam("my path")
            .Build();

        Assert.Equal("https://example.com/my%20path", result);
    }

    [Fact]
    public void Build_WithTrailingSlashInBaseUrl_NoDoubleSlash()
    {
        string result = _sut
            .AddUrl("https://example.com/")
            .AddRouteParam("users")
            .Build();

        Assert.Equal("https://example.com/users", result);
    }

    [Fact]
    public void Build_WithoutUrl_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() =>
            _sut.AddRouteParam("users").Build()
        );
    }

    [Fact]
    public void Build_WithEmptyQueryParamValue_IncludesKey()
    {
        string result = _sut
            .AddUrl("https://example.com")
            .AddQueryParam("flag", "")
            .Build();

        Assert.Equal("https://example.com?flag=", result);
    }

    [Fact]
    public void Build_WithDuplicateQueryParamKey_OverwritesOrAppends()
    {
        string result = _sut
            .AddUrl("https://example.com")
            .AddQueryParam("sort", "asc")
            .AddQueryParam("sort", "desc")
            .Build();

        Assert.Equal("https://example.com?sort=asc&sort=desc", result);
    }

    [Fact]
    public void Build_CalledTwice_ReturnsSameResult()
    {
        _sut = _sut.AddUrl("https://example.com").AddRouteParam("users");

        string first = _sut.Build();
        string second = _sut.Build();

        Assert.Equal(first, second);
    }
}