using System.Security.Cryptography;

namespace Starkeeper.Common.Cryptography.Ciphers;

public sealed class Aes256GcmCipher
{
    private const int KeyLength = 32;
    private const int NonceLength = 12;
    private const int TagLength = 16;
    private const int PayloadOverhead = NonceLength + TagLength;

    public byte[] Encrypt(ReadOnlySpan<byte> plaintext, ReadOnlySpan<byte> encryptionKey)
    {
        ValidateKey(encryptionKey);

        byte[] payload = new byte[PayloadOverhead + plaintext.Length];
        byte[] nonce = RandomNumberGenerator.GetBytes(NonceLength);
        nonce.CopyTo(payload, 0);
        Span<byte> tag = payload.AsSpan(NonceLength, TagLength);
        Span<byte> ciphertext = payload.AsSpan(PayloadOverhead);

        using var aes = new AesGcm(encryptionKey, tagSizeInBytes: TagLength);
        aes.Encrypt(nonce, plaintext, ciphertext, tag);

        return payload;
    }

    public byte[] Decrypt(ReadOnlySpan<byte> payload, ReadOnlySpan<byte> encryptionKey)
    {
        ValidateKey(encryptionKey);

        if (payload.Length < PayloadOverhead)
        {
            throw new CryptographicException("Encrypted payload must contain a nonce and an authentication tag.");
        }

        ReadOnlySpan<byte> nonce = payload[..NonceLength];
        ReadOnlySpan<byte> tag = payload.Slice(NonceLength, TagLength);
        ReadOnlySpan<byte> ciphertext = payload[PayloadOverhead..];
        byte[] plaintext = new byte[ciphertext.Length];

        using var aes = new AesGcm(encryptionKey, tagSizeInBytes: TagLength);
        try
        {
            aes.Decrypt(nonce, ciphertext, tag, plaintext);
        }
        catch
        {
            CryptographicOperations.ZeroMemory(plaintext);
            throw;
        }

        return plaintext;
    }

    private static void ValidateKey(ReadOnlySpan<byte> encryptionKey)
    {
        if (encryptionKey.Length != KeyLength)
        {
            throw new ArgumentException($"Encryption key must be {KeyLength} bytes.", nameof(encryptionKey));
        }
    }
}
