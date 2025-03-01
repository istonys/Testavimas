using System.Threading;
using System.Threading.Tasks;
using Conduit.Features.Followers;
using Conduit.Features.Profiles;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Conduit.Tests.Features.Followers;

public class FollowersControllerTests : IDisposable
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly FollowersController _controller;

    public FollowersControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new FollowersController(_mediatorMock.Object);
    }

    [Fact]
    public async Task Follow_ShouldReturnProfileEnvelope_WhenUserExists()
    {
        var username = "test-user";

        var expectedProfile = new ProfileEnvelope(new Profile
        {
            Username = username,
            Bio = "Test Bio",
            Image = "https://example.com/avatar.jpg"
        });

        _mediatorMock.Setup(m => m.Send(It.IsAny<Add.Command>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedProfile);

        var result = await _controller.Follow(username, CancellationToken.None);

        Assert.Equal(expectedProfile, result);
        _mediatorMock.Verify(m => m.Send(It.IsAny<Add.Command>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Unfollow_ShouldReturnProfileEnvelope_WhenUserExists()
    {
        var username = "test-user";

        var expectedProfile = new ProfileEnvelope(new Profile
        {
            Username = username,
            Bio = "Test Bio",
            Image = "https://example.com/avatar.jpg"
        });

        _mediatorMock.Setup(m => m.Send(It.IsAny<Delete.Command>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedProfile);

        var result = await _controller.Unfollow(username, CancellationToken.None);

        Assert.Equal(expectedProfile, result);
        _mediatorMock.Verify(m => m.Send(It.IsAny<Delete.Command>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    public void Dispose()
    {
        _controller.Dispose();
    }
}
