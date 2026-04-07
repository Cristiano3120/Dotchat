namespace DotchatServer.src.Application.Interfaces.Security;

public interface IHashingService
{
    public string Hash(string input);
}