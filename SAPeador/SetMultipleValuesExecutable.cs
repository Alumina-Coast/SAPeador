using SAPFEWSELib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters;
using System.Text;
using System.Threading;
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
        public bool Additive { get; set; }
        public string ItemPath { get; set; }

        public SetMultipleValuesExecutable(string itemPath, List<string> singleValues, List<string> lowValues, List<string> highValues, bool additive = true)
        {
            ItemPath = itemPath;
            SingleValues = singleValues;
            LowValues = lowValues;
            HighValues = highValues;
            Additive = additive;
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
                var component = (GuiVComponent)session.FindById(ItemPath);
                component.ShowContextMenu();
                component = null;
                var usr = (GuiUserArea)session.FindById("wnd[0]/usr");
                usr.SelectContextMenuItemByPosition("1");
                usr = null;
                var wnd = (GuiFrameWindow)session.Children.ElementAt(session.Children.Count - 1);

                var table = (GuiTableControl)wnd.FindById("usr/tabsTAB_STRIP/tabpSIVA/ssubSCREEN_HEADER:SAPLALDB:3010/tblSAPLALDBSINGLE");

                if (!Additive)
                {
                    wnd.SendVKey((int)SAPVirtualKey.SHIFT_F4);
                }

                int rowCount = 0;

                GuiTableRow row;
                GuiTextField target;
                foreach(var single in SingleValues)
                {
                    while (true)
                    {
                        row = table.GetAbsoluteRow(rowCount++);
                        target = (GuiTextField)row.ElementAt(1);
                        if (string.IsNullOrWhiteSpace(target.Text))
                        {
                            target.Text = single;
                            break;
                        }
                        table.VerticalScrollbar.Position++;
                        table = (GuiTableControl)wnd.FindById("usr/tabsTAB_STRIP/tabpSIVA/ssubSCREEN_HEADER:SAPLALDB:3010/tblSAPLALDBSINGLE");
                    }
                    table.VerticalScrollbar.Position++;
                    table = (GuiTableControl)wnd.FindById("usr/tabsTAB_STRIP/tabpSIVA/ssubSCREEN_HEADER:SAPLALDB:3010/tblSAPLALDBSINGLE");
                }
                target = null;

                var tab = (GuiTab)wnd.FindById("usr/tabsTAB_STRIP/tabpINTL");
                tab.Select();
                tab = null;
                table = (GuiTableControl)wnd.FindById("usr/tabsTAB_STRIP/tabpINTL/ssubSCREEN_HEADER:SAPLALDB:3020/tblSAPLALDBINTERVAL");

                rowCount = 0;
                GuiTextField targetLow;
                GuiTextField targetHigh;
                foreach (var low in LowValues)
                {
                    while (true)
                    {
                        row = table.GetAbsoluteRow(rowCount++);
                        targetLow = (GuiTextField)row.ElementAt(1);
                        targetHigh = (GuiTextField)row.ElementAt(2);
                        if (string.IsNullOrWhiteSpace(targetLow.Text) && string.IsNullOrWhiteSpace(targetHigh.Text))
                        {
                            targetLow.Text = low;
                            targetHigh.Text = HighValues.ElementAt(LowValues.IndexOf(low));
                            break;
                        }
                        table.VerticalScrollbar.Position++;
                        table = (GuiTableControl)wnd.FindById("usr/tabsTAB_STRIP/tabpINTL/ssubSCREEN_HEADER:SAPLALDB:3020/tblSAPLALDBINTERVAL");
                    }
                    table.VerticalScrollbar.Position++;
                    table = (GuiTableControl)wnd.FindById("usr/tabsTAB_STRIP/tabpINTL/ssubSCREEN_HEADER:SAPLALDB:3020/tblSAPLALDBINTERVAL");
                }
                targetLow = null;
                targetHigh = null;

                row = null;
                table = null;

                wnd.SendVKey((int)SAPVirtualKey.F8);
                wnd = null;
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
