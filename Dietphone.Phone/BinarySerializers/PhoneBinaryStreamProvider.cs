using System;
using System.IO;
using System.Windows;
using Dietphone.Tools;

namespace Dietphone.BinarySerializers
{
    public sealed class PhoneBinaryStreamProvider : SmartphoneBinaryStreamProvider
    {
        public PhoneBinaryStreamProvider(FileFactory fileFactory)
            : base(fileFactory)
        {
        }

        protected override Stream GetFirstRunInputStream(string fileName)
        {
            var relativePath = GetFirstRunRelativePath(fileName);
            var resource = Application.GetResourceStream(new Uri(relativePath, UriKind.Relative));
            return resource.Stream;
        }
    }
}