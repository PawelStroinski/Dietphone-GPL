using System.IO;
using Dietphone.Tools;

namespace Dietphone.BinarySerializers
{
    public abstract class SmartphoneBinaryStreamProvider : BinaryStreamProvider
    {
        private readonly FileFactory fileFactory;

        public SmartphoneBinaryStreamProvider(FileFactory fileFactory)
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
                return GetFirstRunInputStream(fileName);
            }
        }

        public OutputStream GetOutputStream(string fileName)
        {
            return new SmartphoneOutputStream(fileFactory, fileName);
        }

        protected abstract Stream GetFirstRunInputStream(string fileName);
    }
}