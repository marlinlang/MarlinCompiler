using MarlinCompiler.Common.FileLocations;

namespace MarlinCompiler.Common.Messages;

/// <summary>
/// The fatality of a message.
/// </summary>
public enum MessageFatality
{
    /// <summary>
    /// Fatal error.
    /// </summary>
    Severe,

    /// <summary>
    /// A stylistic or semantic possible mistake.
    /// </summary>
    Warning,

    /// <summary>
    /// Any message that isn't caused by a mistake or fatal error.
    /// </summary>
    Information
}

/// <summary>
/// A compilation message.
/// </summary>
/// <param name="Content">The message itself.</param>
public readonly record struct Message(MessageId Id, string Content)
{
    /// <summary>
    /// The location of the cause of the message.
    /// </summary>
    public TokenLocation? Location { get; init; } = null;

    /// <summary>
    /// The fatality of the message.
    /// </summary>
    public MessageFatality Fatality { get; init; } = MessageFatality.Information;

    public ConsoleColor PrintColor => Fatality switch
    {
        MessageFatality.Severe      => ConsoleColor.Red,
        MessageFatality.Warning     => ConsoleColor.Yellow,
        MessageFatality.Information => ConsoleColor.Cyan,
        _                           => throw new InvalidOperationException()
    };
}