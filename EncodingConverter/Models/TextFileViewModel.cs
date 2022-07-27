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
                this.SourceEncoding = this.DetectedEncoding = dr.Detected.Encoding;
            }
            else
            {
                this.SourceEncoding = null;
            }

            this.IsEnabledConvert = true;
        }

        public string Path { get; }

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
