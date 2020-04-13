using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using TagLib;
using YoutubeExplode;
using YoutubeExplode.Models;
using YoutubeExplode.Models.MediaStreams;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using Image = System.Drawing.Image;
using System.Net;
using Path = System.IO.Path;
using NReco.VideoConverter;

namespace Projet_Final_CS___Music_Player
{
	/// <summary>
	/// Interaction logic for Downloader.xaml
	/// </summary>
	public partial class Downloader : Window
	{
		Video video;
        Picture picture;
		YoutubeClient client;
		AudioStreamInfo streamInfo;
		AudioStreamInfo audioStreamInfo;
		MediaStreamInfoSet streamInfoSet;
        
		string ext;

        private readonly HttpClient _httpClient = new HttpClient();
        private readonly SemaphoreSlim _requestRateSemaphore = new SemaphoreSlim(1, 1);
        private DateTimeOffset _lastRequestInstant = DateTimeOffset.MinValue;


        public Downloader()
		{
			InitializeComponent();
		}

		private async void Button_Click(object sender, RoutedEventArgs e)
		{
			string link = linkBox.Text;
			var id = YoutubeClient.ParseVideoId(link);

			client = new YoutubeClient();
			video = await client.GetVideoAsync(id);
			streamInfoSet = await client.GetVideoMediaStreamInfosAsync(video.Id);
			streamInfo = streamInfoSet.Audio.WithHighestBitrate();


            ext = streamInfo.Container.GetFileExtension();

            MediaStreamInfo m = 

			audioStreamInfo = streamInfoSet.Audio
						.OrderByDescending(s => s.Container == Container.WebM)
						.ThenByDescending(s => s.Bitrate)
						.First();

            var pic = new BitmapImage(new Uri(video.Thumbnails.HighResUrl));
            thumbnail.Source = pic;

            //pic.CreateOptions = BitmapCreateOptions.None;
            //WriteableBitmap wb = new WriteableBitmap(pic);
            //ImageConverter converter = new ImageConverter();
            //byte[] b = (byte[])converter.ConvertTo(m, typeof(byte[]));

            //MemoryStream ms = new MemoryStream(b);
            

            var webClient = new WebClient();
            byte[] imageBytes = webClient.DownloadData(video.Thumbnails.HighResUrl);
            picture = new Picture(imageBytes);

            titleInfo.Text = video.Title;
		}

		private async void Button_Click_1(object sender, RoutedEventArgs e)
		{
            string path = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, /*$"DownloadedSongs/{video.Title}.mp3"*/ @"DownloadedSongs");

            try
            {
                await client.DownloadMediaStreamAsync(audioStreamInfo, $"DownloadedSongs/{video.Title}.webm");

                FFMpegConverter converter = new FFMpegConverter();
                converter.ConvertMedia($"DownloadedSongs/{video.Title}.webm", $"DownloadedSongs/{video.Title}.mp3", "mp3");

                TryExtractArtistAndTitle(video.Title, out var artist, out var title);

                var taggedFile = TagLib.File.Create($"DownloadedSongs/{video.Title}.mp3");
                Debug.WriteLine("DURATION : " + taggedFile.Properties.Duration);
                taggedFile.Tag.Performers = new[] { artist };
                taggedFile.Tag.Title = title;
                taggedFile.Tag.Album = "Downloaded from Youtube";
                taggedFile.Tag.Pictures = picture != null ? new[] { picture } : Array.Empty<IPicture>();
                taggedFile.Save();
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            this.DialogResult = true;
		}

        private bool TryExtractArtistAndTitle(string videoTitle, out string artist, out string title)
        {
            // Get rid of common rubbish in music video titles
            videoTitle = videoTitle.Replace("(official video)", "");
            videoTitle = videoTitle.Replace("(official lyric video)", "");
            videoTitle = videoTitle.Replace("(official music video)", "");
            videoTitle = videoTitle.Replace("(official audio)", "");
            videoTitle = videoTitle.Replace("(official)", "");
            videoTitle = videoTitle.Replace("(lyric video)", "");
            videoTitle = videoTitle.Replace("(lyrics)", "");
            videoTitle = videoTitle.Replace("(acoustic video)", "");
            videoTitle = videoTitle.Replace("(acoustic)", "");
            videoTitle = videoTitle.Replace("(live)", "");
            videoTitle = videoTitle.Replace("(animated video)", "");

            // Split by common artist/title separator characters
            var split = videoTitle.Split(new[] { " - ", " ~ ", " — ", " – " }, StringSplitOptions.RemoveEmptyEntries);

            // Extract artist and title
            if (split.Length >= 2)
            {
                artist = split[0].Trim();
                title = split[1].Trim();
                return true;
            }

            if (split.Length == 1)
            {
                artist = null;
                title = split[0].Trim();
                return true;
            }

            artist = null;
            title = null;

            return false;
        }
    }
}

