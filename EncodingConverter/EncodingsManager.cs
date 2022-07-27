using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace EncodingConverter;

class EncodingsManager
{
    static readonly object s_SyncRoot = new();
    static readonly Encoding[] s_SystemEncodings;
    static HashSet<Encoding> s_EncodingSet = default!;

    public static Encoding DefaultEncoding { get; } = Encoding.UTF8;

    public static ObservableCollection<Encoding> ObservableEncodings { get; } = new();

    static EncodingsManager()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        s_SystemEncodings = Encoding.GetEncodings()
            .Select(z => z.GetEncoding())
            .ToArray();

        ReSort();
    }

    public static void ReSort()
    {
        Debug.Assert(DefaultEncoding is not null);

        lock (s_SyncRoot)
        {
            s_EncodingSet = s_SystemEncodings.ToHashSet();

            ObservableEncodings.Clear();
            foreach (var encoding in s_EncodingSet.OrderBy(x => x.EncodingName))
            {
                ObservableEncodings.Add(encoding);
            }
        }

        Debug.Assert(s_EncodingSet.Contains(DefaultEncoding));
    }

    public static void TryAddEncoding(Encoding encoding)
    {
        Debug.Assert(encoding is not null);

        lock (s_SyncRoot)
        {
            if (s_EncodingSet.Add(encoding))
            {
                var index = s_EncodingSet
                    .OrderBy(x => x.EncodingName)
                    .ToList()
                    .IndexOf(encoding);

                Debug.Assert(index >= 0);

                ObservableEncodings.Insert(index, encoding);
            }
        }
    }
}
