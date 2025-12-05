using SafeSqlTalker.Infrastructure.Security;

namespace SafeSqlTalker.Tests;

public class SqlGuardTests
{
    private readonly SqlGuard _guard = new();

    [Fact]
    public void ValidateQuery_ShouldAllow_SimpleSelect()
    {
        var sql = "SELECT * FROM Products";
        var result = _guard.ValidateQuery(sql);
        Assert.True(result.IsSafe);
    }

    [Fact]
    public void ValidateQuery_ShouldBlock_DropTable()
    {
        var sql = "DROP TABLE Products";
        var result = _guard.ValidateQuery(sql);
        Assert.False(result.IsSafe);
        Assert.Contains("Only SELECT", result.ErrorMessage);
    }

    [Theory]
    [InlineData("DELETE FROM Products")]
    [InlineData("INSERT INTO Products VALUES (1, 'A')")]
    [InlineData("UPDATE Products SET Price = 0")]
    [InlineData("TRUNCATE TABLE Products")]
    public void ValidateQuery_ShouldBlock_DestructiveCommands(string sql)
    {
        var result = _guard.ValidateQuery(sql);
        Assert.False(result.IsSafe);
    }

    [Fact]
    public void ValidateQuery_ShouldBlock_ObfuscatedDelete()
    {
        // Un parser regex semplice qui fallirebbe, ma l'AST Parser no.
        var sql = "SELECT * FROM Products; DELETE FROM Orders;";
        var result = _guard.ValidateQuery(sql);
        Assert.False(result.IsSafe);
        Assert.Contains("Multiple statements", result.ErrorMessage);
    }
}
