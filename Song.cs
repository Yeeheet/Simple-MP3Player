using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using TagLib;

namespace Projet_Final_CS___Music_Player
{
	class Song
	{
		public string path { get; }
		public ImageSource image { get; set; }

		public string artist { get; set; }
		public string album { get; set; }
		public string title { get; set; }
		public string _duration { get; set; }

		public TimeSpan duration { get; set; }
		public Color dominantColor { get; set; }

		/// <summary>
		/// Creates an empty <see cref="Song"/> object.
		/// </summary>
		public Song() { }

		/// <summary>
		/// Creates a <see cref="Song"/> object.
		/// </summary>
		/// <param name="p">Path to the file</param>
		/// <param name="a">Artist</param>
		/// <param name="al">Album</param>
		/// <param name="t">Title</param>
		/// <param name="d">Duration</param>
		/// <param name="i">Cover</param>
		public Song(string p, string a, string al, string t, TimeSpan d, ImageSource i)
		{
			path = p;
			artist = a;
			album = al;
			title = t;
			duration = d;
			_duration = duration.ToString(@"mm\:ss");
			image = i;
		}

		public override bool Equals(object obj)
		{
			if (!(obj is Song))
				return false;

			return path == (obj as Song).path;
		}

		public override int GetHashCode()
		{
			return path.GetHashCode();
		}

		public override string ToString()
		{
			return artist + " " + title + " " + album;
		}
	}
}
