using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

using PropertyChanged.SourceGenerator;

using UtfUnknown;

namespace EncodingConverter.Models;

internal partial class PreviewEncodingsWindowViewModel
{
    private readonly byte[] _originalBytes;
    [Notify] private bool? _isIncludeAllEncodings;

    public PreviewEncodingsWindowViewModel(byte[] originalBytes)
    {
        this._originalBytes = originalBytes;
        this.PreviewItemsView = new(PreviewItems);
        this.IsIncludeAllEncodings = false;
    }

    public ObservableCollection<TextWithEncodingViewModel> PreviewItems { get; } = new();

    public ListCollectionView PreviewItemsView { get; }

    public TextWithEncodingViewModel? SelectedPreviewItem { get; set; }

    public void LoadEncodings()
    {
        var result = CharsetDetector.DetectFromBytes(this._originalBytes);

        var detected = result.Details
            .Where(x => x.Encoding is not null)
            .Select(x =>
            {
                EncodingsManager.TryAddEncoding(x.Encoding);
                return new TextWithEncodingViewModel(this._originalBytes, x.Encoding, true);
            })
            .ToList();

        var e = detected.Select(x => x.Encoding).ToHashSet();
        var others = EncodingsManager.ObservableEncodings
            .Where(x => !e.Contains(x))
            .Select(x => new TextWithEncodingViewModel(this._originalBytes, x, false))
            .ToList();

        foreach (var item in detected.Concat(others))
        {
            this.PreviewItems.Add(item);
        }
    }

    void OnIsIncludeAllEncodingsChanged(bool? oldValue, bool? newValue)
    {
        if (newValue == true)
        {
            this.PreviewItemsView.Filter = null;
        }
        else
        {
            this.PreviewItemsView.Filter = x => ((TextWithEncodingViewModel)x).IsDetectedEncoding;
        }
    }
}
