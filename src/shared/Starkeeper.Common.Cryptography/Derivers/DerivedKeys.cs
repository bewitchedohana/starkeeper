using System.Security.Cryptography;

namespace Starkeeper.Common.Cryptography.Derivers;

public sealed class DerivedKeys : IDisposable
{
    private readonly byte[] _encryptionKey;
    private readonly byte[] _clientAuthHash;
    private bool _disposed;

    internal DerivedKeys(byte[] encryptionKey, byte[] clientAuthHash)
    {
        ArgumentNullException.ThrowIfNull(encryptionKey);
        ArgumentNullException.ThrowIfNull(clientAuthHash);

        if (encryptionKey.Length != MasterKeyDeriver.MasterKeyLength || clientAuthHash.Length != MasterKeyDeriver.MasterKeyLength)
        {
            throw new ArgumentException($"Derived keys must be {MasterKeyDeriver.MasterKeyLength} bytes.");
        }

        _encryptionKey = encryptionKey;
        _clientAuthHash = clientAuthHash;
    }

    public ReadOnlyMemory<byte> EncryptionKey
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            return _encryptionKey;
        }
    }

    public ReadOnlyMemory<byte> ClientAuthHash
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            return _clientAuthHash;
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        CryptographicOperations.ZeroMemory(_encryptionKey);
        CryptographicOperations.ZeroMemory(_clientAuthHash);
        _disposed = true;

        GC.SuppressFinalize(this);
    }
}
