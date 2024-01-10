using SAPFEWSELib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPeador
{
    public class SetConditionalExecutable : IExecutable
    {
        private InteractionState state = InteractionState.NOT_EXECUTED;
        private string message = string.Empty;
        private bool interruptOnFailure;
        public string Value { get; set; }
        public string ItemPath { get; set; }

        public SetConditionalExecutable(string itemPath, string value)
        {
            ItemPath = itemPath;
            Value = value;
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

        }
    }
}
