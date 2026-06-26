namespace DotchatServer.src.Application.Interfaces.Security;

/// <summary>
/// Represents a service responsible for hashing input strings, such as passwords, to produce a byte array hash. 
/// This interface abstracts the hashing mechanism, allowing for different implementations (e.g., using different algorithms or libraries) while providing a consistent method for hashing across the application.
/// </summary>
public interface IHashingService
{
    /// <summary>
    /// Hashes the specified input string and returns the resulting byte array.
    /// </summary>
    /// <param name="input">The input string to hash.</param>
    /// <returns>A byte array representing the hashed input.</returns>
    public byte[] Hash(string input);

    /// <summary>
    /// Verifies if the specific string and the hash are equal
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public bool Verify(string input, byte[] hash);
}