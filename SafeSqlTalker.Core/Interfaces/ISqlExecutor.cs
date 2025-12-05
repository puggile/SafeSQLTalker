namespace SafeSqlTalker.Core.Interfaces;

public interface ISqlExecutor
{
    // Restituisce dynamic perché non sappiamo a priori quali colonne chiederà l'utente
    Task<IEnumerable<dynamic>> ExecuteQueryAsync(string sql);

    Task<string> GetDatabaseSchemaAsync();
}