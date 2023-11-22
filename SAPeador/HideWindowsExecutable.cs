using SAPFEWSELib;
using System;

namespace SAPeador
{
    /// <summary>
    /// Minimizes all windows related to the session.
    /// </summary>
	public class HideWindowsExecutable : IExecutable
	{
        private InteractionState state = InteractionState.NOT_EXECUTED;
        private string message = string.Empty;
        private bool interruptOnFailure;

		/// <summary>
		/// Minimizes all windows related to the session.
		/// </summary>
		/// <param name="interruptOnFailure">Whether this particular action stops sequence execution on failure. False by default.</param>
		public HideWindowsExecutable(bool interruptOnFailure = false)
        {
            this.interruptOnFailure = interruptOnFailure;
        }

        public InteractionState GetState()
        {
            return state;
        }

        private void SetState(InteractionState value)
        {
            state = value;
        }

        public string GetMessage()
        {
            return message;
        }

        private void SetMessage(string value)
        {
            message = value;
        }

        public bool GetInterruptOnFailure()
        {
            return interruptOnFailure;
        }

        public void SetInterruptOnFailure(bool value)
        {
            interruptOnFailure = value;
        }

        void IExecutable.Execute(GuiSession session)
		{
            SetState(InteractionState.FAILURE);
			try
			{
                var frame = (GuiFrameWindow)session.Children.Item(0);
                frame.Iconify();
                SetMessage($"Windows hidden.");
            }
            catch (Exception e)
            {
                SetMessage($"Failed to minimize windows. Error: {e.Message}");
                return;
            }
            SetState(InteractionState.SUCCESS);
		}
	}
}
