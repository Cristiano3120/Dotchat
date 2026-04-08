using DotchatServer.src.Application.Interfaces;
using DotchatServer.src.Application.Interfaces.Security;
using Isopoh.Cryptography.Argon2;
using System.Diagnostics;

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
            string password = $"password{i}";

            string hash = Hash(password);
            stopwatch.Stop();

            Console.WriteLine($"Password: {password}, Hash: {hash}, Time taken: {stopwatch.ElapsedMilliseconds} ms");
        }

        return Task.CompletedTask;
    }
}