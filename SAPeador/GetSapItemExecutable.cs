using SAPFEWSELib;
using System;
using System.Linq;
using static System.Collections.Specialized.BitVector32;

namespace SAPeador
{
	/// <summary>
	/// Searches for a SAP component and recover all of its general data for identification.
	/// </summary>
	public class GetSapItemExecutable : IExecutable
	{
        private InteractionState state = InteractionState.NOT_EXECUTED;
        private string message = string.Empty;
        private bool interruptOnFailure;
		/// <summary>
		/// Item found along with its data. Returns null if nothing was found or unable to read.
		/// </summary>
        public SapItem Value { get; set; } = null;
		/// <summary>
		/// Id for the item to find and read.
		/// </summary>
		public string ItemPath { get; set; }

		/// <summary>
		/// Searches for a SAP component and recover all of its general data for identification.
		/// </summary>
		/// <param name="itemPath">Id for the item to find and read.</param>
		/// <param name="interruptOnFailure">Whether this particular action stops sequence execution on failure. False by default.</param>
		public GetSapItemExecutable(string itemPath, bool interruptOnFailure = false)
        {
            ItemPath = itemPath;
            this.interruptOnFailure = interruptOnFailure;
        }

        public InteractionState GetState()
        {
            return state;
        }

        public void SetState(InteractionState value)
        {
            state = value;
        }

        public string GetMessage()
        {
            return message;
        }

        public void SetMessage(string value)
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

			Value = SapItem.Get(session, ItemPath);

			if (Value is null)
			{
                SetMessage($"Could not read an item with id {ItemPath}.");
				return;
			}

            SetMessage($"Item with id {ItemPath} succesfully read.");
            SetState(InteractionState.SUCCESS);
		}
	}
}
