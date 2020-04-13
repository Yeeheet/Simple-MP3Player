using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;
using TagLib;

namespace Projet_Final_CS___Music_Player
{
	class Playlist
	{
		public List<Song> songsList { get; set; }
		public Song currentSong;
		public DateTime date;

		/// <summary>
		/// Creates an instance of <see cref="Playlist"/>.
		/// </summary>
		public Playlist()
		{
			songsList = new List<Song>();
			date = DateTime.Now;
		}

		/// <summary>
		/// Adds a <see cref="Song"/> to the list.
		/// </summary>
		/// <param name="filename">
		/// Path to the file
		/// </param>
		public Song AddSong(string filename)
		{
			Song nSong = new Song();

			if ((nSong = songsList.Find(s => s.path.Equals(filename))) == null)
			{
				var tfile = TagLib.File.Create(filename);
				string title = tfile.Tag.Title;
				string artist = tfile.Tag.FirstPerformer;
				string album = tfile.Tag.Album;
				TimeSpan duration = tfile.Properties.Duration;

				if (tfile.Tag.Pictures.Length > 0)
				{
					IPicture pic = tfile.Tag.Pictures[0];

					MemoryStream ms = new MemoryStream(pic.Data.Data);
					ms.Seek(0, SeekOrigin.Begin);

					BitmapImage bitmap = new BitmapImage();
					bitmap.BeginInit();
					bitmap.StreamSource = ms;
					bitmap.EndInit();

					ImageSource imgSrc = bitmap as ImageSource;

					Bitmap i = new Bitmap(ms);

					int size = i.Width * i.Height;
					long R, G, B, A;
					R = G = B = A = 0;
					for (int x = 0; x < i.Width; x++)
						for (int y = 0; y < i.Height; y++)
						{
							A += i.GetPixel(x, y).A;
							R += i.GetPixel(x, y).R;
							G += i.GetPixel(x, y).G;
							B += i.GetPixel(x, y).B;
						}

					nSong = new Song(filename, artist, album, title, duration, imgSrc);
					nSong.dominantColor = System.Windows.Media.Color.FromArgb(Convert.ToByte(A / size), Convert.ToByte(R / size), Convert.ToByte(G / size), Convert.ToByte(B / size));
				}
				else
				{
					Bitmap src = Properties.Resources.unknown_song;

					MemoryStream ms = new MemoryStream();
					src.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
					BitmapImage image = new BitmapImage();
					image.BeginInit();
					ms.Seek(0, SeekOrigin.Begin);
					image.StreamSource = ms;
					image.EndInit();

					ImageSource imgSrc = image;

					if (String.IsNullOrEmpty(artist)) artist = "Unknown Artist";
					if (String.IsNullOrEmpty(title)) title = "Unknown title";
					if (String.IsNullOrEmpty(album)) album = "Unkown album";

					nSong = new Song(filename, artist, album, title, duration, imgSrc);
					nSong.dominantColor = Colors.White;
				}

				songsList.Add(nSong);

				ICollectionView view = CollectionViewSource.GetDefaultView(songsList);
				view.Refresh();
			}

			return nSong;
		}

		/// <summary>
		/// Export all <see cref="Song"/> path to an XML file.
		/// </summary>
		/// <param name="filename">
		/// Path to the save folder
		/// </param>
		public void ExportToXML(string path)
		{
			XmlTextWriter textWriter = new XmlTextWriter(Path.Combine(path, "Playlist-" + date.ToString("MM.dd.yyyy") + ".xml"), null);
			textWriter.Formatting = Formatting.Indented;
			textWriter.WriteStartDocument();
			textWriter.WriteComment("Playlist created on " + date.ToString());

			textWriter.WriteStartElement("Paths");

			foreach(Song s in songsList)
			{
				textWriter.WriteStartElement("Song");
				textWriter.WriteAttributeString("title", s.title);
				textWriter.WriteString(s.path);
				textWriter.WriteEndElement();
			}

			textWriter.WriteEndDocument();
			textWriter.Close();

			DialogResult res = System.Windows.Forms.MessageBox.Show(songsList.Count + " songs path exported to " + path, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		/// <summary>
		/// Import all <see cref="Song"/> from an XML file.
		/// </summary>
		/// <param name="file">
		/// Path to the XML file
		/// </param>
		public void ImportFromXML(string file)
		{
			XmlDocument doc = new XmlDocument();
			XmlTextReader reader = new XmlTextReader(file);
			reader.Read();
			doc.Load(reader);

			int i = 0;
			int j = 0;
			foreach (XmlNode node in doc.DocumentElement.ChildNodes)
			{
				if (System.IO.File.Exists(node.InnerText))
				{
					AddSong(node.InnerText);
					i++;
				}
				else
					j++;
			}

			DialogResult res = System.Windows.Forms.MessageBox.Show(i + " songs imported !\n" + j + " not imported.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		public void PlayNext(int index)
		{

		}
	}
}
