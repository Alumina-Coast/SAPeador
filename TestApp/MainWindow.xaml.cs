using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;
using SAPeador;
using dotenv;
using dotenv.net;

namespace TestApp
{
	public class WorkerParams
	{
		public string User { get; set; } = string.Empty;
		public string Password { internal get; set; } = string.Empty;
		public string ConnString { get; set; } = string.Empty;
		public bool UseSso { get; set; } = false;
	}

	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
            DotEnv.Load();
            txtConnString.Text = @Environment.GetEnvironmentVariable("SAP_CONN_STRING");
            txtUser.Text = Environment.UserName;
			if (!Environment.Is64BitProcess)
            {
                Title += " (x86)";
            }
            chkSapGui.IsChecked = SapOperator.CanLoadSapGuiLibrary();
            chkAutoIt.IsChecked = SapOperator.CanLoadAutoItLibrary();
        }

		private void Button_Click(object sender, RoutedEventArgs e)
        {
            txtConnString.IsEnabled = false;
            txtUser.IsEnabled = false;
            txtSecret.IsEnabled = false;
            chkSso.IsEnabled = false;
            var worker = new BackgroundWorker();
			worker.DoWork += Worker_DoWork;
			worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
			worker.RunWorkerAsync(new WorkerParams()
			{
				ConnString = txtConnString.Text,
				User = txtUser.Text,
				Password = txtSecret.Password,
				UseSso = chkSso.IsChecked.Value,
			});
		}

		private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                txbConsole.Text += $" <Error {e.Error.Message}>";
            }
            else if (e.Result == null)
			{
				txbConsole.Text += " <No result>";
			}
			else if (e.Result.GetType() == typeof(ExportGridToExcelExecutable))
			{
				var res = (ExportGridToExcelExecutable)e.Result;
				if (res.GetState() == InteractionState.SUCCESS)
                {
                    WriteBase64File(res.ExportedFile.FileExtension, res.ExportedFile.FileData);
                }
				txbConsole.Text += $" <{res.GetState()}>";
			}
			else
			{
				txbConsole.Text += $" <Unexpected type {e.Result.GetType()}>";
            }
            txtConnString.IsEnabled = true;
            txtUser.IsEnabled = true;
            txtSecret.IsEnabled = true;
            chkSso.IsEnabled = true;
        }

		private void WriteBase64File(string extension, string base64data)
		{
			try
			{
				byte[] fileBytes = Convert.FromBase64String(base64data);
				var filePath = System.IO.Path.Combine(Environment.CurrentDirectory, $"RESULT.{extension}");
                File.WriteAllBytes(filePath, fileBytes);
			}
			catch (Exception e)
			{
				txbConsole.Text += $" <Failed to write file, {e.Message}>";
			}
		}

		private void Worker_DoWork(object sender, DoWorkEventArgs e)
		{
			WorkerParams workerParams = (WorkerParams)e.Argument;
			SapOperator oper = new SapOperator(workerParams.ConnString, workerParams.UseSso);
			var op1a1 = new HideWindowsExecutable();
			var op1a2 = new StartTransactionExecutable("IW28");
			var op1a3 = new SetTextExecutable("wnd[0]/usr/ctxtINGRP-LOW", "DPA");
			var op1a4 = new SendVKeyExecutable(SAPVirtualKey.F8);
			var op1a5 = new ExportGridToExcelExecutable("wnd[0]/usr/cntlGRID1/shellcont/shell");
			var seq1 = new Sequence(workerParams.User, workerParams.Password);
			seq1.Actions.Add(op1a1);
			seq1.Actions.Add(op1a2);
			seq1.Actions.Add(op1a3);
			seq1.Actions.Add(op1a4);
			seq1.Actions.Add(op1a5);
			oper.PlaySequence(seq1);
			e.Result = op1a5;
        }
	}
}
