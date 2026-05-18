namespace DotchatServer.src.Application.Interfaces.Security;

public interface IHashingService
{
    public byte[] Hash(string input);
}