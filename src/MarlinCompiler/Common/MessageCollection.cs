using System.Collections;
using System.Linq;

namespace MarlinCompiler.Common;

/// <summary>
/// Utility class for managing a collection of compilation messages.
/// </summary>
public class MessageCollection : IEnumerable<Message>
{
    private List<Message> _messages;

    public bool HasFatalErrors => _messages.Any(x => x.Fatality == MessageFatality.Severe);

    public MessageCollection()
    {
        _messages = new List<Message>();
    }

    public void Error(string message, FileLocation? location = null)
    {
        _messages.Add(new Message(message)
        {
            Location =  location,
            Fatality = MessageFatality.Severe
        });
    }

    public void Warn(string message, FileLocation? location = null)
    {
        _messages.Add(new Message(message)
        {
            Location =  location,
            Fatality = MessageFatality.Warning
        });
    }

    public void Info(string message, FileLocation? location = null)
    {
        _messages.Add(new Message(message)
        {
            Location =  location,
            Fatality = MessageFatality.Information
        });
    }
    
    public void AddRange(MessageCollection other) => _messages.AddRange(other._messages);

    public IEnumerator<Message> GetEnumerator() => _messages.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => _messages.GetEnumerator();
}