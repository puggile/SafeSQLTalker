using Microsoft.SqlServer.TransactSql.ScriptDom;
using SafeSqlTalker.Core.Interfaces;

namespace SafeSqlTalker.Infrastructure.Security;

public class SqlGuard : ISqlGuard
{
    public (bool IsSafe, string? ErrorMessage) ValidateQuery(string sqlQuery)
    {
        // Setup del Parser 
        var parser = new TSql160Parser(true);
        IList<ParseError> errors;

        // Parsing: Trasforma la stringa in un albero di oggetti (AST)
        using var reader = new StringReader(sqlQuery);
        var fragment = parser.Parse(reader, out errors);

        // Controllo errori di sintassi di base
        if (errors.Count > 0)
        {
            return (false, $"Syntax Error: {errors[0].Message}");
        }

        // Analisi AST: Controlliamo cosa c'è dentro
        var script = fragment as TSqlScript;
        if (script == null) return (false, "Invalid SQL Script");

        // Regola 1: Deve esserci esattamente uno statement (niente query multiple con ;)
        if (script.Batches.Count != 1 || script.Batches[0].Statements.Count != 1)
        {
            return (false, "Safety Alert: Multiple statements or empty batches are not allowed.");
        }

        var statement = script.Batches[0].Statements[0];

        // Regola 2: Whitelist - Accettiamo SOLO SELECT
        if (statement is not SelectStatement)
        {
            // Qui becchiamo tutto: DROP, DELETE, UPDATE, TRUNCATE, ALTER...
            return (false, $"Safety Alert: Only SELECT statements are allowed. Detected: {statement.GetType().Name}");
        }

        // Se siamo qui, è una SELECT sintatticamente valida e singola.
        return (true, null);
    }
}