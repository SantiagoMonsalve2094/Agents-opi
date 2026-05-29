using Agents.Opi.Backend.Domain.Enums;

namespace Agents.Opi.Backend.Application.DTOs.Agent;

public sealed record GenerativeAiRequest(
    AgentType AgentType,
    string Prompt,
    bool IncludeKnowledgeSource);
