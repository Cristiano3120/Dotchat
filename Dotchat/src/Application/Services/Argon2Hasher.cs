using System.Diagnostics;
using System.Text;
using DotchatServer.src.Application.Interfaces;
using DotchatServer.src.Application.Interfaces.Security;
using Isopoh.Cryptography.Argon2;
using Serilog;

namespace DotchatServer.src.Application.Services;

/// <summary>
/// Implements the Argon2 hashing algorithm for secure password hashing.
/// </summary>
internal sealed class Argon2Hasher : IHashingService, IWarmable
{
    public byte[] Hash(string input)
    {
        string hash = Argon2.Hash
        (
            password: input,
            timeCost: 2, //Number of iterations
            memoryCost: 19456, //19456 KiB = 19 MiB
            parallelism: 1, //Number of threads and compute lanes to use
            type: Argon2Type.HybridAddressing,
            hashLength: 32 //Length of the resulting hash in bytes 
        );

        return Encoding.UTF8.GetBytes(hash);
    }

    public bool Verify(string input, byte[] hash)
    {
        string phcString = Encoding.UTF8.GetString(hash);
        bool result = Argon2.Verify(phcString, input);
        return result;
    }

    public Task WarmupAsync()
    {
        for (int i = 0; i < 10; i++)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            _ = Hash($"password{i}");
            stopwatch.Stop();

            Log.Information("{Algorithm} hash generated in {ElapsedMs}ms", "Argon2", stopwatch.ElapsedMilliseconds);
        }

        return Task.CompletedTask;
    }
}