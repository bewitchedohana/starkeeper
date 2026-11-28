using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Shouldly;
using Starkeeper.Common.Cryptography.Ciphers;

namespace Starkeeper.Common.Cryptography.UnitTests.Ciphers;

public sealed class Aes256GcmCipherTests
{
    private const int KeyLength = 32;
    private const int NonceLength = 12;
    private const int TagLength = 16;
    private const int PayloadOverhead = NonceLength + TagLength;
    private const int PlaintextLength = 4;

    [Fact]
    public void EncryptDecrypt_WithUtf8Text_ReturnsOriginalText()
    {
        // Arrange
        Aes256GcmCipher cipher = new();
        byte[] encryptionKey = CreateKey();
        const string text = "Cofre: café, 日本語, ñ";
        byte[] plaintext = Encoding.UTF8.GetBytes(text);

        // Act
        byte[] payload = cipher.Encrypt(plaintext, encryptionKey);
        byte[] decrypted = cipher.Decrypt(payload, encryptionKey);

        // Assert
        Encoding.UTF8.GetString(decrypted).ShouldBe(text);
    }

    [Fact]
    public void EncryptDecrypt_WithSerializedJson_ReturnsOriginalObject()
    {
        // Arrange
        Aes256GcmCipher cipher = new();
        byte[] encryptionKey = CreateKey();
        VaultItem item = new("Banco", "senha@123", ["financeiro", "pessoal"]);
        byte[] plaintext = JsonSerializer.SerializeToUtf8Bytes(item);

        // Act
        byte[] payload = cipher.Encrypt(plaintext, encryptionKey);
        byte[] decrypted = cipher.Decrypt(payload, encryptionKey);
        VaultItem result = JsonSerializer.Deserialize<VaultItem>(decrypted)!;

        // Assert
        result.Title.ShouldBe(item.Title);
        result.Secret.ShouldBe(item.Secret);
        result.Tags.ShouldBe(item.Tags);
    }

    [Fact]
    public void EncryptDecrypt_WithEmptyPlaintext_ReturnsEmptyPlaintext()
    {
        // Arrange
        Aes256GcmCipher cipher = new();
        byte[] encryptionKey = CreateKey();

        // Act
        byte[] payload = cipher.Encrypt([], encryptionKey);
        byte[] decrypted = cipher.Decrypt(payload, encryptionKey);

        // Assert
        decrypted.ShouldBeEmpty();
    }

    [Fact]
    public void EncryptDecrypt_WithAllByteValues_ReturnsOriginalBytes()
    {
        // Arrange
        Aes256GcmCipher cipher = new();
        byte[] encryptionKey = CreateKey();
        byte[] plaintext = Enumerable.Range(0, 256).Select(value => (byte)value).ToArray();

        // Act
        byte[] payload = cipher.Encrypt(plaintext, encryptionKey);
        byte[] decrypted = cipher.Decrypt(payload, encryptionKey);

        // Assert
        decrypted.ShouldBe(plaintext);
    }

    [Fact]
    public void EncryptDecrypt_WithLargePlaintext_ReturnsOriginalBytes()
    {
        // Arrange
        Aes256GcmCipher cipher = new();
        byte[] encryptionKey = CreateKey();
        byte[] plaintext = Enumerable.Range(0, 1024 * 1024).Select(value => (byte)(value % 256)).ToArray();

        // Act
        byte[] payload = cipher.Encrypt(plaintext, encryptionKey);
        byte[] decrypted = cipher.Decrypt(payload, encryptionKey);

        // Assert
        decrypted.ShouldBe(plaintext);
    }

    [Fact]
    public void Encrypt_ReturnsPayloadWithNonceTagAndCiphertext()
    {
        // Arrange
        Aes256GcmCipher cipher = new();
        byte[] encryptionKey = CreateKey();
        byte[] plaintext = Encoding.UTF8.GetBytes("dados sensíveis");

        // Act
        byte[] payload = cipher.Encrypt(plaintext, encryptionKey);

        // Assert
        payload.Length.ShouldBe(PayloadOverhead + plaintext.Length);
        payload.AsSpan(0, NonceLength).SequenceEqual(new byte[NonceLength]).ShouldBeFalse();
        payload.AsSpan(NonceLength, TagLength).SequenceEqual(new byte[TagLength]).ShouldBeFalse();
    }

