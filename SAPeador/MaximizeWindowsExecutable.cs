using SAPFEWSELib;
using System;

namespace SAPeador
{
	public class MaximizeWindowsExecutable : IExecutable
	{
        private InteractionState state = InteractionState.NOT_EXECUTED;
        private string message = string.Empty;
        private bool interruptOnFailure;

        public MaximizeWindowsExecutable(bool interruptOnFailure = false) 
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
                frame.Maximize();
                SetMessage($"Windows maximized.");
            }
            catch (Exception e)
            {
                SetMessage($"Failed to maximize windows. Error: {e.Message}");
                return;
            }
            SetState(InteractionState.SUCCESS);
		}
	}
}
