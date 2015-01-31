using System.IO.IsolatedStorage;
using System.IO;

namespace Dietphone.Tools
{
    public sealed class IsolatedFile : File
    {
        private string relativeFilePath;
        private static IsolatedStorageFile isolatedStorage = null;
        private static readonly object isolatedStorageLock = new object();

        public IsolatedFile(string relativeFilePath)
        {
            this.relativeFilePath = relativeFilePath;
        }

        public static IsolatedStorageFile IsolatedStorage
        {
            get
            {
                lock (isolatedStorageLock)
                {
                    if (isolatedStorage == null)
                    {
                        isolatedStorage = IsolatedStorageFile.GetUserStoreForApplication();
                    }
                    return isolatedStorage;
                }
            }
        }

        public bool Exists
        {
            get
            {
                return IsolatedStorage.FileExists(relativeFilePath);
            }
        }

        public Stream GetReadingStream()
        {
            return IsolatedStorage.OpenFile(relativeFilePath, FileMode.Open, FileAccess.Read);
        }

        public Stream GetWritingStream()
        {
            return IsolatedStorage.OpenFile(relativeFilePath, FileMode.Create, FileAccess.Write);
        }

        public void MoveTo(File destination)
        {
            IsolatedStorage.MoveFile(sourceFileName: relativeFilePath,
                destinationFileName: (destination as IsolatedFile).relativeFilePath);
        }

        public void Delete()
        {
            isolatedStorage.DeleteFile(relativeFilePath);
        }
    }

    public sealed class IsolatedFileFactory : FileFactory
    {
        public File Create(string relativeFilePath)
        {
            return new IsolatedFile(relativeFilePath);
        }
    }
}