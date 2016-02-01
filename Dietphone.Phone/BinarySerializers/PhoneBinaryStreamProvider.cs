using System;
using System.IO;
using System.Windows;
using Dietphone.Tools;

namespace Dietphone.BinarySerializers
{
    public sealed class PhoneBinaryStreamProvider : SmartphoneBinaryStreamProvider
    {
        private const string FIRST_RUN_DIRECTORY = "firstrun";

        public PhoneBinaryStreamProvider(FileFactory fileFactory)
            : base (fileFactory)
        {
        }

        protected override Stream GetFirstRunInputStream(string fileName)
        {
            var relativePath = Path.Combine(FIRST_RUN_DIRECTORY, fileName);
            var resource = Application.GetResourceStream(new Uri(relativePath, UriKind.Relative));
            return resource.Stream;
        }
    }
}