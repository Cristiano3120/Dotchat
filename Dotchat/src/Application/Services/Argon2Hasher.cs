using DotchatServer.src.Application.Interfaces.Security;
using Isopoh.Cryptography.Argon2;

namespace DotchatServer.src.Application.Services;

internal sealed class Argon2Hasher : IHashingService
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
}