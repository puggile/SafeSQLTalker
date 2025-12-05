namespace SafeSqlTalker.Core.Interfaces;

public interface ISqlGuard
{
    /// <summary>
    /// Analizza la query e restituisce true solo se è una SELECT sicura.
    /// Restituisce false e un messaggio di errore se rileva comandi pericolosi (DROP, DELETE, ecc.).
    /// </summary>
    (bool IsSafe, string? ErrorMessage) ValidateQuery(string sqlQuery);
}