    [Fact]
    public void Encrypt_PayloadSegmentsCanBeDecryptedWithAesGcm()
    {
        // Arrange
        Aes256GcmCipher cipher = new();
        byte[] encryptionKey = CreateKey();
        byte[] plaintext = Encoding.UTF8.GetBytes("formato do payload");

        // Act
        byte[] payload = cipher.Encrypt(plaintext, encryptionKey);

        // Assert
        byte[] decrypted = new byte[plaintext.Length];
        using var aes = new AesGcm(encryptionKey, tagSizeInBytes: TagLength);
        aes.Decrypt(
            payload.AsSpan(0, NonceLength),
            payload.AsSpan(PayloadOverhead),
            payload.AsSpan(NonceLength, TagLength),
            decrypted);
        decrypted.ShouldBe(plaintext);
    }

    [Fact]
    public void Encrypt_WithSamePlaintext_ReturnsDifferentDecryptablePayloads()
    {
        // Arrange
        Aes256GcmCipher cipher = new();
        byte[] encryptionKey = CreateKey();
        byte[] plaintext = Encoding.UTF8.GetBytes("mesmo texto");

        // Act
        byte[] firstPayload = cipher.Encrypt(plaintext, encryptionKey);
        byte[] secondPayload = cipher.Encrypt(plaintext, encryptionKey);

        // Assert
        firstPayload.SequenceEqual(secondPayload).ShouldBeFalse();
        cipher.Decrypt(firstPayload, encryptionKey).ShouldBe(plaintext);
        cipher.Decrypt(secondPayload, encryptionKey).ShouldBe(plaintext);
    }

    [Fact]
    public async Task Encrypt_WithConcurrentCalls_ReturnsUniqueDecryptablePayloads()
    {
        // Arrange
        Aes256GcmCipher cipher = new();
        byte[] encryptionKey = CreateKey();
        byte[] plaintext = Encoding.UTF8.GetBytes("concorrência");
        const int count = 64;

        // Act
        byte[][] payloads = await Task.WhenAll(
            Enumerable.Range(0, count).Select(_ => Task.Run(() => cipher.Encrypt(plaintext, encryptionKey))));

        // Assert
        payloads.Select(payload => Convert.ToHexString(payload.AsSpan(0, NonceLength))).Distinct().Count().ShouldBe(count);
        payloads.All(payload => cipher.Decrypt(payload, encryptionKey).SequenceEqual(plaintext)).ShouldBeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(NonceLength - 1)]
    [InlineData(NonceLength)]
    [InlineData(PayloadOverhead - 1)]
    [InlineData(PayloadOverhead)]
    [InlineData(PayloadOverhead + PlaintextLength - 1)]
    public void Decrypt_WithTamperedPayload_ThrowsCryptographicException(int index)
    {
        // Arrange
        Aes256GcmCipher cipher = new();
        byte[] encryptionKey = CreateKey();
        byte[] plaintext = Enumerable.Range(0, PlaintextLength).Select(value => (byte)value).ToArray();
        byte[] payload = cipher.Encrypt(plaintext, encryptionKey);
        payload[index] ^= 0x01;

        // Act
        Action act = () => cipher.Decrypt(payload, encryptionKey);

        // Assert
        act.ShouldThrow<CryptographicException>();
    }

