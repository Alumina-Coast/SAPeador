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

    /// <summary>
    /// Represents a sequence of actions to be executed in a specific context.
    /// Provide the right credentials according to your SAP connection string.
    /// </summary>
    public class Sequence
    {
        public string User { get; set; }
        public string Secret { internal get; set; }
        public string SessionId { get; set; }
        public bool KeepAlive { get; set; }
        public bool KeepAliveOnFailure { get; set; }
        public bool InterruptOnFailure { get; set; }
        public SequenceState State { get; internal set; } = SequenceState.QUEUED;
        /// <summary>
        /// Actions in this list will be executed in order.
        /// </summary>
        public List<IExecutable> Actions { get; set; } = new List<IExecutable>();

        /// <summary>
        /// Initializes a new instance of the <see cref="Sequence"/> class.
        /// </summary>
        /// <param name="user">The user identifier for the SAP Client.</param>
        /// <param name="secret">Password for the SAP User. Needed only when logging in without SSO.</param>
        /// <param name="sessionId">Id for the session the sequence is running on. Provide one only if you know it is active and belongs to the User.</param>
        /// <param name="keepAlive">Whether to keep the session alive after the sequence completes. False by default.</param>
        /// <param name="keepAliveOnFailure">Whether to keep the session alive on failure. False by default.</param>
        /// <param name="interruptOnFailure">Whether to interrupt the sequence on failure. True by default.</param>
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