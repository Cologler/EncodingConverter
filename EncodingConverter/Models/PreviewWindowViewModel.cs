using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using PropertyChanged.SourceGenerator;

namespace EncodingConverter.Models;

internal partial class PreviewWindowViewModel
{
    private readonly DecodeSource _decodeSource;
    [Notify]
    private Encoding? _selectedEncoding;
    [Notify]
    private string _decodedText = "";

    public PreviewWindowViewModel(DecodeSource decodeSource)
    {
        this._decodeSource = decodeSource;
    }

    void OnSelectedEncodingChanged(Encoding? oldValue, Encoding? newValue)
    {
        if (newValue is null)
        {
            this.DecodedText = "";
        }
        else
        {
            this.DecodedText = this._decodeSource.Decode(newValue);
        }
    }
}
