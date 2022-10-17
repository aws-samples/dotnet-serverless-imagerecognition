using System.ComponentModel;
using ImageRecognition.API.Client;

namespace ImageRecognition.BlazorWebAssembly.Models
{
    public class AlbumWrapper : INotifyPropertyChanged
    {
        public AlbumWrapper(Album album)
        {
            Album = album;
        }

        public Album Album { get; set; }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName = "")
        {
            var changed = PropertyChanged;
            if (changed == null)
                return;

            changed.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}