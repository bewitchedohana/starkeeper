using Bogus;
using Shouldly;
using Starkeeper.Common.Cryptography.Deriver;

namespace Starkeeper.Common.Cryptography.UnitTests;

public sealed class MasterKeyDeriverTests
{
    [Fact]
    public async Task DeriveMasterKey_WithSameEmailAndPassword_ReturnsSameMasterKey()
    {
        // Arrange
        MasterKeyDeriver masterKeyDeriver = new();
        Faker faker = new() { Random = new Randomizer(1001) };
        const string password = "Senha@123";
        string email = faker.Internet.Email();

        // Act
        byte[] firstMasterKey = await masterKeyDeriver.DeriveMasterKey(password, email);
        byte[] secondMasterKey = await masterKeyDeriver.DeriveMasterKey(password, email);

        // Assert
        firstMasterKey.Length.ShouldBe(32);
        firstMasterKey.ShouldBe(secondMasterKey);
    }

    [Fact]
    public async Task DeriveMasterKey_WithDifferentEmailCasing_ReturnsSameMasterKey()
    {
        // Arrange
        MasterKeyDeriver masterKeyDeriver = new();
        Faker faker = new() { Random = new Randomizer(1002) };
        const string password = "Senha@123";
        string email = faker.Internet.Email();
        string upperCaseEmail = email.ToUpperInvariant();

        // Act
        byte[] lowerCaseEmailKey = await masterKeyDeriver.DeriveMasterKey(password, email);
        byte[] upperCaseEmailKey = await masterKeyDeriver.DeriveMasterKey(password, upperCaseEmail);

        // Assert
        lowerCaseEmailKey.ShouldBe(upperCaseEmailKey);
    }

    [Fact]
    public async Task DeriveMasterKey_WithDifferentPassword_ReturnsDifferentMasterKey()
    {
        // Arrange
        MasterKeyDeriver masterKeyDeriver = new();
        Faker faker = new() { Random = new Randomizer(1003) };
        string email = faker.Internet.Email();
        const string firstPassword = "Senha@123";
        const string secondPassword = "Senha@456";

        // Act
        byte[] firstMasterKey = await masterKeyDeriver.DeriveMasterKey(firstPassword, email);
        byte[] secondMasterKey = await masterKeyDeriver.DeriveMasterKey(secondPassword, email);

        // Assert
        firstMasterKey.SequenceEqual(secondMasterKey).ShouldBeFalse();
    }

    [Fact]
    public async Task DeriveMasterKey_WithDifferentEmail_ReturnsDifferentMasterKey()
    {
        // Arrange
        MasterKeyDeriver masterKeyDeriver = new();
        Faker faker = new() { Random = new Randomizer(1004) };
        const string password = "Senha@123";
        string firstEmail = faker.Internet.Email(uniqueSuffix: "1");
        string secondEmail = faker.Internet.Email(uniqueSuffix: "2");

        // Act
        byte[] firstMasterKey = await masterKeyDeriver.DeriveMasterKey(password, firstEmail);
        byte[] secondMasterKey = await masterKeyDeriver.DeriveMasterKey(password, secondEmail);

        // Assert
        firstMasterKey.SequenceEqual(secondMasterKey).ShouldBeFalse();
    }

    [Fact]
    public async Task DeriveMasterKey_WithNullPassword_ThrowsArgumentNullException()
    {
        // Arrange
        MasterKeyDeriver masterKeyDeriver = new();
        Faker faker = new() { Random = new Randomizer(1005) };
        string email = faker.Internet.Email();

        // Act
        Func<Task> act = () => masterKeyDeriver.DeriveMasterKey(null!, email);

        // Assert
        await act.ShouldThrowAsync<ArgumentNullException>();
    }
}
