using System.Collections;
using System.Collections.Concurrent;
using System.Linq;

namespace MarlinCompiler.Common;

/// <summary>
/// Utility class for managing a collection of compilation messages.
/// </summary>
public class MessageCollection : IEnumerable<Message>
{
    public MessageCollection()
    {
        _messages = new ConcurrentBag<Message>();
    }
    
    private ConcurrentBag<Message> _messages;

    public bool HasFatalErrors => _messages.Any(x => x.Fatality == MessageFatality.Severe);

    public void Error(string message, FileLocation? location)
    {
        _messages.Add(new Message(message)
        {
            Location =  location,
            Fatality = MessageFatality.Severe
        });
    }

    public void Warn(string message, FileLocation? location)
    {
        _messages.Add(new Message(message)
        {
            Location =  location,
            Fatality = MessageFatality.Warning
        });
    }

    public void Info(string message, FileLocation? location)
    {
        _messages.Add(new Message(message)
        {
            Location =  location,
            Fatality = MessageFatality.Information
        });
    }

    public void AddRange(MessageCollection other)
    {
        foreach (Message msg in other._messages)
        {
            _messages.Add(msg);
        }
    }

    public IEnumerator<Message> GetEnumerator() => _messages.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => _messages.GetEnumerator();
}