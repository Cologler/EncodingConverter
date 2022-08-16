using System.Text;

namespace EncodingConverter.Models;

class TextWithEncodingViewModel
{
    readonly byte[] _originalBytes;
    string? _decodedText;

    public TextWithEncodingViewModel(byte[] originalBytes, Encoding encoding, bool isDetectedEncoding)
    {
        this._originalBytes = originalBytes;
        this.Encoding = encoding;
        this.IsDetectedEncoding = isDetectedEncoding;
    }

    public Encoding Encoding { get; }

    public bool IsDetectedEncoding { get; }

    public string EncodingName => Encoding.EncodingName;

    public string DecodedText => _decodedText ??= this.Encoding.GetString(this._originalBytes);
}
