using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using TagLib;

using Application = System.Windows.Application;
using ContextMenu = System.Windows.Forms.ContextMenu;
using Image = System.Windows.Controls.Image;
using ListView = System.Windows.Forms.ListView;
using ListViewItem = System.Windows.Controls.ListViewItem;
using MenuItem = System.Windows.Controls.MenuItem;
using MouseEventArgs = System.Windows.Forms.MouseEventArgs;
using MouseEventHandler = System.Windows.Forms.MouseEventHandler;
using Path = System.IO.Path;

namespace Projet_Final_CS___Music_Player
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		Playlist playlist = new Playlist();

		NotifyIcon trayIcon;
		ContextMenu trayMenu;

		string currentFolder;
		string baseTitle;
		string savePath;

		MediaPlayer player = new MediaPlayer();

		int playerStatus = 0; // 0 = stopped, 1 = playing, 2 = paused
		int repeatMode = 0; // 0 = none, 1 = repeat once, 2 = keep playing the same song
		int nbRepeats = 0;

		bool dynamicColor = false;
		bool minTray = false;

		private readonly DispatcherTimer timer;

		public MainWindow()
		{
			InitializeComponent();

			baseTitle = this.Title = "MP3 Player";
			
			player.MediaEnded += new EventHandler(SongEnded);

			Playlist.ItemsSource = playlist.songsList;

			timer = new DispatcherTimer();
			timer.Interval = TimeSpan.FromMilliseconds(500);
			timer.Tick += new EventHandler(Timer_Tick);

			currentFolder = Environment.CurrentDirectory;

			trayMenu = new ContextMenu();
			trayMenu.MenuItems.Add("Play", TrayPlayClick);
			trayMenu.MenuItems.Add("Pause", TrayPauseClick);
			trayMenu.MenuItems.Add("Stop", TrayStopClick);
			trayMenu.MenuItems.Add("Next", TrayNextClick);
			trayMenu.MenuItems.Add("Exit", TrayExitClick);

			this.StateChanged += new EventHandler(Window_StateChanged);

			trayIcon = new NotifyIcon
			{
				Icon = Properties.Resources.stop,
				ContextMenu = trayMenu
			};
			trayIcon.MouseClick += new MouseEventHandler(TrayIconMouseClick);

			Microsoft.Win32.RegistryKey key = null;
			try
			{
				if ((key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software\\CSMP3Player", true)) == null)
				{
					key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey("Software\\CSMP3Player");
					key.SetValue("InTray", true);
					key.SetValue("SaveDirectory", System.IO.Path.Combine(Environment.CurrentDirectory));
				}
				
				if (key.GetValue("InTray") == null)
					key.SetValue("InTray", true);

				if (key.GetValue("SaveDirectory") == null)
					key.SetValue("SaveDirectory", System.IO.Path.Combine(Environment.CurrentDirectory));
			}
			catch(Exception e)
			{
				Console.WriteLine(e);
			}

			if(Convert.ToBoolean(key.GetValue("InTray")) == true)
			{
				minTray = true;
			}

			savePath = Convert.ToString(key.GetValue("SaveDirectory"));

			key.Close();

			playImg.Source = ICOToPNG(Properties.Resources.play);
			stopImg.Source = ICOToPNG(Properties.Resources.stop);
			pauseImg.Source = ICOToPNG(Properties.Resources.pause);
			refreshImg.Source = ICOToPNG(Properties.Resources.refresh);
			repeatImg.Source = ICOToPNG(Properties.Resources.repeat);
			fastforwardImg.Source = ICOToPNG(Properties.Resources.fastforwardright);
			fastbackwardImg.Source = ICOToPNG(Properties.Resources.fastforwardleft);
		}

		#region Traymenu Actions
		private void TrayExitClick(object sender, EventArgs e)
		{
			trayIcon.Dispose();
			System.Windows.Application.Current.Shutdown();
		}

		private void TrayStopClick(object sender, EventArgs e)
		{
			Stop();
		}

		private void TrayPauseClick(object sender, EventArgs e)
		{
			Pause();
		}

		private void TrayPlayClick(object sender, EventArgs e)
		{
			if (playerStatus != 1) Play();
		}

		private void TrayNextClick(object sender, EventArgs e)
		{
			PlayNext();
		}
		#endregion

		#region MyFuctions

		private BitmapImage ICOToPNG(Icon icon)
		{
			var bitmap = icon.ToBitmap();

			MemoryStream ms = new MemoryStream();
			bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
			ms.Position = 0;
			BitmapImage bi = new BitmapImage();
			bi.BeginInit();
			bi.StreamSource = ms;
			bi.EndInit();

			return bi;
		}

		private void Timer_Tick(object sender, EventArgs e)
		{
			ProgressBar.Value = player.Position.TotalSeconds;
			songCurentPosition.Text = TimeSpan.FromSeconds(ProgressBar.Value).ToString(@"mm\:ss");
		}

		private void Play()
		{
			player.Play();
			timer.Start();
			this.Title = baseTitle + " - Playing " + titreLabel.Text;

			try
			{
				trayIcon.Icon = Properties.Resources.play;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}

			playerStatus = 1;
		}

		private void PlayNext()
		{
			int index = playlist.songsList.FindIndex(s => s.path == playlist.currentSong.path);

			if (index != -1)
			{
				if (++index < playlist.songsList.Count)
				{
					SetSong(playlist.songsList[index]);
				}
				else
					SetSong(playlist.songsList[0]);
			}
			else
				SetSong(playlist.songsList[0]);
		}

		private void Stop()
		{
			player.Stop();
			player.Close();
			timer.Stop();
			ProgressBar.Value = 0.00;
			this.Title = baseTitle + " - Nothing is playing";
			songCurentPosition.Text = "0:00";

			artistLabel.Text = "";
			albumLabel.Text = "";
			titreLabel.Text = "";
			songDuration.Text = "0:00";
			ProgressBar.Maximum = 0.0;

			try
			{
				trayIcon.Icon = Properties.Resources.stop;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}

			playerStatus = 0;
		}

		private void Pause()
		{
			player.Pause();
			timer.Stop();
			this.Title = baseTitle + " - Paused " + titreLabel.Text;

			try
			{
				trayIcon.Icon = Properties.Resources.pause;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}

			playerStatus = 2;
		}
		#endregion

		#region SongManagement
		private void SetSong(Song song)
		{
			if (timer.IsEnabled) Stop();

			player.Open(new Uri(song.path));
			AlbumImage.Source = song.image;
			artistLabel.Text = song.artist;
			albumLabel.Text = song.album;
			titreLabel.Text = song.title;
			songDuration.Text = song.duration.ToString(@"mm\:ss");
			ProgressBar.Maximum = song.duration.TotalSeconds;

			playlist.currentSong = song;

			if (dynamicColor == true) ProgressBar.Foreground = new SolidColorBrush(song.dominantColor);

			Play();
		}

		private void ImportSong(object sender, RoutedEventArgs e)
		{
			string filename = "";

			Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

			dlg.DefaultExt = ".mp3";
			dlg.Filter = "MP3 Files (*.mp3)|*.mp3";

			Nullable<bool> result = dlg.ShowDialog();

			if (result == true)
			{
				filename = dlg.FileName;
				Song ns = playlist.AddSong(filename);

				if (playlist.songsList.Count < 2)
				{
					SetSong(ns);
					playlist.currentSong = ns;
				}
			}
		}

		private void ImportFolder(object sender, RoutedEventArgs e)
		{
			using (var fbd = new FolderBrowserDialog())
			{
				DialogResult result = fbd.ShowDialog();

				if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
				{
					string[] files = Directory.GetFiles(fbd.SelectedPath);

					int prev = playlist.songsList.Count;

					foreach (string file in files)
					{
						if (Path.GetExtension(file).Equals(".mp3"))
							playlist.AddSong(file);
					}

					int cur = playlist.songsList.Count;

					DialogResult res = System.Windows.Forms.MessageBox.Show(cur - prev + " songs imported !", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
				}
			}
		}

		private void ExportPlaylist(object sender, RoutedEventArgs e)
		{
			DialogResult res = System.Windows.Forms.MessageBox.Show("This feature will save the path of each song not the actual song.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);

			playlist.ExportToXML(savePath);
		}

		private void ImportPlaylist(object sender, RoutedEventArgs e)
		{
			Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

			dlg.DefaultExt = ".xml";
			dlg.Filter = "XML Files (*.xml)|*.xml";

			Nullable<bool> result = dlg.ShowDialog();

			if (result == true)
			{
				playlist.ImportFromXML(dlg.FileName);
			}
		}
		#endregion

		#region Events
		private void Window_StateChanged(object sender, EventArgs e)
		{
			Console.WriteLine(minTray);

			if (minTray == true)
			{
				if (this.WindowState == WindowState.Minimized)
				{
					this.ShowInTaskbar = false;
					trayIcon.Visible = true;
				}
				else if (this.WindowState == WindowState.Normal)
				{
					trayIcon.Visible = false;
					this.ShowInTaskbar = true;
					this.WindowState = WindowState.Normal;
				}
			}
		}

		private void TrayIconMouseClick(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left) this.WindowState = WindowState.Normal;
		}

		private void playBtn_Click(object sender, RoutedEventArgs e)
		{
			if(playerStatus == 2) Play();
		}

		private void stopBtn_Click(object sender, RoutedEventArgs e)
		{
			Stop();
		}

		private void pauseBtn_Click(object sender, RoutedEventArgs e)
		{
			if(playerStatus == 1) Pause();
		}

		private void Expander_Expanded(object sender, RoutedEventArgs e)
		{
			this.Width += ((Expander)sender).ActualWidth;
		}

		private void Expander_Collapsed(object sender, RoutedEventArgs e)
		{
			this.Width -= ((Expander)sender).ActualWidth;
		}

		private void Playlist_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			if(Playlist.SelectedIndex >= 0) SetSong(playlist.songsList[Playlist.SelectedIndex]);
		}

		private void ProgressBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			System.Windows.Point mousepnt = e.GetPosition(this);

			double value = ((mousepnt.X - ProgressBar.Margin.Left) / ProgressBar.Width) * ProgressBar.Maximum;

			timer.Stop();
			player.Stop();
			ProgressBar.Value = value;
			player.Position = TimeSpan.FromSeconds(value);
			timer.Start();
			player.Play();
		}

		private void SongEnded(object sender, EventArgs e)
		{
			Console.WriteLine("SongEnded : " + repeatMode);

			switch(repeatMode)
			{
				case 0:
					if (playlist.songsList.Count <= 1 || (playlist.currentSong == playlist.songsList[playlist.songsList.Count - 1]))
					{
						Stop();
						break;
					}
					PlayNext();
					break;

				case 1:
					if (++nbRepeats <= 1) { Stop(); SetSong(playlist.currentSong); RepeatOnceBtn.Background = new SolidColorBrush(Colors.Transparent);}
					else { PlayNext(); nbRepeats = 0; repeatMode = 0; }
					break;

				case 2:
					SetSong(playlist.currentSong);
					break;
			}
		}

		private void YoutubeDownloader_Click(object sender, RoutedEventArgs e)
		{
			DialogResult res = System.Windows.Forms.MessageBox.Show("This feature is kind of broken !\nIt works but the duration is wrong.\n\nSure you want to do this ?", "Confirmation", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
			if (res == System.Windows.Forms.DialogResult.OK)
			{
				Downloader downloader = new Downloader();
				Nullable<bool> dg = downloader.ShowDialog();
			}

			if (res == System.Windows.Forms.DialogResult.Cancel)
			{
				return;
			}
		}

		private void DeleteSong(object sender, RoutedEventArgs e)
		{
			if (!Playlist.SelectedItems.Contains(playlist.currentSong))
			{
				foreach(Song s in Playlist.SelectedItems)
					playlist.songsList.Remove(s);

				ICollectionView view = CollectionViewSource.GetDefaultView(playlist.songsList);
				view.Refresh();
			}
			else
			{
				Stop();
				Playlist.SelectedItem = -1;

				foreach (Song s in Playlist.SelectedItems)
					playlist.songsList.Remove(s);

				ICollectionView view = CollectionViewSource.GetDefaultView(playlist.songsList);
				view.Refresh();
			}
		}

		private void ListViewItem_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
		{
			var item = sender as ListViewItem;
			System.Windows.Controls.ContextMenu contextMenu = new System.Windows.Controls.ContextMenu();

			if (item != null && item.IsSelected)
			{
				if (Playlist.SelectedItems.Count >= 1)
				{
					MenuItem m1 = new MenuItem();
					m1.Header = "Play";
					m1.Click += PlaySong;

					MenuItem m2 = new MenuItem();
					m2.Header = "Delete";
					m2.Click += DeleteSong;

					contextMenu.Items.Add(m1);
					contextMenu.Items.Add(m2);
					item.ContextMenu = contextMenu;
				}
			}
		}

		private void PlaySong(object sender, RoutedEventArgs e)
		{
			SetSong(Playlist.SelectedItem as Song);
		}

		private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			player.Volume = (sender as Slider).Value;
		}

		private void RepeatOnce_Click(object sender, RoutedEventArgs e)
		{
			if (repeatMode != 1)
			{
				repeatMode = 1;
				nbRepeats = 0;
				(sender as System.Windows.Controls.Button).Background = new SolidColorBrush(Colors.LimeGreen);
				RepeatBtn.Background = new SolidColorBrush(Colors.Transparent);
			}
			else
			{
				repeatMode = 0;
				(sender as System.Windows.Controls.Button).Background = new SolidColorBrush(Colors.Transparent);
			}
		}

		private void Repeat_Click(object sender, RoutedEventArgs e)
		{
			if (repeatMode != 2)
			{
				repeatMode = 2;
				(sender as System.Windows.Controls.Button).Background = new SolidColorBrush(Colors.LimeGreen);
				RepeatOnceBtn.Background = new SolidColorBrush(Colors.Transparent);
			}
			else
			{
				repeatMode = 0;
				(sender as System.Windows.Controls.Button).Background = new SolidColorBrush(Colors.Transparent);
			}
		}
		
		private void fastforward_Click(object sender, RoutedEventArgs e)
		{
			player.Position += TimeSpan.FromSeconds(10);
		}

		private void fastbackward_Click(object sender, RoutedEventArgs e)
		{
			player.Position -= TimeSpan.FromSeconds(10);
		}

		private void Options_Click(object sender, RoutedEventArgs e)
		{
			Options dlg = new Options();

			dlg.Owner = this;
			dlg.optionsChanged += new OptionsChanged(dlg_OptionsChanged);

			dlg.Show();
		}

		private void dlg_OptionsChanged(object sender, EventArgs e)
		{
			Options dlg = (Options)sender;

			if (dlg.trayChanged == true)
			{
				if (dlg.trayCheck.IsChecked == true)
				{
					minTray = true;
				}
				else
				{
					minTray = false;
				}

				dlg.trayChanged = false;
			}

			if(dlg.colorChanged)
			{
				ProgressBar.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(dlg.color.A, dlg.color.R, dlg.color.G, dlg.color.B));
				dlg.colorChanged = false;
			}

			if(dlg.dynamicChanged)
			{
				dynamicColor = dlg.dynamicCheck.IsChecked ?? false;

				if(playlist.currentSong != null)
					ProgressBar.Foreground = new SolidColorBrush(playlist.currentSong.dominantColor);

				dlg.dynamicChanged = false;
			}

			if(dlg.pathChanged)
			{
				savePath = dlg.path;

				Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software\\CSMP3Player", true);
				key.SetValue("SaveDirectory", savePath);

				dlg.pathChanged = false;
			}
		}
		
		private void Exit_Click(object sender, RoutedEventArgs e)
		{
			System.Windows.Application.Current.Shutdown();
		}
		#endregion
	}
}
