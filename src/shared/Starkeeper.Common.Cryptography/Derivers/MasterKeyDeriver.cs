using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;

namespace Starkeeper.Common.Cryptography.Derivers;

public sealed class MasterKeyDeriver
{
    public const int MasterKeyLength = 32;

    private readonly int _degreeOfParallelism = 4;
    private readonly int _iterations = 3;
    private readonly int _memorySize = 65536;

    public async Task<byte[]> DeriveMasterKey(string password, string emailSalt)
    {
        ArgumentNullException.ThrowIfNull(password);
        ArgumentNullException.ThrowIfNull(emailSalt);

        byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
        byte[] saltBytes = Encoding.UTF8.GetBytes(emailSalt.ToLowerInvariant());

        try
        {
            using Argon2id argon2Id = new(passwordBytes)
            {
                Iterations = _iterations,
                MemorySize = _memorySize,
                DegreeOfParallelism = _degreeOfParallelism,
                Salt = saltBytes
            };

            return await argon2Id.GetBytesAsync(MasterKeyLength);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(passwordBytes);
            CryptographicOperations.ZeroMemory(saltBytes);
        }
    }
}
