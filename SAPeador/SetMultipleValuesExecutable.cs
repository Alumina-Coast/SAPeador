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

                //if !additive clear table

                int rowCount = 0;

                GuiTableRow row;
                GuiTextField target;
                foreach(var single in SingleValues)
                {
                    row = table.GetAbsoluteRow(rowCount++);
                    target = (GuiTextField)row.ElementAt(1);
                    target.Text = single;
                    table.VerticalScrollbar.Position++;
                    table = (GuiTableControl)wnd.FindById("usr/tabsTAB_STRIP/tabpSIVA/ssubSCREEN_HEADER:SAPLALDB:3010/tblSAPLALDBSINGLE");
                }

                var tab = (GuiTab)wnd.FindById("usr/tabsTAB_STRIP/tabpINTL");
                tab.Select();
                table = (GuiTableControl)wnd.FindById("usr/tabsTAB_STRIP/tabpINTL/ssubSCREEN_HEADER:SAPLALDB:3020/tblSAPLALDBINTERVAL");

                rowCount = 0;
                foreach (var low in LowValues)
                {
                    row = table.GetAbsoluteRow(rowCount++);
                    target = (GuiTextField)row.ElementAt(1);
                    target.Text = low;
                    target = (GuiTextField)row.ElementAt(2);
                    target.Text = HighValues.ElementAt(LowValues.IndexOf(low));
                    table.VerticalScrollbar.Position++;
                    table = (GuiTableControl)wnd.FindById("usr/tabsTAB_STRIP/tabpINTL/ssubSCREEN_HEADER:SAPLALDB:3020/tblSAPLALDBINTERVAL");
                }

                wnd.SendVKey((int)SAPVirtualKey.F8);
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
