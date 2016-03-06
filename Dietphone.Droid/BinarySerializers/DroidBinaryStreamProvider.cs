using System.IO;
using Android.Content;
using Dietphone.Tools;

namespace Dietphone.BinarySerializers
{
    public sealed class DroidBinaryStreamProvider : SmartphoneBinaryStreamProvider
    {
        private readonly Context context;

        public DroidBinaryStreamProvider(FileFactory fileFactory, Context context)
            : base(fileFactory)
        {
            this.context = context;
        }

        protected override Stream GetFirstRunInputStream(string fileName)
        {
            var assets = context.Assets;
            var relativePath = GetFirstRunRelativePath(fileName);
            return assets.Open(relativePath);
        }
    }
}