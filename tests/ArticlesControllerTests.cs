using System.Threading;
using System.Threading.Tasks;
using Conduit.Features.Articles;
using Conduit.Domain;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Conduit.Tests.Features.Articles;

public class ArticlesControllerTests : IDisposable
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly ArticlesController _controller;

    private static readonly string[] TestTags = ["tag1", "tag2"];

    public ArticlesControllerTests()
    {
        _mediatorMock = new Mock<IMediator>(); // Mocking the mediator dependency
        _controller = new ArticlesController(_mediatorMock.Object); // Injecting the mock into the controller
    }

    [Fact]
    public async Task Get_ShouldReturnArticlesEnvelope()
    {
        var expectedResponse = new ArticlesEnvelope(); // Arrange: Mock expected response
        _mediatorMock.Setup(m => m.Send(It.IsAny<List.Query>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse); // Mock Mediator behavior

        var result = await _controller.Get("", "", "", 10, 0, CancellationToken.None); // Act: Call Get method

        Assert.Equal(expectedResponse, result); // Assert: Check if returned value matches expected
        _mediatorMock.Verify(m => m.Send(It.IsAny<List.Query>(), It.IsAny<CancellationToken>()), Times.Once); // Ensure method was called once
    }

    [Fact]
    public async Task GetFeed_ShouldReturnArticlesEnvelope()
    {
        var expectedResponse = new ArticlesEnvelope();
        _mediatorMock.Setup(m => m.Send(It.IsAny<List.Query>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var result = await _controller.GetFeed("", "", "", 10, 0, CancellationToken.None);

        Assert.Equal(expectedResponse, result);
        _mediatorMock.Verify(m => m.Send(It.IsAny<List.Query>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Get_WithSlug_ShouldReturnArticleEnvelope()
    {
        var slug = "test-slug";
        var mockArticle = new Article { Title = "Test", Description = "Test Desc", Body = "Test Body" }; // Creating a mock article
        var expectedResponse = new ArticleEnvelope(mockArticle); // Wrapping it in expected response
        _mediatorMock.Setup(m => m.Send(It.IsAny<Details.Query>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var result = await _controller.Get(slug, CancellationToken.None);

        Assert.Equal(expectedResponse, result);
        _mediatorMock.Verify(m => m.Send(It.Is<Details.Query>(q => q.Slug == slug), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Create_ShouldReturnArticleEnvelope()
    {
        var command = new Create.Command(new Create.ArticleData { Title = "Test", Description = "Test Desc", Body = "Test Body" }); // Creating a valid command
        var mockArticle = new Article { Title = "Test", Description = "Test Desc", Body = "Test Body" };
        var expectedResponse = new ArticleEnvelope(mockArticle);
        _mediatorMock.Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var result = await _controller.Create(command, CancellationToken.None);

        Assert.Equal(expectedResponse, result);
        _mediatorMock.Verify(m => m.Send(command, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Edit_ShouldReturnArticleEnvelope()
    {
        var slug = "test-slug";
        var model = new Edit.Model(new Edit.ArticleData("Test Title", "Test Desc", "Test Body", TestTags)); // Creating a valid edit model
        var command = new Edit.Command(model, slug);
        var mockArticle = new Article { Title = "Test", Description = "Test Desc", Body = "Test Body" };
        var expectedResponse = new ArticleEnvelope(mockArticle);
        _mediatorMock.Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var result = await _controller.Edit(slug, model, CancellationToken.None);

        Assert.Equal(expectedResponse, result);
        _mediatorMock.Verify(m => m.Send(command, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Delete_ShouldCallMediatorSend()
    {
        var slug = "test-slug";
        var command = new Delete.Command(slug);
        _mediatorMock.Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _controller.Delete(slug, CancellationToken.None);

        _mediatorMock.Verify(m => m.Send(It.Is<Delete.Command>(q => q.Slug == slug), It.IsAny<CancellationToken>()), Times.Once); // Ensuring the delete command is sent once
    }

    public void Dispose()
    {
        _controller.Dispose(); // Clean up resources after test execution
    }
}
