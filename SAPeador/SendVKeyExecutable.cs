using SAPFEWSELib;
using System;

namespace SAPeador
{
	/// <summary>
	/// Sends a virtual key to a window in a session.
	/// </summary>
	public class SendVKeyExecutable : IExecutable
	{
        private InteractionState state = InteractionState.NOT_EXECUTED;
        private string message = string.Empty;
        private bool interruptOnFailure;
		/// <summary>
		/// Virtual key to be sent.
		/// </summary>
        public SAPVirtualKey VKey { get; set; }
		/// <summary>
		/// Item id where the key will be sent to. Must be a window type.
		/// </summary>
        public string ItemPath { get; set; }

        /// <summary>
        /// Sends a virtual key to a window in a session.
        /// </summary>
        /// <param name="vKey">Virtual key to be sent.</param>
        /// <param name="itemPath">Item id where the key will be sent to. Must be a window type. Set to main window by default.</param>
        /// <param name="interruptOnFailure">Whether this particular action stops sequence execution on failure. False by default.</param>
        public SendVKeyExecutable(SAPVirtualKey vKey, string itemPath = "wnd[0]", bool interruptOnFailure = false)
        {
            this.interruptOnFailure = interruptOnFailure;
            VKey = vKey;
            ItemPath = itemPath;
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

            if (string.IsNullOrWhiteSpace(ItemPath))
            {
                SetMessage($"No item path given.");
                return;
            }

            SapItem sapItem = GetSapItemExecutable.Call(session, ItemPath);
            if (sapItem is null)
            {
                SetMessage($"Item with id {ItemPath} could not be read.");
                return;
            }
            if (!sapItem.Type.ToUpper().Contains("WINDOW"))
            {
                SetMessage($"Item with id {ItemPath} and type {sapItem.Type} is not valid for this operation.");
                return;
            }

            try
            {
                GuiFrameWindow frame = sapItem.GetComponent<GuiFrameWindow>(session);
                frame.SendVKey((int)VKey);
            }
            catch (Exception e)
            {
                SetMessage($"Failed to send vkey {VKey} to item with id {ItemPath}. Error: {e.Message}");
                return;
            }

            SetMessage($"VKey {VKey} sent to item with id {ItemPath}.");
            SetState(InteractionState.SUCCESS);
		}
	}

	public enum SAPVirtualKey
	{
		ENTER = 00,
		F1 = 01,
		F2 = 02,
		F3 = 03,
		F4 = 04,
		F5 = 05,
		F6 = 06,
		F7 = 07,
		F8 = 08,
		F9 = 09,
		F10 = 10,
		CTRL_S = 11,
		F12 = 12,
		SHIFT_F1 = 13,
		SHIFT_F2 = 14,
		SHIFT_F3 = 15,
		SHIFT_F4 = 16,
		SHIFT_F5 = 17,
		SHIFT_F6 = 18,
		SHIFT_F7 = 19,
		SHIFT_F8 = 20,
		SHIFT_F9 = 21,
		SHIFT_CTRL_0 = 22,
		SHIFT_F11 = 23,
		SHIFT_F12 = 24,
		CTRL_F1 = 25,
		CTRL_F2 = 26,
		CTRL_F3 = 27,
		CTRL_F4 = 28,
		CTRL_F5 = 29,
		CTRL_F6 = 30,
		CTRL_F7 = 31,
		CTRL_F8 = 32,
		CTRL_F9 = 33,
		CTRL_F10 = 34,
		CTRL_F11 = 35,
		CTRL_F12 = 36,
		CTRL_SHIFT_F1 = 37,
		CTRL_SHIFT_F2 = 38,
		CTRL_SHIFT_F3 = 39,
		CTRL_SHIFT_F4 = 40,
		CTRL_SHIFT_F5 = 41,
		CTRL_SHIFT_F6 = 42,
		CTRL_SHIFT_F7 = 43,
		CTRL_SHIFT_F8 = 44,
		CTRL_SHIFT_F9 = 45,
		CTRL_SHIFT_F10 = 46,
		CTRL_SHIFT_F11 = 47,
		CTRL_SHIFT_F12 = 48,
		CTRL_E = 70,
		CTRL_F = 71,
		CTRL_FORWARDSLASH = 72,
		CTRL_BACKSLASH = 73,
		CTRL_N = 74,
		CTRL_O = 75,
		CTRL_X = 76,
		CTRL_C = 77,
		CTRL_V = 78,
		CTRL_Z = 79,
		CTRL_PAGEUP = 80,
		PAGEUP = 81,
		PAGEDOWN = 82,
		CTRL_PAGEDOWN = 83,
		CTRL_G = 84,
		CTRL_R = 85,
		CTRL_P = 86,
	}
}
