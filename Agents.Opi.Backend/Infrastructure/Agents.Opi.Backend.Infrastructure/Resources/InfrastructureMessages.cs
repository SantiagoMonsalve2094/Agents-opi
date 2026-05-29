namespace Agents.Opi.Backend.Infrastructure.Resources;

public static class InfrastructureMessages
{
    public const string MissingOpenAiApiKey = "No se encontro la API key configurada para el agente.";
    public const string OpenAiError = "OpenAI respondio con error HTTP {0}.";
    public const string OpenAiStreamError = "OpenAI respondio con error en el stream.";
    public const string OpenAiIncompleteOutput = "La respuesta del modelo quedo incompleta. Intenta nuevamente o reduce el alcance de la entrada.";
    public const string OpenAiEmptyOutput = "No se pudo generar la respuesta.";
    public const string OpenAiUnknownError = "Error desconocido de OpenAI.";
    public const string SyllabusMarkdownNotFound = "No se encontro el Markdown del syllabus ISTQB.";
}
