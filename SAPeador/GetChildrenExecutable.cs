using SAPFEWSELib;
using System;
using System.Collections.Generic;

namespace SAPeador
{
    /// <summary>
    /// Recovers the data from the children of a SAP container.
    /// </summary>
    public class GetChildrenExecutable : IExecutable
    {
        private InteractionState state = InteractionState.NOT_EXECUTED;
        private string message = string.Empty;
        /// <summary>
        /// List with the found <see cref="SapItem"/>s.
        /// </summary>
        public List<SapItem> Children { get; private set; } = new List<SapItem>();
        /// <summary>
        /// Id for the container to read.
        /// </summary>
        public string ItemPath { get; set; }
        private bool interruptOnFailure;

		/// <summary>
		/// Recovers the data from the children of a SAP container.
		/// </summary>
		/// <param name="itemPath">Id for the container to read.</param>
		/// <param name="interruptOnFailure">Whether this particular action stops sequence execution on failure. False by default.</param>
		public GetChildrenExecutable(string itemPath, bool interruptOnFailure = false)
        {
            ItemPath = itemPath;
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

            var sapItem = SapItem.Get(session, ItemPath);
            if (sapItem is null)
            {
                SetMessage($"Item with id {ItemPath} could not be read.");
                return;
            }
            if (!sapItem.IsContainer)
            {
                SetMessage($"Item with id {ItemPath} is not a container.");
                return;
            }

            var rootChildrenIds = new List<string>();
            try
            {
                GuiComponentCollection rootChildren;
                try
                {
                    rootChildren = sapItem.GetComponent<GuiVContainer>(session).Children;
                }
                catch
                {
                    rootChildren = sapItem.GetComponent<GuiContainer>(session).Children;
                }
                foreach (GuiComponent child in rootChildren)
                {
                    rootChildrenIds.Add(child.Id);
                }
            }
            catch (Exception e)
            {
                SetMessage($"Failed to read {ItemPath}'s children. Error: {e.Message}");
                return;
            }

            foreach (string childId in rootChildrenIds)
            {
                var child = SapItem.Get(session,childId);
                if (child != null)
                {
                    Children.Add(child);
                }
            }

            SetMessage($"Children for {ItemPath} read.");
            SetState(InteractionState.SUCCESS);
        }
    }
}
