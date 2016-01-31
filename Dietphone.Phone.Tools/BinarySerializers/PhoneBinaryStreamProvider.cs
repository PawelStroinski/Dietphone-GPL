using System;
using System.IO;
using System.Windows;
using Dietphone.Tools;

namespace Dietphone.BinarySerializers
{
    public sealed class PhoneBinaryStreamProvider : BinaryStreamProvider
    {
        private readonly FileFactory fileFactory;
        private const string FIRST_RUN_DIRECTORY = "firstrun";

        public PhoneBinaryStreamProvider(FileFactory fileFactory)
        {
            this.fileFactory = fileFactory;
        }

        public Stream GetInputStream(string fileName)
        {
            var file = fileFactory.Create(fileName);
            if (file.Exists)
            {
                return file.GetReadingStream();
            }
            else
            {
                var relativePath = Path.Combine(FIRST_RUN_DIRECTORY, fileName);
                var resource = Application.GetResourceStream(new Uri(relativePath, UriKind.Relative));
                return resource.Stream;
            }
        }

        public OutputStream GetOutputStream(string fileName)
        {
            return new PhoneOutputStream(fileFactory, fileName);
        }
    }
}