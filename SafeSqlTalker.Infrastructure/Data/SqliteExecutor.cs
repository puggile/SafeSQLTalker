using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SafeSqlTalker.Core.Interfaces;
using System.Text;

namespace SafeSqlTalker.Infrastructure.Data;

public class SqliteExecutor : ISqlExecutor
{
    private readonly string _connectionString;
    // 1. Aggiungiamo il campo per il Logger
    private readonly ILogger<SqliteExecutor> _logger;

    public SqliteExecutor(IConfiguration configuration, ILogger<SqliteExecutor> logger)
    {
        // Recuperiamo la stringa di connessione da appsettings.json
        _connectionString = configuration.GetConnectionString("DefaultConnection")
                            ?? "Data Source=safetalker.db";
        _logger = logger;
    }

    public async Task<IEnumerable<dynamic>> ExecuteQueryAsync(string sql)
    {
        using var connection = new SqliteConnection(_connectionString);

        // Dapper apre la connessione, esegue e mappa il risultato in oggetti dinamici
        // Questo ci permette di restituire JSON con qualsiasi colonna (Nome, Prezzo, Totale, ecc.)
        return await connection.QueryAsync(sql);
    }

    public async Task<string> GetDatabaseSchemaAsync()
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        var schemaBuilder = new StringBuilder();

        // 1. Prendi tutte le tabelle utente (escludi quelle interne di sqlite)
        var tables = await connection.QueryAsync<string>(
            "SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%';");

        // Convertiamo in lista per contarle ed evitare problemi di enumerazione multipla
        var tableList = tables.ToList();
        _logger.LogInformation("Schema Discovery: Found {Count} tables: {TableNames}",
            tableList.Count,
            string.Join(", ", tableList));
        // -----------------------------------------------

        foreach (var tableName in tableList)
        {
            // 2. Per ogni tabella, ottieni le info sulle colonne (PRAGMA table_info)
            // Restituisce colonne: cid, name, type, notnull, dflt_value, pk
            var columns = await connection.QueryAsync(
                $"PRAGMA table_info({tableName})");

            // 3. Formatta stile: "TableName (Col1 Type, Col2 Type)"
            var columnDefs = columns.Select(c => $"{c.name} {c.type}");
            var columnsString = string.Join(", ", columnDefs);

            schemaBuilder.AppendLine($"{tableName} ({columnsString})");
        }

        return schemaBuilder.ToString();
    }
}