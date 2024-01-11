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

    public class SetConditionalExecutable : IExecutable
    {
        private InteractionState state = InteractionState.NOT_EXECUTED;
        private string message = string.Empty;
        private bool interruptOnFailure;
        public SapConditions Condition { get; set; }
        public string ItemPath { get; set; }
        public bool ExcludeSelection { get; set; }

        public SetConditionalExecutable(string itemPath, SapConditions condition, bool excludeSelection = false)
        {
            ItemPath = itemPath;
            Condition = condition;
            ExcludeSelection = excludeSelection;
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

            try
            {
                var item = (GuiVComponent)session.FindById(ItemPath);
                item.ShowContextMenu();
                item = null;
                var usr = (GuiUserArea)session.FindById("wnd[0]/usr");
                usr.SelectContextMenuItem("&006");
                usr = null;
                var wnd = (GuiFrameWindow)session.Children.ElementAt(session.Children.Count - 1);
                var shell = (GuiShell)wnd.FindById("usr/cntlOPTION_CONTAINER/shellcont/shell");
                shell.SetFocus();
                wnd = null;
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
                var btn = (GuiButton)session.FindById("wnd[1]/tbar[0]/btn[0]");
                btn.Press();
                btn = null;
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
