using Moq;
using System.Collections.Generic;

namespace Dietphone.Models.Tests
{
    public class StorageCreatorStub : StorageCreator
    {
        public string CultureName
        {
            set { }
        }

        public Storage<T> CreateStorage<T>() where T : Entity, new()
        {
            var mock = new Mock<Storage<T>>();
            var entities = new List<T>();
            if (typeof(T) == typeof(Category))
            {
                entities.Add(new T());
            }
            mock.Setup(m => m.Load()).Returns(entities);
            return mock.Object;
        }
    }
}
