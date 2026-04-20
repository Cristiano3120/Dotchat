using System.Diagnostics;

using DotchatServer.src.Application.Interfaces;
using DotchatServer.src.Application.Interfaces.Security;

using Isopoh.Cryptography.Argon2;

using Serilog;

namespace DotchatServer.src.Application.Services;

internal sealed class Argon2Hasher : IHashingService, IWarmable
{
    public string Hash(string input)
        => Argon2.Hash
        (
            password: input,
            timeCost: 2, //Number of iterations
            memoryCost: 19456, //19456 KiB = 19 MiB
            parallelism: 1, //Number of threads and compute lanes to use
            type: Argon2Type.HybridAddressing,
            hashLength: 32 //Length of the resulting hash in bytes 
        );

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