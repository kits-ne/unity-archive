using Cysharp.Threading.Tasks;
using TMPro;
using UniBloc;

namespace Samples.BLoC
{
    public static class TextMeshProExtensions
    {
        public static void BindTo(this Stream<string> stream, TextMeshProUGUI text)
        {
            stream.AsAsyncEnumerable().BindTo(text, text.destroyCancellationToken);
        }

        public static void BindTo(this Stream<int> stream, TextMeshProUGUI text)
        {
            stream.AsAsyncEnumerable().BindTo(
                text,
                (tmp, value) => tmp.text = value.ToString(),
                text.canvas
            );
        }

        public static void BindTo<T>(this Stream<T> stream, TextMeshProUGUI text) where T : class
        {
            stream.AsAsyncEnumerable().BindTo(
                text,
                (tmp, value) => tmp.text = value.ToString(),
                text.canvas
            );
        }
    }
}