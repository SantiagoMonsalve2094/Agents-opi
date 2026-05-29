namespace Agents.Opi.Backend.Infrastructure.GenerativeAi.OpenAi;

public sealed class OpenAiOptions
{
    public string Model { get; init; } = "gpt-5-nano";
    public int MaxOutputTokens { get; init; } = 20000;
}
