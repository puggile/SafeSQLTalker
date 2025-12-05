using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using SafeSqlTalker.Core.Interfaces;
using SafeSqlTalker.Core.Models;

namespace SafeSqlTalker.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QueryController : Controller
    {
        private readonly IAiSqlGenerator _aiGenerator;
        private readonly ISqlGuard _sqlGuard;
        private readonly ISqlExecutor _executor;
        private readonly ILogger<QueryController> _logger;
        private readonly IMemoryCache _cache; // Servizio di Caching

        public QueryController(
            IAiSqlGenerator aiGenerator,
            ISqlGuard sqlGuard,
            ISqlExecutor executor,
            ILogger<QueryController> logger,
            IMemoryCache cache) // Iniettiamo la cache
        {
            _aiGenerator = aiGenerator;
            _sqlGuard = sqlGuard;
            _executor = executor;
            _logger = logger;
            _cache = cache;
        }

        [HttpPost("ask")]
        [ProducesResponseType(typeof(QueryResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Ask([FromBody] UserQuestion request)
        {
            if (string.IsNullOrWhiteSpace(request.Text))
                return BadRequest("Please ask a question.");

            _logger.LogInformation("1. User asked: {Question}", request.Text);

            try
            {
                // Chiave univoca per la cache
                var cacheKey = "DbSchemaDefinition";

                // Proviamo a prenderlo dalla cache. Se non c'è, eseguiamo la funzione.
                var schemaDef = await _cache.GetOrCreateAsync(cacheKey, async entry =>
                {
                    // Impostiamo una scadenza (es. lo schema scade dopo 1 ora)
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);

                    _logger.LogInformation("Cache Miss: Fetching schema from Database...");
                    return await _executor.GetDatabaseSchemaAsync();
                }) ?? string.Empty;
                // --------------------------------------------------

                // Step 1: AI Generation (Usiamo lo schema dinamico)
                var generatedSql = await _aiGenerator.GenerateSqlQueryAsync(request.Text, schemaDef);
                _logger.LogInformation("2. AI Generated SQL: {Sql}", generatedSql);

                // Step 2: Guard Rails
                var safetyCheck = _sqlGuard.ValidateQuery(generatedSql);
                if (!safetyCheck.IsSafe)
                {
                    _logger.LogWarning("SECURITY BLOCK: {Error}", safetyCheck.ErrorMessage);
                    return BadRequest(new
                    {
                        error = "Safety Protocol Engaged",
                        details = safetyCheck.ErrorMessage,
                        queryAttempted = generatedSql
                    });
                }

                // Step 3: Execution
                var results = await _executor.ExecuteQueryAsync(generatedSql);

                // Materializziamo la lista per contarla e restituirla
                var resultList = results.ToList();
                _logger.LogInformation("3. Query Executed Successfully. Rows returned: {Count}", resultList.Count);

                return Ok(new QueryResponse
                {
                    Question = request.Text,
                    GeneratedQuery = generatedSql,
                    Data = resultList
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing request");
                return StatusCode(500, "An internal error occurred.");
            }
        }
    }
}
