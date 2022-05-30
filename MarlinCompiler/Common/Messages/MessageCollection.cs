using System.Collections;
using System.Collections.Concurrent;
using MarlinCompiler.Common.FileLocations;

namespace MarlinCompiler.Common.Messages;

/// <summary>
/// Utility class for managing a collection of compilation messages.
/// </summary>
public class MessageCollection : IEnumerable<Message>
{
    public MessageCollection()
    {
        _messages = new ConcurrentBag<Message>();
    }

    private readonly ConcurrentBag<Message> _messages;

    public bool HasFatalErrors => _messages.Any(x => x.Fatality == MessageFatality.Severe);

    public void Error(MessageId id, string message, TokenLocation location)
    {
        _messages.Add(
            new Message(id, message)
            {
                Location = location,
                Fatality = MessageFatality.Severe
            }
        );
    }

    public void Warn(MessageId id, string message, TokenLocation? location)
    {
        _messages.Add(
            new Message(id, message)
            {
                Location = location,
                Fatality = MessageFatality.Warning
            }
        );
    }

    public void Info(MessageId id, string message, TokenLocation? location)
    {
        _messages.Add(
            new Message(id, message)
            {
                Location = location,
                Fatality = MessageFatality.Information
            }
        );
    }

    public void AddRange(MessageCollection other)
    {
        foreach (Message msg in other._messages)
        {
            _messages.Add(msg);
        }
    }

    public IEnumerator<Message> GetEnumerator()
    {
        return _messages.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return _messages.GetEnumerator();
    }
}