using SAPFEWSELib;
using System;
using System.Collections.Generic;

namespace SAPeador
{
    public class GetChildrenExecutable : IExecutable
    {
        private InteractionState state = InteractionState.NOT_EXECUTED;
        private string message = string.Empty;
        public List<SapItem> Children { get; private set; } = new List<SapItem>();
        public string ItemPath { get; set; }
        private bool interruptOnFailure;

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

            SapItem sapItem = GetSapItemExecutable.Call(session, ItemPath);
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
                    rootChildren = ((GuiVContainer)session.FindById(ItemPath)).Children;
                }
                catch
                {
                    rootChildren = ((GuiContainer)session.FindById(ItemPath)).Children;
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
                SapItem child = GetSapItemExecutable.Call(session,childId);
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
