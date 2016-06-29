using System;
using System.IO;
using System.Collections.Generic;

namespace Dietphone.BinarySerializers
{
    public abstract class BinaryFile<T> : BinarySerializer<T> where T : new()
    {
        public BinaryStreamProvider StreamProvider { protected get; set; }
        public string CultureName { protected get; set; }
        protected abstract string FileName { get; }
        protected abstract byte WritingVersion { get; }
        protected Byte ReadingVersion { get; private set; }

        public abstract void WriteItem(BinaryWriter writer, T item);

        public abstract void ReadItem(BinaryReader reader, T item);

        protected List<T> ReadFile()
        {
            using (var input = StreamProvider.GetInputStream(FileName))
            {
                using (var reader = new BinaryReader(input))
                {
                    ReadingVersion = reader.ReadByte();
                    return reader.ReadList<T>(this);
                }
            }
        }

        protected void WriteFile(List<T> items)
        {
            var output = StreamProvider.GetOutputStream(FileName);
            var outputStream = output.Stream;
            long size;
            using (var writer = new BinaryWriter(outputStream))
            {
                writer.Write(WritingVersion);
                writer.WriteList<T>(items, this);
                size = outputStream.Length;
            }
            output.Commit(size);
        }
    }
}