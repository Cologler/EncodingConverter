using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using UtfUnknown;

namespace EncodingConverter.Models
{
    class TextFileViewModel : INotifyPropertyChanged
    {
        private string _detectedEncodingName;
        private bool _isEnabledConvert;
        private string _convertStatus;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = default!)
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
                try
                {
                    return CharsetDetector.DetectFromFile(this.Path);
                }
                catch (IOException)
                {
                    // pass
                }
                catch (UnauthorizedAccessException)
                {
                    // pass
                }

                return null;
            });

            if (dr?.Detected?.Encoding is not null)
            {
                this.DetectedEncodingName = dr.Detected.EncodingName;
                this.DetectedEncoding = dr.Detected.Encoding;
                this.IsEnabledConvert = true;
            }
        }

        public string Path { get; }

        public string DetectedEncodingName
        {
            get => this._detectedEncodingName;
            private set
            {
                if (this._detectedEncodingName != value)
                {
                    this._detectedEncodingName = value;
                    this.OnPropertyChanged();
                }
            }
        }

        public Encoding DetectedEncoding { get; private set; }

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

            var sourceEncoding = this.DetectedEncoding;
            if (targetEncoding == sourceEncoding)
                return;

            if (!this.IsEnabledConvert)
                return;
            this.IsEnabledConvert = false;

            var path = this.Path;

            if (await Task.Run(() =>
            {
                var baseDir = System.IO.Path.GetDirectoryName(path)!;
                var originName = System.IO.Path.GetFileNameWithoutExtension(path);
                var originExt = System.IO.Path.GetExtension(path);
                var newName = $"{originName}.{targetEncoding.WebName.ToLower()}{originExt}";
                var newPath = System.IO.Path.Combine(baseDir, newName);

                try
                {
                    using (var reader = new StreamReader(path, sourceEncoding))
                    using (var writer = new StreamWriter(newPath, false, targetEncoding))
                    {
                        writer.Write(reader.ReadToEnd());
                    }

                    if (!toNewFile)
                    {
                        File.Move(path, System.IO.Path.Combine(baseDir, $"{originName}.origin{originExt}"), true);
                        File.Move(newPath, path, false);
                    }

                    return true;
                }
                catch (IOException)
                {
                    // pass
                }
                catch (UnauthorizedAccessException)
                {
                    // pass
                }

                return false;
            }))
            {
                this.ConvertStatus = "Done";
            }
        }
    }
}
