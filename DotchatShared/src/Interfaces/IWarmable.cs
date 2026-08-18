namespace DotchatShared.src.Interfaces;

/// <summary>
/// Defines a contract for services that require a warmup phase to prepare resources or perform initial computations before being fully operational.
/// </summary>
public interface IWarmable
{
    /// <summary>
    /// Performs the warmup process, which may include tasks such as precomputing values, initializing caches, or any other necessary setup to ensure the service is ready for use.
    /// </summary>
    /// <returns></returns>
    Task WarmupAsync();
}