using System.IO;

namespace Dietphone.BinarySerializers
{
    public sealed class DesktopBinaryStreamProvider : BinaryStreamProvider
    {
        public const string DIRECTORY = @"c:\temp\dietphone";

        public Stream GetInputStream(string fileName)
        {
            var path = Path.Combine(DIRECTORY, fileName);
            return new FileStream(path, FileMode.Open);
        }

        public OutputStream GetOutputStream(string fileName)
        {
            var path = Path.Combine(DIRECTORY, fileName);
            var stream = new FileStream(path, FileMode.Truncate);
            return new DesktopOutputStream(stream);
        }
    }
}