using System;
using System.Linq;
using Dietphone.Models;
using System.IO;
using Ploeh.AutoFixture;

namespace Dietphone.BinarySerializers.Tests
{
    public abstract class BinaryStorageTestsBase
    {
        protected readonly Fixture fixture = new Fixture();

        protected T WriteAndRead<T>(BinaryStorage<T> storage, T itemToWrite) where T : Entity, new()
        {
            var stream = new MemoryStream();
            var writer = new BinaryWriter(stream);
            storage.WriteItem(writer, itemToWrite);
            var itemToRead = new T();
            var reader = new BinaryReader(stream);
            stream.Position = 0;
            storage.ReadItem(reader, itemToRead);
            return itemToRead;
        }
    }
}
