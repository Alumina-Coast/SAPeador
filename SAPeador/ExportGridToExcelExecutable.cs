using AutoItX3Lib;
using SAPFEWSELib;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CSharp;

namespace SAPeador
{
    public class DocumentExport
    {
        public string FileExtension { get; set; } = string.Empty;
        public string FileData { get; set; } = string.Empty;
    }

    public class ExportGridToExcelExecutable : IExecutable
	{
		private static readonly object _lockObject = new object();
        private InteractionState state = InteractionState.NOT_EXECUTED;
        private string message = string.Empty;
        private bool interruptOnFailure;
        public string ItemPath { get; set; }
        public DocumentExport ExportedFile { get; private set; } = new DocumentExport();
        private readonly string _fileExtension = "XLSX";

        public ExportGridToExcelExecutable(string itemPath, bool interruptOnFailure = false)
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
            if (!sapItem.Type.ToUpper().Contains("SHELL"))
            {
                SetMessage($"Item with id {ItemPath} and type {sapItem.Type} is not valid for this operation.");
                return;
            }

            try
            {
                var grid = (GuiGridView)session.FindById(ItemPath);
                grid.ContextMenu();
                grid.SelectContextMenuItem("&XXL");

                var fileName = Guid.NewGuid().ToString();
                var filePath = Path.Combine(Path.GetTempPath(), $"{fileName}.{_fileExtension}");

                lock (_lockObject)
                {
                    var button = (GuiButton)session.FindById("wnd[1]/tbar[0]/btn[0]");

                    var cts = new CancellationTokenSource();
                    Task.Run(() => HandleSaveAsWindow(filePath,cts.Token));
					button.Press();
                    cts.Cancel();

                    if (!File.Exists(filePath))
                    {
                        SetMessage($"Could not create exported file for {ItemPath}.");
                        return; 
                    }
                    byte[] fileBytes = File.ReadAllBytes(filePath);
                    var fileData = Convert.ToBase64String(fileBytes);

                    ExportedFile = new DocumentExport() { FileData = fileData, FileExtension = _fileExtension };

                    File.Delete(filePath);
                }

                SetMessage($"{ItemPath} succesfully exported.");
                SetState(InteractionState.SUCCESS);
            }
            catch (Exception e)
            {
                SetMessage($"Failed to export {ItemPath}. Error: {e.Message}");
            }
        }

        private async Task HandleSaveAsWindow(string filePath, CancellationToken cancellationToken)
        {
            var autoIt = new AutoItX3();
            var handleId = string.Empty;
            while (handleId == string.Empty && !cancellationToken.IsCancellationRequested)
            {
                object[,] list = autoIt.WinList("[CLASS:#32770]");
                for (int i = 0; i < list.Length/2; i++)
                {
                    var hwnd = list[1, i];
                    if (hwnd is null) { continue; }
                    if (autoIt.ControlShow($"[HANDLE:{hwnd}]", "", "[ID:1148]") != 0)
                    {
                        handleId = $"[HANDLE:{hwnd}]";
                        break;
                    }
                }
            }
            while (autoIt.ControlSetText(handleId, "", "[ID:1148]", filePath) == 0 && !cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(100, cancellationToken);
            }
            while (autoIt.ControlClick(handleId, "", "[CLASS:Button; INSTANCE:2]") == 0 && !cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(100, cancellationToken);
            }

            while (autoIt.WinExists("[CLASS:#32770]") == 0 && !cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(100, cancellationToken);
            }
            var hwndConfirm = autoIt.WinGetHandle("[CLASS:#32770]");
            autoIt.WinActivate(hwndConfirm);
            while (autoIt.ControlClick(hwndConfirm, "", "[CLASS:Button; INSTANCE:1]") == 0 && !cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(100, cancellationToken);
            }
        }
	}
}
