using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UtfUnknown;

namespace EncodingConverter.Models;

internal class DecodeSource
{
    private readonly byte[] _originalBytes;
    private DetectionResult? _detectionResult;

    public DecodeSource(byte[] originalBytes)
    {
        this._originalBytes = originalBytes;
    }

    public string Decode(Encoding encoding)
    {
        Debug.Assert(encoding is not null);

        return encoding.GetString(this._originalBytes);
    }

    public DetectionResult DetectEncodings()
    {
        if (_detectionResult is null)
        {
            _detectionResult = CharsetDetector.DetectFromBytes(this._originalBytes);
            foreach (var item in _detectionResult.Details)
            {
                EncodingsManager.TryAddEncoding(item.Encoding);
            }
        } 

        return _detectionResult;
    }
}
