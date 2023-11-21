using System.Collections.Generic;

namespace SAPeador
{
    public enum SequenceState
    {
        QUEUED,
        IN_PROCESS,
        INTERRUPTED,
        COMPLETED,
    }

    public class Sequence
    {
        public string User { get; set; }
        public string Secret { internal get; set; }
        public string SessionId { get; set; }
        public bool KeepAlive { get; set; }
        public bool KeepAliveOnFailure { get; set; }
        public bool InterruptOnFailure { get; set; }
        public SequenceState State { get; internal set; } = SequenceState.QUEUED;
        public List<IExecutable> Actions { get; set; } = new List<IExecutable>();

        public Sequence(string user, string secret = "", string sessionId = "", bool keepAlive = false, bool keepAliveOnFailure = false, bool interruptOnFailure = true)
        {
            User = user;
            Secret = secret;
            SessionId = sessionId;
            KeepAlive = keepAlive;
            InterruptOnFailure = interruptOnFailure;
            KeepAliveOnFailure = keepAliveOnFailure;
        }
    }
}