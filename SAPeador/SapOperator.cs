using SAPeador.Exceptions;
using SAPFEWSELib;
using System;
using System.Collections.Generic;
using System.Threading;
using AutoItX3Lib;

namespace SAPeador
{
	/// <summary>
	/// Manages the execution of sequences of actions within the SAP GUI Client.
	/// This class is responsible for establishing SAP sessions and executing actions defined in the Sequence.
	/// </summary>
	public class SapOperator
	{
		private readonly string _sapConnectionString;
		private readonly bool _useSso;
		private static readonly object _lockObject = new object();

		/// <summary>
		/// Initializes a new instance of the <see cref="SapOperator"/> class.
		/// </summary>
		/// <param name="sapConnectionString">The connection string to establish connection with the SAP Client.</param>
		/// <param name="useSso">Specifies whether if Single Sign-On is expected for authentication. False by default.</param>
		public SapOperator(string sapConnectionString, bool useSso = false)
		{
			_sapConnectionString = sapConnectionString;
			_useSso = useSso;
        }

        public static bool CanLoadSapGuiLibrary()
        {
            try
            {
                var rotInstance = new SapROTWr.CSapROTWrapper();
                var appInstance = new GuiApplication();
                return true;
            }
			catch { }
            return false;
        }

        public static bool CanLoadAutoItLibrary()
        {
            try
            {
                var autoItInstance = new AutoItX3();
                return true;
            }
            catch { }
            return false;
		}

		/// <summary>
		/// Executes the actions from the sequence in the corresponding session and under the flags set in the sequence object.
		/// </summary>
		/// <param name="sequence">The sequence to be played on the SAP GUI Client.</param>
		/// <exception cref="SapSessionException">Thrown when unable to get a new or particular session.</exception>
		/// <exception cref="SapApplicationException">Thrown when unable to use the SAP GUI Client.</exception>
		public void PlaySequence(Sequence sequence)
		{
			sequence.State = SequenceState.IN_PROCESS;
			GuiSession session;
			if (string.IsNullOrWhiteSpace(sequence.SessionId))
			{
				session = GetNewSession(sequence.User, sequence.Secret);
			}
			else
			{
				session = GetSession(sequence.User, sequence.SessionId);
			}
			if (session is null)
			{
				return;
			}
			sequence.SessionId = session.Id;
			foreach (IExecutable action in sequence.Actions)
			{
				action.Execute(session);
				if ((sequence.InterruptOnFailure || action.GetInterruptOnFailure()) && action.GetState() == InteractionState.FAILURE)
				{
					sequence.State = SequenceState.INTERRUPTED;
					break;
				}
			}
			if (!sequence.KeepAliveOnFailure && sequence.State == SequenceState.INTERRUPTED)
			{
				CloseSession(session);
				return;
			}
			if (!sequence.KeepAlive && sequence.State == SequenceState.IN_PROCESS)
			{
				CloseSession(session);
			}
			if (sequence.State == SequenceState.IN_PROCESS)
			{
				sequence.State = SequenceState.COMPLETED;
			}
			// if there's any need to do more state logic refactor everything to a state machine first
		}

		private GuiApplication GetEngine()
		{
			object engine, sapGuiRot = null;
			try
			{
				var sapROTWrapper = new SapROTWr.CSapROTWrapper();
				sapGuiRot = sapROTWrapper.GetROTEntry("SAPGUI");
            }
            catch (Exception e)
            {
                throw new SapApplicationException($"Failed to instantiate scripting engine. Error: {e.Message}");
            }
            engine = sapGuiRot?.GetType().InvokeMember("GetScriptingEngine", System.Reflection.BindingFlags.InvokeMethod, null, sapGuiRot, null);
			if (engine is null)
			{
				throw new SapApplicationException($"Could not instantiate scripting engine.");
            }
            try
            {
                var app = (GuiApplication)engine;
                return app;
            }
            catch
            {
                throw new SapApplicationException($"Could not instantiate scripting engine as GuiApplication.");
            }
		}

		private GuiSession GetNewSession(string user, string secret = "")
        {
            if (user.ToUpper() != Environment.UserName.ToUpper() && _useSso)
            {
                throw new SapSessionException($"'{user}' is not the current user for SSO.");
            }
			if (!_useSso && (string.IsNullOrWhiteSpace(secret) || string.IsNullOrWhiteSpace(user)))
            {
                throw new SapSessionException($"Credentials required.");
            }

            GuiApplication app = GetEngine();
            GuiSession newSession;

            lock (_lockObject)
			{
				foreach (GuiConnection connection in app.Connections)
				{
					var someSession = (GuiSession)connection.Sessions.Item(0);
					if (someSession.Info.User.ToUpper() == user.ToUpper())
					{
						List<string> existingIds = new List<string>();
						foreach (GuiSession session in connection.Sessions)
						{
							existingIds.Add(session.Id);
						}
						someSession.CreateSession();
						var timeout = DateTime.Now + TimeSpan.FromSeconds(10);
						while (DateTime.Now < timeout)
						{
							foreach (GuiSession session in connection.Sessions)
							{
								if (!existingIds.Contains(session.Id))
								{
									return session;
								}
							}
							Thread.Sleep(10);
						}
						throw new SapSessionException($"Could not create a new session in existing connection for user '{user}'.");
					}
                }
                try
                {
                    var conn = app.OpenConnectionByConnectionString(_sapConnectionString);
                    newSession = (GuiSession)conn.Sessions.Item(0);
                }
                catch (Exception e)
                {
                    throw new SapSessionException($"Failed to create session for user '{user}'. Error: {e.Message}");
                }
                if (!_useSso)
				{
                    try
                    {
						if (newSession.Info.Transaction == "S000")
                        {
                            GuiTextField usernameField = (GuiTextField)newSession.FindById("wnd[0]/usr/txtRSYST-BNAME");
                            GuiTextField passwordField = (GuiTextField)newSession.FindById("wnd[0]/usr/pwdRSYST-BCODE");
                            GuiFrameWindow frame = (GuiFrameWindow)newSession.FindById("wnd[0]");
                            usernameField.Text = user;
                            passwordField.Text = secret;
                            frame.SendVKey((int)SAPVirtualKey.ENTER);
                        }
                    }
                    catch { }
                }
            }
            if (newSession.Info.User.ToUpper() != user.ToUpper())
			{
				var connection = (GuiConnection)newSession.Parent;
                // Close session uses the lock object, do NOT call inside a lock
                connection?.CloseSession(newSession.Id);
                throw new SapSessionException($"Could not create a new session for user '{user}'.");
			}
            return newSession;
        }

        private GuiSession GetSession(string user, string sessionId)
        {
            GuiApplication app = GetEngine();
            foreach (GuiConnection connection in app.Connections)
            {
                var someSession = (GuiSession)connection.Sessions.Item(0);
                if (someSession.Info.User.ToUpper() == user.ToUpper())
                {
                    foreach (GuiSession session in connection.Sessions)
                    {
						if (session.Id == sessionId)
						{
							return session;
						}
                    }
                }
            }
			throw new SapSessionException($"User '{user}' has no session with id {sessionId}.");
		}

        private static void CloseSession(GuiSession session)
        {
			lock (_lockObject)
			{
				try
				{
					var conn = (GuiConnection)session.Parent;
					conn.CloseSession(session.Id);
				}
				catch
				{
					return;
				}
			}
		}
	}
}