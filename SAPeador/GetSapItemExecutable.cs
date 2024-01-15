using SAPFEWSELib;
using System;
using System.Linq;
using static System.Collections.Specialized.BitVector32;

namespace SAPeador
{
	/// <summary>
	/// Contains all the fields you would encounter in a SAP component class.
	/// It helps identify what kind of component are you dealing with before directly interacting with it.
	/// The Id field is particularly useful since it's what most <see cref="IExecutable"/>s will be using to identify components.
	/// </summary>
	public class SapItem
    {
		public string WindowId { get; set; } = string.Empty;
        public string Id { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public int Left { get; set; } = 0;
        public int Top { get; set; } = 0;
        public string IconName { get; set; } = string.Empty;
        public bool Changeable { get; set; } = false;
        public bool IsContainer { get; set; } = false;

        public T GetComponent<T>(GuiSession session)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(WindowId))
                {
                    return (T)session.FindById(Id);
                }
                else
                {
                    return (T)((GuiFrameWindow)session.FindById(WindowId)).FindById(Id);
                }
            }
            catch
            {
                return default;
            }
        }
    }

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
				var breakpoint = "wnd[";
				if (itemPath.Contains(breakpoint) && itemPath.Length > itemPath.LastIndexOf(breakpoint) + breakpoint.Length + 3)
                {
                    itemPath = itemPath.Substring(itemPath.LastIndexOf(breakpoint) + breakpoint.Length + 3);
                }
				GuiComponent item = null;
                string wndId = string.Empty;
                foreach (GuiFrameWindow wnd in session.Children)
                {
					try
                    {
                        item = wnd.FindById(itemPath);
                        wndId = wnd.Id.Split('/').Last();
                        break;
                    }
					catch { }
                }
				if (item is null)
                {
                    try
                    {
                        item = session.FindById(itemPath);
                    }
                    catch { }
                }
                // only update itemPath if its a child of a window (x/wnd[n]/...)
                if (item.Id.Contains(breakpoint) && itemPath.Length > itemPath.LastIndexOf(breakpoint) + breakpoint.Length + 3)
                {
                    itemPath = item.Id.Substring(item.Id.LastIndexOf(breakpoint) + breakpoint.Length + 3);
                }
				var sapItem = new SapItem()
				{
					WindowId = wndId,
					Name = item.Name,
					Type = item.Type,
					Id = itemPath,
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
