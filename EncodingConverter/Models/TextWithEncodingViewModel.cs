using System.Text;

namespace EncodingConverter.Models;

class TextWithEncodingViewModel
{
    readonly DecodeSource _decodeSource;
    string? _decodedText;

    public TextWithEncodingViewModel(DecodeSource decodeSource, Encoding encoding, bool isDetectedEncoding)
    {
        this._decodeSource = decodeSource;
        this.Encoding = encoding;
        this.IsDetectedEncoding = isDetectedEncoding;
    }

    public Encoding Encoding { get; }

    public bool IsDetectedEncoding { get; }

    public string EncodingName => Encoding.EncodingName;

    public string DecodedText => _decodedText ??= this._decodeSource.Decode(this.Encoding);
}
