using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Features.Comments;
using Conduit.Infrastructure;
using Conduit.Domain;
using Conduit.Infrastructure.Errors;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Conduit.Tests.Features.Comments;

public class ListQueryHandlerTests : IDisposable
{
    private readonly ConduitContext _context;
    private readonly List.QueryHandler _handler;

    public ListQueryHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ConduitContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;

        _context = new ConduitContext(options);
        _handler = new List.QueryHandler(_context);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoCommentsExist()
    {
        // Arrange
        _context.Articles.RemoveRange(_context.Articles);
        _context.Comments.RemoveRange(_context.Comments);
        await _context.SaveChangesAsync();

        var article = new Article { Slug = "empty-article" };
        _context.Articles.Add(article);
        await _context.SaveChangesAsync();

        var query = new List.Query("empty-article");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Comments);
    }

    [Fact]
    public async Task Handle_ShouldThrowRestException_WhenArticleDoesNotExist()
    {
        // Arrange
        _context.Articles.RemoveRange(_context.Articles);
        _context.Comments.RemoveRange(_context.Comments);
        await _context.SaveChangesAsync();

        var query = new List.Query("non-existent-article");

        // Act & Assert
        await Assert.ThrowsAsync<RestException>(async () =>
            await _handler.Handle(query, CancellationToken.None));
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
