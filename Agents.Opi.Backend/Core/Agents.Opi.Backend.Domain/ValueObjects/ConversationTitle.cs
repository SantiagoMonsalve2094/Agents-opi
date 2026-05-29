namespace Agents.Opi.Backend.Domain.ValueObjects;

public sealed record ConversationTitle(string Value)
{
    private const int MaxLength = 80;

    public static ConversationTitle FromInput(string input)
    {
        var normalized = string.Join(' ', input.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries));
        return new ConversationTitle(normalized.Length <= MaxLength ? normalized : normalized[..MaxLength]);
    }

    public override string ToString() => Value;
}
