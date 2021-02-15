using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using UtfUnknown;

namespace EncodingConverter
{
    class TextFileViewModel : INotifyPropertyChanged
    {
        private string _encodingName;
        private bool _isEnabledConvert;
        private string _convertStatus;

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = default)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public TextFileViewModel(string path)
        {
            this.Path = path;
            this.ConvertStatus = "Convert";
        }

        public async Task DetectAsync()
        {
            var dr = await Task.Run(() =>
            {
                return CharsetDetector.DetectFromFile(this.Path);
            });

            var encoding = dr.Detected?.Encoding;
            if (encoding != null)
            {
                this.EncodingName = dr.Detected.EncodingName;
                this.Encoding = dr.Detected.Encoding;
                this.IsEnabledConvert = true;
            }
        }

        public string Path { get; }

        public string EncodingName
        {
            get => this._encodingName;
            private set
            {
                if (this._encodingName != value)
                {
                    this._encodingName = value;
                    this.OnPropertyChanged();
                }
            }
        }

        public Encoding Encoding { get; private set; }

        public string ConvertStatus
        {
            get => this._convertStatus;
            private set
            {
                if (this._convertStatus != value)
                {
                    this._convertStatus = value;
                    this.OnPropertyChanged();
                }                
            }
        }

        public bool IsEnabledConvert
        {
            get => this._isEnabledConvert;
            private set
            {
                if (this._isEnabledConvert != value)
                {
                    this._isEnabledConvert = value;
                    this.OnPropertyChanged();
                }
            }
        }

        public async Task ConvertAsync(Encoding targetEncoding, bool toNewFile)
        {
            if (targetEncoding is null)
                throw new ArgumentNullException(nameof(targetEncoding));
            if (!this.IsEnabledConvert)
                return;
            if (targetEncoding == this.Encoding)
                return;

            this.IsEnabledConvert = false;
            var path = this.Path;
            await Task.Run(() =>
            {
                var baseDir = System.IO.Path.GetDirectoryName(path);
                var originName = System.IO.Path.GetFileNameWithoutExtension(path);
                var originExt = System.IO.Path.GetExtension(path);
                var newName = $"{originName}.{targetEncoding.WebName.ToLower()}{originExt}";
                var newPath = System.IO.Path.Combine(baseDir, newName);
                using (var reader = new StreamReader(this.Path, this.Encoding))
                using (var writer = new StreamWriter(newPath, false, targetEncoding))
                {
                    writer.Write(reader.ReadToEnd());
                }

                if (!toNewFile)
                {
                    File.Move(path, System.IO.Path.Combine(baseDir, $"{originName}.origin{originExt}"), true);
                    File.Move(newPath, path, false);
                }
            });
            this.ConvertStatus = "Done";
        }
    }
}
