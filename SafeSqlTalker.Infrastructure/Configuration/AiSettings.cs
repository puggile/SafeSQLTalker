namespace SafeSqlTalker.Infrastructure.Configuration;

public class AiSettings
{
    // Costante utile per riferirsi alla sezione nel JSON senza scrivere stringhe a mano
    public const string SectionName = "AiSettings";

    public required string ModelId { get; set; }
    public required string Endpoint { get; set; }
    public string ApiKey { get; set; } = "ignore-this"; // Valore di default
}