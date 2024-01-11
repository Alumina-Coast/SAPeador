using SAPFEWSELib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace SAPeador
{
    public class SetMultipleValuesExecutable : IExecutable
    {
        private InteractionState state = InteractionState.NOT_EXECUTED;
        private string message = string.Empty;
        private bool interruptOnFailure;
        public List<string> SingleValues { get; set; }
        public List<string> LowValues { get; set; }
        public List<string> HighValues { get; set; }
        public string ItemPath { get; set; }

        public SetMultipleValuesExecutable(string itemPath, List<string> singleValues, List<string> lowValues, List<string> highValues)
        {
            ItemPath = itemPath;
            SingleValues = singleValues;
            LowValues = lowValues;
            HighValues = highValues;
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
                //var item = (GuiTextField)session.FindById(ItemPath);
                //PreviousText = item.Text;
                //item.Text = Text;
            }
            catch (Exception e)
            {
                SetMessage($"Failed to read {ItemPath}. Error: {e.Message}");
                return;
            }

            SetMessage($"Value  written on item with id {ItemPath}.");
            SetState(InteractionState.SUCCESS);
        }
    }
}
