using System.Threading;
using System.Threading.Tasks;
using Conduit.Features.Favorites;
using Conduit.Features.Articles;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Conduit.Tests.Features.Favorites;

public class FavoritesControllerTests : IDisposable
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly FavoritesController _controller;

    public FavoritesControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new FavoritesController(_mediatorMock.Object);
    }

    [Fact]
    public async Task FavoriteAdd_ShouldReturnArticleEnvelope()
    {
        // Arrange
        var slug = "test-article";
        var expectedArticle = new Conduit.Domain.Article { Title = "Test Article", Slug = slug };
        var expectedResponse = new ArticleEnvelope(expectedArticle);

        _mediatorMock.Setup(m => m.Send(It.IsAny<Add.Command>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.FavoriteAdd(slug, CancellationToken.None);

        // Assert
        Assert.Equal(expectedResponse, result);
        _mediatorMock.Verify(m => m.Send(It.IsAny<Add.Command>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task FavoriteDelete_ShouldReturnArticleEnvelopeAndRemoveFavorite()
    {
        // Arrange
        var slug = "test-article";
        var expectedArticle = new Conduit.Domain.Article { Title = "Test Article", Slug = slug };
        var expectedResponse = new ArticleEnvelope(expectedArticle);

        _mediatorMock.Setup(m => m.Send(It.IsAny<Conduit.Features.Favorites.Delete.Command>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.FavoriteDelete(slug, CancellationToken.None);

        // Assert
        Assert.Equal(expectedResponse, result);
        _mediatorMock.Verify(m => m.Send(It.IsAny<Conduit.Features.Favorites.Delete.Command>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    public void Dispose()
    {
        _controller.Dispose();
    }
}
