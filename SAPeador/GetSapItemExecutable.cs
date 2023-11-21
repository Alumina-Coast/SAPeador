using SAPFEWSELib;

namespace SAPeador
{
    public class SapItem
    {
        public string Id { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public int Left { get; set; } = 0;
        public int Top { get; set; } = 0;
        public string IconName { get; set; } = string.Empty;
        public bool Changeable { get; set; } = false;
        public bool IsContainer { get; set; } = false;
    }

    internal class GetSapItemExecutable : IExecutable
	{
        private InteractionState state = InteractionState.NOT_EXECUTED;
        private string message = string.Empty;
        private bool interruptOnFailure;
        public SapItem Value { get; set; } = null;
        public string ItemPath { get; set; }

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

			Value = Call(session, ItemPath);

			if (Value is null)
			{
                SetMessage($"Could not read an item with id {ItemPath}.");
				return;
			}

            SetMessage($"Item with id {ItemPath} succesfully read.");
            SetState(InteractionState.SUCCESS);
		}

		internal static SapItem Call(GuiSession session, string itemPath)
		{
			try
			{
				var item = session.FindById(itemPath);
				var sapItem = new SapItem()
				{
					Name = item.Name,
					Type = item.Type,
					Id = item.Id,
					IsContainer = item.ContainerType,
				};
				try
				{
					var vItem = (GuiVComponent)item;
					sapItem.Changeable = vItem.Changeable;
					sapItem.Text = vItem.Text;
					sapItem.Top = vItem.Top;
					sapItem.Left = vItem.Left;
					sapItem.IconName = vItem.IconName;
				}
				catch { }
				return sapItem;
			}
			catch
			{
				return null;
			}
		}
	}
}
