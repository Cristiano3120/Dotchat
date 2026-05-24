namespace DotchatServer.src.Application.Interfaces;

public interface IHtmlRenderable
{
    string HtmlBody { get; init; }
}

public interface IHtmlRenderable<T> : IHtmlRenderable where T : IHtmlRenderable<T>
{
    static abstract implicit operator string(T value);
}