    [Fact]
    public void Decrypt_WithSwappedNonces_ThrowsCryptographicException()
    {
        // Arrange
        Aes256GcmCipher cipher = new();
        byte[] encryptionKey = CreateKey();
        byte[] plaintext = Encoding.UTF8.GetBytes("nonces trocados");
        byte[] firstPayload = cipher.Encrypt(plaintext, encryptionKey);
        byte[] secondPayload = cipher.Encrypt(plaintext, encryptionKey);
        firstPayload.AsSpan(0, NonceLength).SequenceEqual(secondPayload.AsSpan(0, NonceLength)).ShouldBeFalse();

        for (int index = 0; index < NonceLength; index++)
        {
            (firstPayload[index], secondPayload[index]) = (secondPayload[index], firstPayload[index]);
        }

        // Act
        Action act = () => cipher.Decrypt(firstPayload, encryptionKey);

        // Assert
        act.ShouldThrow<CryptographicException>();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Decrypt_WithModifiedPayloadLength_ThrowsCryptographicException(bool appendByte)
    {
        // Arrange
        Aes256GcmCipher cipher = new();
        byte[] encryptionKey = CreateKey();
        byte[] payload = cipher.Encrypt(Encoding.UTF8.GetBytes("tamanho"), encryptionKey);
        byte[] tamperedPayload = appendByte ? [.. payload, 0x00] : payload[..^1];

        // Act
        Action act = () => cipher.Decrypt(tamperedPayload, encryptionKey);

        // Assert
        act.ShouldThrow<CryptographicException>();
    }

    [Fact]
    public void Decrypt_WithWrongKey_ThrowsCryptographicException()
    {
        // Arrange
        Aes256GcmCipher cipher = new();
        byte[] encryptionKey = CreateKey();
        byte[] wrongKey = CreateKey(1);
        byte[] payload = cipher.Encrypt(Encoding.UTF8.GetBytes("chave errada"), encryptionKey);

        // Act
        Action act = () => cipher.Decrypt(payload, wrongKey);

        // Assert
        act.ShouldThrow<CryptographicException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(NonceLength - 1)]
    [InlineData(PayloadOverhead - 1)]
    public void Decrypt_WithPayloadShorterThanNonceAndTag_ThrowsCryptographicException(int length)
    {
        // Arrange
        Aes256GcmCipher cipher = new();
        byte[] encryptionKey = CreateKey();
        byte[] payload = new byte[length];

        // Act
        Action act = () => cipher.Decrypt(payload, encryptionKey);

        // Assert
        act.ShouldThrow<CryptographicException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(KeyLength - 1)]
    [InlineData(KeyLength + 1)]
    public void Encrypt_WithInvalidKeyLength_ThrowsArgumentException(int length)
    {
        // Arrange
        Aes256GcmCipher cipher = new();
        byte[] encryptionKey = new byte[length];

        // Act
        Action act = () => cipher.Encrypt(new byte[1], encryptionKey);

        // Assert
        act.ShouldThrow<ArgumentException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(KeyLength - 1)]
    [InlineData(KeyLength + 1)]
    public void Decrypt_WithInvalidKeyLength_ThrowsArgumentException(int length)
    {
        // Arrange
        Aes256GcmCipher cipher = new();
        byte[] validKey = CreateKey();
        byte[] encryptionKey = new byte[length];
        byte[] payload = cipher.Encrypt(new byte[1], validKey);

        // Act
        Action act = () => cipher.Decrypt(payload, encryptionKey);

        // Assert
        act.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void EncryptDecrypt_DoNotAlterCallerKeyPlaintextOrPayload()
    {
        // Arrange
        Aes256GcmCipher cipher = new();
        byte[] encryptionKey = CreateKey();
        byte[] plaintext = Encoding.UTF8.GetBytes("intacto");
        byte[] originalKey = encryptionKey.ToArray();
        byte[] originalPlaintext = plaintext.ToArray();
        byte[] payload = cipher.Encrypt(plaintext, encryptionKey);
        byte[] originalPayload = payload.ToArray();

        // Act
        cipher.Decrypt(payload, encryptionKey);

        // Assert
        encryptionKey.ShouldBe(originalKey);
        plaintext.ShouldBe(originalPlaintext);
        payload.ShouldBe(originalPayload);
    }

    private static byte[] CreateKey(byte firstByte = 0)
    {
        return Enumerable.Range(0, KeyLength).Select(index => (byte)(firstByte + index)).ToArray();
    }

    private sealed record VaultItem(string Title, string Secret, string[] Tags);
}
