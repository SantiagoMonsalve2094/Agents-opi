namespace Agents.Opi.Backend.Application.Resources;

public static class ApplicationMessages
{
    public const string AgentTypeRequired = "El tipo de agente es obligatorio.";
    public const string InvalidAgentType = "Tipo de agente inválido. Usa QA o PO.";
    public const string PhaseRequired = "La fase es obligatoria.";
    public const string InvalidPhase = "Fase inválida para el agente indicado.";
    public const string InputRequired = "La entrada es obligatoria.";
    public const string UserRequired = "El usuario autenticado es obligatorio.";
    public const string UserExternalIdRequired = "El identificador externo del usuario autenticado es obligatorio.";
    public const string UserEmailRequired = "El correo del usuario autenticado es obligatorio.";
    public const string ConversationNotFound = "La conversación no existe.";
    public const string ConversationForbidden = "La conversación no pertenece al usuario autenticado.";
    public const string ConversationRequired = "La conversacion es obligatoria.";
    public const string FeedbackRequired = "El feedback es obligatorio.";
    public const string PhaseOutputNotFound = "La fase indicada aun no tiene una salida para refinar.";
    public const string PreviousOutputFallback = "No hay resultado anterior.";
}
