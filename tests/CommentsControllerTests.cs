using System.Threading;
using System.Threading.Tasks;
using Conduit.Features.Comments;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Conduit.Tests.Features.Comments;

public class CommentsControllerTests : IDisposable
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly CommentsController _controller;

    public CommentsControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new CommentsController(_mediatorMock.Object);
    }

    [Fact]
    public async Task Get_ShouldReturnCommentsEnvelope()
    {
        // Arrange
        var slug = "test-article";
        var expectedResponse = new CommentsEnvelope([]);
        _mediatorMock.Setup(m => m.Send(It.IsAny<List.Query>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.Get(slug, CancellationToken.None);

        // Assert
        Assert.Equal(expectedResponse, result);
        _mediatorMock.Verify(m => m.Send(It.IsAny<List.Query>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Create_ShouldReturnCommentEnvelope()
    {
        // Arrange
        var slug = "test-article";
        var model = new Create.Model(new Create.CommentData("Test comment"));
        var expectedResponse = new CommentEnvelope(new Conduit.Domain.Comment { Body = "Test comment" });
        _mediatorMock.Setup(m => m.Send(It.IsAny<Create.Command>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.Create(slug, model, CancellationToken.None);

        // Assert
        Assert.Equal(expectedResponse, result);
        _mediatorMock.Verify(m => m.Send(It.IsAny<Create.Command>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Delete_ShouldCallMediatorSend()
    {
        // Arrange
        var slug = "test-article";
        var commentId = 1;
        var command = new Delete.Command(slug, commentId);
        _mediatorMock.Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _controller.Delete(slug, commentId, CancellationToken.None);

        // Assert
        _mediatorMock.Verify(m => m.Send(It.Is<Delete.Command>(q => q.Slug == slug && q.Id == commentId), It.IsAny<CancellationToken>()), Times.Once);
    }

    public void Dispose()
    {
        _controller.Dispose();
    }
}
