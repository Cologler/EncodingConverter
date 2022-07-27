using System.Diagnostics;
using System.Linq;
using System.Text;

namespace EncodingConverter;

class EncodingsManager
{
    public static Encoding[] Encodings { get; private set; } = default!;

    public static Encoding DefaultEncoding { get; } = Encoding.UTF8;

    static EncodingsManager()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        ReSort();
    }

    public static void ReSort()
    {
        Debug.Assert(DefaultEncoding is not null);

        Encodings = Encoding.GetEncodings()
            .Select(z => z.GetEncoding())
            .OrderBy(x => x.EncodingName)
            .ToArray();

        Debug.Assert(Encodings.Contains(DefaultEncoding));
    }
}
