using AutoItX3Lib;
using SAPFEWSELib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace SAPeador
{
    public enum SapConditions
    {
        PATTERN = 7,
        EXCLUDE_PATTERN = 6,
        EQUAL = 5,
        GREATER_OR_EQUAL_THAN = 4,
        LESS_OR_EQUAL_THAN = 3,
        GREATER_THAN = 2,
        LESS_THAN = 1,
        NOT_EQUAL = 0,
    }

    /// <summary>
    /// Sets a condition (NOT_EQUAL, LESS_THAN, and such) on a field.
    /// </summary>
    public class SetConditionalExecutable : IExecutable
    {
        private InteractionState state = InteractionState.NOT_EXECUTED;
        private string message = string.Empty;
        private bool interruptOnFailure;
        /// <summary>
        /// Condition to be set, must be one of SapCondition.
        /// </summary>
        public SapConditions Condition { get; set; }
        /// <summary>
        /// Id for the target field.
        /// </summary>
        public string ItemPath { get; set; }

        /// <summary>
        /// Sets a condition (NOT_EQUAL, LESS_THAN, and such) on a field.
        /// </summary>
        /// <param name="itemPath">Id for the target field.</param>
        /// <param name="condition">Condition to be set, must be one of SapCondition.</param>
        /// <param name="interruptOnFailure">Whether this particular action stops sequence execution on failure. False by default.</param>
        public SetConditionalExecutable(string itemPath, SapConditions condition, bool interruptOnFailure = false)
        {
            ItemPath = itemPath;
            Condition = condition;
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

        // We are using AutoIt in this because I couldn't find a nice way of selecting the conditon.
        // The container for the conditions is a custom one, so it does not have row selection methods.
        // TODO: Add verification that the right condition was set or fail. (check if possible)
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

            try
            {
                var item = (GuiVComponent)session.FindById(ItemPath);
                item.ShowContextMenu();
                item = null;
                var usr = (GuiUserArea)session.FindById("wnd[0]/usr");
                usr.SelectContextMenuItemByPosition("0");
                usr = null;
                var wnd = (GuiFrameWindow)session.Children.ElementAt(session.Children.Count - 1);
                var shell = (GuiShell)wnd.FindById("usr/cntlOPTION_CONTAINER/shellcont/shell");
                shell.SetFocus();
                shell = null;

                var autoIt = new AutoItX3();
                var handleId = string.Empty;
                while (handleId == string.Empty)
                {
                    object[,] list = autoIt.WinList("[CLASS:#32770]");
                    for (int i = 0; i < list.Length / 2; i++)
                    {
                        var hwnd = list[1, i];
                        if (hwnd is null) { continue; }
                        if (autoIt.ControlShow($"[HANDLE:{hwnd}]", "", "[CLASS:SAPALVGrid; INSTANCE:1]") != 0)
                        {
                            handleId = $"[HANDLE:{hwnd}]";
                            break;
                        }
                    }
                }

                autoIt.WinActivate(handleId);
                autoIt.ControlFocus(handleId, "", "[ID:1148]");
                autoIt.ControlSend(handleId, "", "", "{DOWN 20}");
                autoIt.ControlSend(handleId, "", "", "{UP " + (int)Condition + "}"); 
                var btn = (GuiButton)wnd.FindById("tbar[0]/btn[0]");
                btn.Press();
                btn = null;
                wnd = null;
            }
            catch (Exception e)
            {
                SetMessage($"Failed to set condition {Condition} on item with id {ItemPath}. Error: {e.Message}");
                return;
            }

            SetMessage($"Condition '{Condition}' written on item with id {ItemPath}.");
            SetState(InteractionState.SUCCESS);
        }
    }
}
