namespace SafeSqlTalker.Core.Models;

public class QueryResponse
{
    public required string Question { get; set; }
    public required string GeneratedQuery { get; set; }
    public IEnumerable<dynamic>? Data { get; set; }
}