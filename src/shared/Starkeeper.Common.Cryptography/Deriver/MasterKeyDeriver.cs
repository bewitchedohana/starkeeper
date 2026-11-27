using System.Text;
using Konscious.Security.Cryptography;

namespace Starkeeper.Common.Cryptography.Deriver;

public sealed class MasterKeyDeriver
{
    private readonly int _degreeOfParallelism = 4;
    private readonly int _iterations = 3;
    private readonly int _memorySize = 65536;
    private readonly int _derivationByteSize = 32;

    public async Task<byte[]> DeriveMasterKey(string password, string emailSalt)
    {
        byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
        byte[] saltBytes = Encoding.UTF8.GetBytes(emailSalt.ToLowerInvariant());
        using Argon2id argon2Id = new(passwordBytes)
        {
            Iterations = _iterations,
            MemorySize = _memorySize,
            DegreeOfParallelism = _degreeOfParallelism,
            Salt = saltBytes
        };

        return await argon2Id.GetBytesAsync(_derivationByteSize);
    }
}
