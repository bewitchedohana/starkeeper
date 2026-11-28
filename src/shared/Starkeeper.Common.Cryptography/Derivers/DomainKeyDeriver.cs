using System.Security.Cryptography;
using System.Text;

namespace Starkeeper.Common.Cryptography.Derivers;

public sealed class DomainKeyDeriver
{
    private const int DerivedKeyLength = MasterKeyDeriver.MasterKeyLength;
    private static readonly byte[] EncryptionInfo = Encoding.UTF8.GetBytes("starkeeper.v1.encryption");
    private static readonly byte[] ClientAuthInfo = Encoding.UTF8.GetBytes("starkeeper.v1.auth");

    public DerivedKeys Derive(byte[] masterKey)
    {
        ArgumentNullException.ThrowIfNull(masterKey);

        if (masterKey.Length != MasterKeyDeriver.MasterKeyLength)
        {
            throw new ArgumentException($"Master key must be {MasterKeyDeriver.MasterKeyLength} bytes.", nameof(masterKey));
        }

        byte[] encryptionKey = new byte[DerivedKeyLength];
        byte[] clientAuthHash = new byte[DerivedKeyLength];

        try
        {
            HKDF.Expand(HashAlgorithmName.SHA256, masterKey, encryptionKey, info: EncryptionInfo);
            HKDF.Expand(HashAlgorithmName.SHA256, masterKey, clientAuthHash, info: ClientAuthInfo);

            return new DerivedKeys(encryptionKey, clientAuthHash);
        }
        catch
        {
            CryptographicOperations.ZeroMemory(encryptionKey);
            CryptographicOperations.ZeroMemory(clientAuthHash);
            throw;
        }
    }
}
