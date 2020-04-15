using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using CheckBox = System.Windows.Controls.CheckBox;
using Color = System.Drawing.Color;

namespace Projet_Final_CS___Music_Player
{
	public delegate void OptionsChanged(object sender, EventArgs e);

	/// <summary>
	/// App options
	/// </summary>
	public partial class Options : Window
	{
		public event OptionsChanged optionsChanged;

		public Color color;

		public bool colorChanged = false;
		public bool trayChanged = false;
		public bool dynamicChanged = false;
		public bool pathChanged = false;

		public string path { get; set; }

		public Options()
		{
			InitializeComponent();

			Microsoft.Win32.RegistryKey key;
			if ((key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software\\CSMP3Player", true)) == null)
			{
				key.SetValue("InTray", true);
				key.SetValue("SaveDirectory", System.IO.Path.Combine(Environment.CurrentDirectory));
				key.Close();
				trayCheck.IsChecked = true;
			}

			if (key.GetValue("InTray") == null)
				key.SetValue("InTray", true);

			if (key.GetValue("SaveDirectory") == null)
				key.SetValue("SaveDirectory", System.IO.Path.Combine(Environment.CurrentDirectory));

			bool val = Convert.ToBoolean(key.GetValue("InTray"));
			path = Convert.ToString(key.GetValue("SaveDirectory"));
			trayCheck.IsChecked = val;
			directoryTxt.Text = path;
		}

		private void CheckBox_Click(object sender, RoutedEventArgs e)
		{
			CheckBox c = sender as CheckBox;

			Microsoft.Win32.RegistryKey key;

			key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software\\CSMP3Player", true);

			if (c.IsChecked == true)
				key.SetValue("InTray", true);
			else
				key.SetValue("InTray", false);

			key.Close();

			trayChanged = true;

			OptionsChanged options = optionsChanged;
			if (options != null) options(this, EventArgs.Empty);
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			ColorDialog colorDlg = new ColorDialog();
			colorDlg.AllowFullOpen = true;
			colorDlg.AnyColor = true;
			colorDlg.SolidColorOnly = false;

			if (colorDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				color = colorDlg.Color;
				colorChanged = true;
			}

			OptionsChanged options = optionsChanged;
			if (options != null) options(this, EventArgs.Empty);
		}

		private void dynamicCheck_Click(object sender, RoutedEventArgs e)
		{
			colorChanged = false;
			dynamicChanged = true;

			OptionsChanged options = optionsChanged;
			if (options != null) options(this, EventArgs.Empty);
		}

		private void Button_Click_1(object sender, RoutedEventArgs e)
		{
			using (var fbd = new FolderBrowserDialog())
			{
				DialogResult result = fbd.ShowDialog();

				if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
				{
					path = fbd.SelectedPath;
					pathChanged = true;

					OptionsChanged options = optionsChanged;
					if (options != null) options(this, EventArgs.Empty);
				}
			}
		}
	}
}
