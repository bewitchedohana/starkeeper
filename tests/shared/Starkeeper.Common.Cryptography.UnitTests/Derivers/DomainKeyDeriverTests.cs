using System.Reflection;
using Shouldly;
using Starkeeper.Common.Cryptography.Derivers;

namespace Starkeeper.Common.Cryptography.UnitTests.Derivers;

public sealed class DomainKeyDeriverTests
{
    private const string ExpectedEncryptionKeyHex = "24cd5dc6d068add82e7f8eab6b3eef2e6bccd40f2eb736756fe6d56cb057114b";
    private const string ExpectedClientAuthHashHex = "a901bdcb885bc63aad1f0d4c4306691e6360754249164967264d5b60a27959d7";
    private const int ExpectedDerivedKeyLength = 32;

    [Fact]
    public void Derive_WithSameMasterKey_ReturnsSameKeys()
    {
        // Arrange
        DomainKeyDeriver domainKeyDeriver = new();
        byte[] masterKey = CreateMasterKey();

        // Act
        using DerivedKeys firstKeys = domainKeyDeriver.Derive(masterKey);
        using DerivedKeys secondKeys = domainKeyDeriver.Derive(masterKey);

        // Assert
        firstKeys.EncryptionKey.Length.ShouldBe(ExpectedDerivedKeyLength);
        firstKeys.ClientAuthHash.Length.ShouldBe(ExpectedDerivedKeyLength);
        firstKeys.EncryptionKey.ToArray().ShouldBe(secondKeys.EncryptionKey.ToArray());
        firstKeys.ClientAuthHash.ToArray().ShouldBe(secondKeys.ClientAuthHash.ToArray());
    }

    [Fact]
    public void Derive_WithSameMasterKey_ReturnsDistinctKeys()
    {
        // Arrange
        DomainKeyDeriver domainKeyDeriver = new();
        byte[] masterKey = CreateMasterKey();

        // Act
        using DerivedKeys keys = domainKeyDeriver.Derive(masterKey);

        // Assert
        keys.EncryptionKey.Span.SequenceEqual(keys.ClientAuthHash.Span).ShouldBeFalse();
    }

    [Fact]
    public void Derive_WithKnownMasterKey_ReturnsExpectedEncryptionKey()
    {
        // Arrange
        DomainKeyDeriver domainKeyDeriver = new();
        byte[] masterKey = CreateMasterKey();

        // Act
        using DerivedKeys keys = domainKeyDeriver.Derive(masterKey);

        // Assert
        keys.EncryptionKey.ToArray().ShouldBe(Convert.FromHexString(ExpectedEncryptionKeyHex));
    }

    [Fact]
    public void Derive_WithKnownMasterKey_ReturnsExpectedClientAuthHash()
    {
        // Arrange
        DomainKeyDeriver domainKeyDeriver = new();
        byte[] masterKey = CreateMasterKey();

        // Act
        using DerivedKeys keys = domainKeyDeriver.Derive(masterKey);

        // Assert
        keys.ClientAuthHash.ToArray().ShouldBe(Convert.FromHexString(ExpectedClientAuthHashHex));
    }

    [Fact]
    public void Derive_WithDifferentMasterKey_ReturnsDifferentKeys()
    {
        // Arrange
        DomainKeyDeriver domainKeyDeriver = new();
        byte[] firstMasterKey = CreateMasterKey();
        byte[] secondMasterKey = CreateMasterKey(1);

        // Act
        using DerivedKeys firstKeys = domainKeyDeriver.Derive(firstMasterKey);
        using DerivedKeys secondKeys = domainKeyDeriver.Derive(secondMasterKey);

        // Assert
        firstKeys.EncryptionKey.Span.SequenceEqual(secondKeys.EncryptionKey.Span).ShouldBeFalse();
        firstKeys.ClientAuthHash.Span.SequenceEqual(secondKeys.ClientAuthHash.Span).ShouldBeFalse();
    }

    [Fact]
    public void Derive_WithNullMasterKey_ThrowsArgumentNullException()
    {
        // Arrange
        DomainKeyDeriver domainKeyDeriver = new();

        // Act
        Action act = () => domainKeyDeriver.Derive(null!);

        // Assert
        act.ShouldThrow<ArgumentNullException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(31)]
    [InlineData(33)]
    public void Derive_WithInvalidMasterKeyLength_ThrowsArgumentException(int length)
    {
        DomainKeyDeriver domainKeyDeriver = new();
        byte[] masterKey = new byte[length];

        Action act = () => domainKeyDeriver.Derive(masterKey);

        act.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Derive_AndDispose_DoNotAlterCallerMasterKey()
    {
        DomainKeyDeriver domainKeyDeriver = new();
        byte[] masterKey = CreateMasterKey();
        byte[] originalMasterKey = masterKey.ToArray();

        using (DerivedKeys keys = domainKeyDeriver.Derive(masterKey))
        {
            keys.EncryptionKey.Length.ShouldBe(ExpectedDerivedKeyLength);
        }

        masterKey.ShouldBe(originalMasterKey);
    }

    [Fact]
    public void Dispose_ZerosDerivedKeyArrays()
    {
        DomainKeyDeriver domainKeyDeriver = new();
        DerivedKeys keys = domainKeyDeriver.Derive(CreateMasterKey());
        ReadOnlyMemory<byte> encryptionKey = keys.EncryptionKey;
        ReadOnlyMemory<byte> clientAuthHash = keys.ClientAuthHash;

        encryptionKey.Span.SequenceEqual(new byte[ExpectedDerivedKeyLength]).ShouldBeFalse();
        clientAuthHash.Span.SequenceEqual(new byte[ExpectedDerivedKeyLength]).ShouldBeFalse();

        keys.Dispose();

        encryptionKey.ToArray().ShouldBe(new byte[ExpectedDerivedKeyLength]);
        clientAuthHash.ToArray().ShouldBe(new byte[ExpectedDerivedKeyLength]);
    }

    [Fact]
    public void Derive_AfterDispose_ThrowsObjectDisposedException()
    {
        DomainKeyDeriver domainKeyDeriver = new();
        DerivedKeys keys = domainKeyDeriver.Derive(CreateMasterKey());

        keys.Dispose();

        Action act = () => { _ = keys.EncryptionKey; };
        Action actAuth = () => { _ = keys.ClientAuthHash; };
        act.ShouldThrow<ObjectDisposedException>();
        actAuth.ShouldThrow<ObjectDisposedException>();
    }

    [Fact]
    public void DerivedKeys_ExposesOnlyRawByteMemory()
    {
        PropertyInfo[] properties = typeof(DerivedKeys).GetProperties();

        properties.Length.ShouldBe(2);
        Assert.All(properties, property => Assert.Equal(typeof(ReadOnlyMemory<byte>), property.PropertyType));
        typeof(IDisposable).IsAssignableFrom(typeof(DerivedKeys)).ShouldBeTrue();
    }

    private static byte[] CreateMasterKey(byte firstByte = 0)
    {
        return Enumerable.Range(0, ExpectedDerivedKeyLength).Select(index => (byte)(firstByte + index)).ToArray();
    }
}
