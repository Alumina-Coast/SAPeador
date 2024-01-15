using SAPFEWSELib;
using System;

namespace SAPeador
{
    /// <summary>
    /// Puts a given text in a text field.
    /// </summary>
	public class SetTextExecutable : IExecutable
	{
        private InteractionState state = InteractionState.NOT_EXECUTED;
        private string message = string.Empty;
        /// <summary>
        /// Previous text in the field.
        /// </summary>
        public string PreviousText { get; private set; } = string.Empty;
        /// <summary>
        /// Id for the target text field.
        /// </summary>
        public string ItemPath { get; set; }
        /// <summary>
        /// Text to write on the text field.
        /// </summary>
        public string Text { get; set; }
		private bool interruptOnFailure;

		/// <summary>
		/// Puts a given text in a text field.
		/// </summary>
		/// <param name="itemPath">Id for the target text field.</param>
		/// <param name="text">Text to write on the text field.</param>
		/// <param name="interruptOnFailure">Whether this particular action stops sequence execution on failure. False by default.</param>
		public SetTextExecutable(string itemPath, string text, bool interruptOnFailure = false)
        {
            ItemPath = itemPath;
            Text = text;
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

            if (string.IsNullOrWhiteSpace(ItemPath))
            {
                SetMessage($"No item path given.");
                return;
            }

            var sapItem = GetSapItemExecutable.Call(session, ItemPath);
			if (sapItem is null) 
			{
                SetMessage($"Item with id {ItemPath} could not be read.");
				return;
			}
			if (!sapItem.Changeable || !sapItem.Type.Contains("TextField"))
            {
                SetMessage($"Item with id {ItemPath} and type {sapItem.Type} is not valid for this operation.");
                return;
			}

			try
            {
                var item = sapItem.GetComponent<GuiTextField>(session);
                PreviousText = item.Text;
                item.Text = Text;
            }
			catch (Exception e)
            {
                SetMessage($"Failed to read {ItemPath}. Error: {e.Message}");
                return;
            }

            SetMessage($"Value '{Text}' written on item with id {ItemPath}.");
            SetState(InteractionState.SUCCESS);
		}
	}
}
