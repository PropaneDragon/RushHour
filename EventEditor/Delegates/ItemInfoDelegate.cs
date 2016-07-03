using EventEditor.UI;
using System.Net;
using System;
using System.Windows;
using System.Text.RegularExpressions;

namespace EventEditor.Delegates
{
    public delegate void InfoDownloadCompletedEventHandler();
    public delegate void InfoDownloadFailedEventHandler();

    public class ItemInfoDelegate
    {
        private Window _parent;
        private LoadingWindow _loadingWindow = null;
        private WebClient _webClient = null;
        private Uri _imageUri = null;

        private ulong _id = 0ul;
        private string _title = "";

        public event InfoDownloadCompletedEventHandler DownloadCompleted;
        public event InfoDownloadFailedEventHandler DownloadFailed;

        public Uri ImageUri
        {
            get
            {
                return _imageUri;
            }
        }

        public string ItemTitle
        {
            get
            {
                return _title;
            }
        }

        public ItemInfoDelegate(Window parent, ulong id)
        {
            _parent = parent;
            _id = id;
        }

        public void DownloadData()
        {
            _loadingWindow = new LoadingWindow("Loading information");
            _loadingWindow.Owner = _parent;
            _loadingWindow.Show();

            _webClient = new WebClient();
            _webClient.DownloadProgressChanged += WebClient_DownloadProgressChanged;
            _webClient.DownloadStringCompleted += WebClient_DownloadStringCompleted;
            _webClient.DownloadStringAsync(new Uri("http://steamcommunity.com/sharedfiles/filedetails/?id=" + _id.ToString()));
        }

        private void WebClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            if (_loadingWindow != null)
            {
                _loadingWindow.Progress = e.ProgressPercentage;
            }
        }

        private void WebClient_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (_loadingWindow != null)
            {
                _loadingWindow.Close();
                _loadingWindow = null;
            }

            if (e.Error == null && !e.Cancelled)
            {
                ParseData(e.Result);

                if(DownloadCompleted != null)
                {
                    DownloadCompleted();
                }
            }
            else
            {
                if (DownloadFailed != null)
                {
                    DownloadFailed();
                }
            }
        }

        private void ParseData(string result)
        {
            Regex imageFinder = new Regex("(?:<link rel.*\"image_src\".*href=\")(.*)(?:\".*)");
            Regex titleFinder = new Regex("(?:.*<div *class *= *\"workshopItemTitle\">)(.*?)(?:</div>.*)");

            Match matchedImage = imageFinder.Match(result);
            Match matchedTitle = titleFinder.Match(result);

            if(matchedImage != null && matchedImage.Groups.Count >= 2)
            {
                Regex imageUrlShortener = new Regex("(.*?)(?:&output-quality=.*)");
                string longImageUrl = matchedImage.Groups[1].Value;

                Match shortenedImageMatch = imageUrlShortener.Match(longImageUrl);

                if(shortenedImageMatch != null && shortenedImageMatch.Groups.Count >= 2)
                {
                    _imageUri = new Uri(shortenedImageMatch.Groups[1].Value);
                }
            }

            if(matchedTitle != null && matchedTitle.Groups.Count >= 2)
            {
                _title = matchedTitle.Groups[1].Value;
            }
        }
    }
}
