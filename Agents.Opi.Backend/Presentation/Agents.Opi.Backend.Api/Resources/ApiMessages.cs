namespace Agents.Opi.Backend.Api.Resources;

public static class ApiMessages
{
    public const string BadRequestTitle = "Solicitud inválida";
    public const string UnauthorizedTitle = "No autenticado";
    public const string ForbiddenTitle = "No autorizado";
    public const string NotFoundTitle = "No encontrado";
    public const string ValidationTitle = "Validación";
    public const string ConflictTitle = "Conflicto";
    public const string UnexpectedTitle = "Error inesperado";
    public const string BadRequest = "La solicitud no pudo procesarse.";
    public const string Unauthorized = "Debes autenticarte desde la extensión de Azure DevOps.";
    public const string Forbidden = "No tienes permisos para acceder a este recurso.";
    public const string NotFound = "El recurso solicitado no existe.";
    public const string Validation = "Hay campos inválidos o faltantes.";
    public const string Conflict = "La operación no pudo completarse por el estado actual.";
    public const string Unexpected = "No se pudo completar la operación.";
    public const string GenerationFailed = "No se pudo generar la respuesta.";
    public const string InvalidAgentType = "Tipo de agente inválido. Usa QA o PO.";
    public const string InvalidPhase = "Fase inválida para el agente indicado.";
    public const string MissingExtensionSecret = "AZURE_DEVOPS_EXTENSION_SECRET no está configurado.";
}
