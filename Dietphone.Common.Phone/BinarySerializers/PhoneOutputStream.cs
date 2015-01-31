using System;
using Dietphone.Tools;

namespace Dietphone.BinarySerializers
{
    public sealed class PhoneOutputStream : OutputStream
    {
        public System.IO.Stream Stream { get; private set; }
        private File file;
        private File fileTemp1;
        private File fileTemp2;
        private File fileTemp3;
        private const string TEMP_NAME = "{0}_temp{1}_{2}";

        public PhoneOutputStream(FileFactory fileFactory, string fileName)
        {
            var guid = Guid.NewGuid().ToString();
            file = fileFactory.Create(fileName);
            fileTemp1 = fileFactory.Create(string.Format(TEMP_NAME, fileName, 1, guid));
            fileTemp2 = fileFactory.Create(string.Format(TEMP_NAME, fileName, 2, guid));
            fileTemp3 = fileFactory.Create(string.Format(TEMP_NAME, fileName, 3, guid));
            Stream = fileTemp1.GetWritingStream();
        }

        public void Commit(long size)
        {
            CheckSize(size);
            fileTemp1.MoveTo(fileTemp2);
            var fileExisted = file.Exists;
            if (fileExisted)
                file.MoveTo(fileTemp3);
            fileTemp2.MoveTo(file);
            if (fileExisted)
                fileTemp3.Delete();
        }

        private void CheckSize(long size)
        {
            using (var readingStream = fileTemp1.GetReadingStream())
            {
                var actual = readingStream.Length;
                if (size != actual)
                    throw new InvalidOperationException(string.Format("Size should be {0} but is {1}.", size, actual));
            }
        }
    }
}
