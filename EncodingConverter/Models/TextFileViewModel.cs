using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using PropertyChanged.SourceGenerator;

using UtfUnknown;

namespace EncodingConverter.Models
{
    partial class TextFileViewModel
    {
        [Notify] bool _isEnabledConvert;
        [Notify] string _convertStatus;
        [Notify] Encoding? _detectedEncoding;
        [Notify] Encoding? _sourceEncoding;

        public TextFileViewModel(string path)
        {
            this.Path = path;
            this.ConvertStatus = string.Empty;
        }

        public async Task LoadFileAsync()
        {
            byte[]? fileContent;
            try
            {
                fileContent = await File.ReadAllBytesAsync(this.Path);
            }
            catch (Exception exc)
            {
                this.ConvertStatus = "Error";
                this.LoadFileException = exc;
                // pass
                return;
            }

            this.DecodeSource = new(fileContent);

            var result = this.DecodeSource.DetectEncodings();

            if (result.Detected?.Encoding is { } encoding)
            {
                EncodingsManager.TryAddEncoding(encoding);
                this.SourceEncoding = this.DetectedEncoding = encoding;
            }
            else
            {
                this.SourceEncoding = null;
            }

            this.ConvertStatus = "Convert";
            this.IsEnabledConvert = true;
        }

        public string Path { get; }

        public DecodeSource? DecodeSource { get; private set; }

        public Exception LoadFileException { get; private set; }

        public string DetectedEncodingName => this.DetectedEncoding?.EncodingName ?? string.Empty;

        public async Task ConvertAsync(Encoding targetEncoding, bool toNewFile)
        {
            if (targetEncoding is null)
                throw new ArgumentNullException(nameof(targetEncoding));

            var sourceEncoding = this.SourceEncoding;
            if (sourceEncoding is null || targetEncoding == sourceEncoding)
                return;

            if (!this.IsEnabledConvert)
                return;
            this.IsEnabledConvert = false;

            Debug.Assert(sourceEncoding is not null);
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
