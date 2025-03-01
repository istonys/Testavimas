using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Features.Articles;
using Conduit.Infrastructure;
using Conduit.Domain;
using Moq;
using Xunit;
using Microsoft.EntityFrameworkCore;

namespace Conduit.Tests.Features.Articles;

public class ListQueryHandlerTests : IDisposable
{
    private readonly ConduitContext _context;
    private readonly Mock<ICurrentUserAccessor> _currentUserAccessorMock;
    private readonly List.QueryHandler _handler;

    public ListQueryHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ConduitContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;

        _context = new ConduitContext(options);
        _currentUserAccessorMock = new Mock<ICurrentUserAccessor>();
        _handler = new List.QueryHandler(_context, _currentUserAccessorMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnArticles_WhenNoFiltersAreApplied()
    {
        // Arrange
        var query = new List.Query(string.Empty, string.Empty, string.Empty, 10, 0);

        _context.Articles.Add(new Article { Title = "Test Article", Slug = "test-article" });
        await _context.SaveChangesAsync();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Articles);
        Assert.Equal("Test Article", result.Articles.First().Title);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
