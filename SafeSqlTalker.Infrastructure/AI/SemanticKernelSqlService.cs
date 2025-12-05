using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using SafeSqlTalker.Core.Interfaces;
using SafeSqlTalker.Infrastructure.Configuration;

namespace SafeSqlTalker.Infrastructure.AI;

public class SemanticKernelSqlService : IAiSqlGenerator
{
    private readonly Kernel _kernel;
    private readonly IChatCompletionService _chatCompletionService;
    private readonly AiSettings _settings;

    public SemanticKernelSqlService(IOptions<AiSettings> options)
    {
        // .Value estrae l'istanza popolata dal file JSON
        _settings = options.Value;

        // Validazione difensiva (Tech Lead practice)
        if (string.IsNullOrWhiteSpace(_settings.ModelId))
            throw new ArgumentNullException(nameof(_settings.ModelId), "AI ModelId is missing in configuration.");

        if (string.IsNullOrWhiteSpace(_settings.Endpoint))
            throw new ArgumentNullException(nameof(_settings.Endpoint), "AI Endpoint is missing in configuration.");

        var builder = Kernel.CreateBuilder();

        builder.AddOpenAIChatCompletion(
            modelId: _settings.ModelId,
            apiKey: _settings.ApiKey,
            endpoint: new Uri(_settings.Endpoint)
        );

        _kernel = builder.Build();
        _chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();
    }


    public async Task<string> GenerateSqlQueryAsync(string userPrompt, string dbSchema)
    {
        // 1. System Prompt Engineering (Fondamentale)
        // Istruiamo il modello a comportarsi come un Senior SQL Expert.
        var systemMessage = $@"
You are a Senior SQL Expert specializing in SQLite.
Your task is to convert the user's natural language question into a valid SQL query.

RULES:
1. Use ONLY the provided database schema.
2. Return ONLY the raw SQL query. Do not use Markdown blocks (```sql), do not add explanations.
3. The query must be a Read-Only SELECT statement.
4. If the user asks to MODIFY data (INSERT, UPDATE, DELETE, DROP, ALTER) or perform destructive actions, YOU MUST RETURN EXACTLY: 'VIOLATION_REQUEST'.
6. Do NOT generate SQL if the intent is destructive.
6. If the request cannot be satisfied with the schema, return 'SELECT NULL'.
7. **IMPORTANT: Always use 'SELECT DISTINCT' when retrieving lists of names or entities to avoid duplicates.**
8. **Use meaningful aliases for tables (e.g., 'p' for Products, 'o' for Orders) instead of T1, T2.**
9. **For string comparisons, prefer the use of LIKE operator to handle potential case sensitivity issues (e.g. WHERE Region LIKE 'Milano').**

DB SCHEMA:
{dbSchema}
";
        // 2. Costruzione della Chat
        var history = new ChatHistory();
        history.AddSystemMessage(systemMessage);
        history.AddUserMessage(userPrompt);

        // 3. Invocazione AI
        var result = await _chatCompletionService.GetChatMessageContentAsync(
            history,
            kernel: _kernel
        );

        var output = result.Content?.Trim() ?? string.Empty;

        // Se il modello ha rilevato un intento malevolo, lanciamo un'eccezione specifica
        // che il Controller gestirà.
        if (output.Contains("VIOLATION_REQUEST"))
        {
            throw new InvalidOperationException("SECURITY_ALERT: Destructive intent detected by AI.");
        }

        return CleanLlamaOutput(output);
    }

    private string CleanLlamaOutput(string output)
    {
        return output.Replace("```sql", "").Replace("```", "").Trim();
    }
}