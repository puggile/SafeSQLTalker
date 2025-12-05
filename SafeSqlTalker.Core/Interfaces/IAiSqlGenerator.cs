namespace SafeSqlTalker.Core.Interfaces;

public interface IAiSqlGenerator
{
    /// <summary>
    /// Traduce una domanda in linguaggio naturale in una query SQL basata sullo schema fornito.
    /// </summary>
    Task<string> GenerateSqlQueryAsync(string userPrompt, string dbSchema);
}