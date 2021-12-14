using System;
using System.ComponentModel;
using ImageRecognition.API.Client;
using Newtonsoft.Json;

namespace ImageRecognition.BlazorFrontend.Models
{
    public class PhotoWrapper : INotifyPropertyChanged
    {
        private string _status;

        public PhotoWrapper(Photo photo)
        {
            Photo = photo;
            _status = Photo.ProcessingStatus.ToString();
        }

        public Photo Photo { get; set; }

        public string Status
        {
            get
            {
                if (Photo.ProcessingStatus == ProcessingStatus.Failed) return "Failed";
                return _status;
            }
            set
            {
                _status = value;
                OnPropertyChanged("Status");
            }
        }

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