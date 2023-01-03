using System.Diagnostics.Tracing;
namespace ES_Test1;
[EventSource(Name = "Test1")]
public sealed class Test1EventSource : EventSource
{
    public static Test1EventSource Log { get; } = new Test1EventSource();

    [Event(1, Keywords = Keywords.Startup)]
    public void AppStarted(string message, int favoriteNumber) => WriteEvent(1, message, favoriteNumber);

    [Event(2, Keywords = Keywords.Requests)]
    public void RequestStart(int requestId) => WriteEvent(2, requestId);

    [Event(3, Keywords = Keywords.Requests)]
    public void RequestStop(int requestId) => WriteEvent(3, requestId);

    [Event(4, Keywords = Keywords.Startup, Level = EventLevel.Verbose)]
    public void DebugMessage(string message) => WriteEvent(4, message);

    [Event(5, Keywords = Keywords.Requests)]
    public void AppInfo(object arg) => WriteEvent(5, new {x = 3, y = 5});
    //protected void WriteEvent (int eventId, params object?[] args)

    public static class Keywords
    {
        public const EventKeywords Startup = (EventKeywords)0x0001;
        public const EventKeywords Requests = (EventKeywords)0x0002;
    }
}

