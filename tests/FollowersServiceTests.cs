using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Features.Followers;
using Conduit.Infrastructure;
using Conduit.Domain;
using Conduit.Infrastructure.Errors;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using Conduit.Features.Profiles;

namespace Conduit.Tests.Features.Followers;

public class FollowersServiceTests : IDisposable
{
    private readonly ConduitContext _context;
    private readonly Add.QueryHandler _addHandler;
    private readonly Delete.QueryHandler _deleteHandler;
    private readonly Mock<ICurrentUserAccessor> _currentUserAccessorMock;
    private readonly Mock<IProfileReader> _profileReaderMock;
    private readonly string _testUsername = "observer-user";
    private readonly string _targetUsername = "target-user";

    public FollowersServiceTests()
    {
        var options = new DbContextOptionsBuilder<ConduitContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;

        _context = new ConduitContext(options);
        _currentUserAccessorMock = new Mock<ICurrentUserAccessor>();
        _currentUserAccessorMock.Setup(x => x.GetCurrentUsername()).Returns(_testUsername);
        _profileReaderMock = new Mock<IProfileReader>();

        _addHandler = new Add.QueryHandler(_context, _currentUserAccessorMock.Object, _profileReaderMock.Object);
        _deleteHandler = new Delete.QueryHandler(_context, _currentUserAccessorMock.Object, _profileReaderMock.Object);

        SeedDatabase();
    }

    private void SeedDatabase()
    {
        _context.Persons.RemoveRange(_context.Persons);
        _context.FollowedPeople.RemoveRange(_context.FollowedPeople);
        _context.SaveChanges();

        var observer = new Person { Username = _testUsername };
        var target = new Person { Username = _targetUsername };

        _context.Persons.AddRange(observer, target);
        _context.SaveChanges();
    }

    [Fact]
    public async Task Add_ShouldThrowException_WhenUserDoesNotExist()
    {
        var command = new Add.Command("non-existent-user");

        await Assert.ThrowsAsync<RestException>(async () =>
            await _addHandler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Delete_ShouldThrowException_WhenUserDoesNotExist()
    {
        var command = new Delete.Command("non-existent-user");

        await Assert.ThrowsAsync<RestException>(async () =>
            await _deleteHandler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Add_ShouldCreateFollowedPeople_WhenUserExists()
    {
        // Arrange
        var command = new Add.Command(_targetUsername);
        var expectedProfile = new ProfileEnvelope(new Profile
        {
            Username = _targetUsername,
            Bio = "Test Bio",
            Image = "test.png"
        });
        _profileReaderMock.Setup(x => x.ReadProfile(_targetUsername, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(expectedProfile);

        // Act
        var result = await _addHandler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(expectedProfile, result);
        var observer = await _context.Persons.FirstOrDefaultAsync(p => p.Username == _testUsername);
        Assert.NotNull(observer);
        var target = await _context.Persons.FirstOrDefaultAsync(p => p.Username == _targetUsername);
        Assert.NotNull(target);
        var followed = await _context.FollowedPeople.FirstOrDefaultAsync(f => f.ObserverId == observer!.PersonId && f.TargetId == target!.PersonId);
        Assert.NotNull(followed);
    }

    [Fact]
    public async Task Add_ShouldNotDuplicateFollowedPeople_WhenAlreadyFollowing()
    {
        // Arrange
        var command = new Add.Command(_targetUsername);
        var expectedProfile = new ProfileEnvelope(new Profile
        {
            Username = _targetUsername,
            Bio = "Test Bio",
            Image = "test.png"
        });
        _profileReaderMock.Setup(x => x.ReadProfile(_targetUsername, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(expectedProfile);

        // Act
        await _addHandler.Handle(command, CancellationToken.None);
        await _addHandler.Handle(command, CancellationToken.None);

        // Assert
        var count = _context.FollowedPeople.Count(f => f.Observer!.Username == _testUsername && f.Target!.Username == _targetUsername);
        Assert.Equal(1, count);
    }

    [Fact]
    public async Task Delete_ShouldRemoveFollowedPeople_WhenFollowingExists()
    {
        // Arrange
        var commandAdd = new Add.Command(_targetUsername);
        var expectedProfile = new ProfileEnvelope(new Profile
        {
            Username = _targetUsername,
            Bio = "Test Bio",
            Image = "test.png"
        });
        _profileReaderMock.Setup(x => x.ReadProfile(_targetUsername, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(expectedProfile);
        await _addHandler.Handle(commandAdd, CancellationToken.None);

        var followedBefore = await _context.FollowedPeople.FirstOrDefaultAsync(f => f.Observer!.Username == _testUsername && f.Target!.Username == _targetUsername);
        Assert.NotNull(followedBefore);

        // Act
        var commandDelete = new Delete.Command(_targetUsername);
        var result = await _deleteHandler.Handle(commandDelete, CancellationToken.None);

        // Assert
        var followedAfter = await _context.FollowedPeople.FirstOrDefaultAsync(f => f.Observer!.Username == _testUsername && f.Target!.Username == _targetUsername);
        Assert.Null(followedAfter);
        Assert.Equal(expectedProfile, result);
    }

    [Fact]
    public async Task Delete_ShouldNotThrow_WhenNotFollowing()
    {
        // Arrange
        var commandDelete = new Delete.Command(_targetUsername);
        var expectedProfile = new ProfileEnvelope(new Profile
        {
            Username = _targetUsername,
            Bio = "Test Bio",
            Image = "test.png"
        });
        _profileReaderMock.Setup(x => x.ReadProfile(_targetUsername, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(expectedProfile);

        // Act
        var result = await _deleteHandler.Handle(commandDelete, CancellationToken.None);

        // Assert
        var followed = await _context.FollowedPeople.FirstOrDefaultAsync(f => f.Observer!.Username == _testUsername && f.Target!.Username == _targetUsername);
        Assert.Null(followed);
        Assert.Equal(expectedProfile, result);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
