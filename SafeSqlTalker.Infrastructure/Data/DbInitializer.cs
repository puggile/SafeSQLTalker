using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;

namespace SafeSqlTalker.Infrastructure.Data;

public class DbInitializer
{
    private readonly string _connectionString;
    private readonly string _dbName = "init_db.sql";

    public DbInitializer(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
                            ?? "Data Source=safetalker.db";
    }

    public void Initialize()
    {
        // 1. Leggiamo lo script SQL creato prima
        var sqlFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _dbName);

        if (!File.Exists(sqlFile))
        {
            throw new Exception($"SqlLite creation script not found at path: {sqlFile}");
        }

        var script = File.ReadAllText(sqlFile);

        // 2. Eseguiamo lo script
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        connection.Execute(script);
    }
}