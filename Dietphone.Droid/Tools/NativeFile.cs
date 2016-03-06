// Inspired by https://github.com/MvvmCross/MvvmCross-Plugins/blob/b766150dcbd183c155850848060ec8522fcd53f7/File/MvvmCross.Plugins.File/HackFileShare/MvxIoFileStoreBase.cs
using IOFile = System.IO.File;
using IOFileMode = System.IO.FileMode;
using IOFileAccess = System.IO.FileAccess;
using IOPath = System.IO.Path;
using IOStream = System.IO.Stream;

namespace Dietphone.Tools
{
    public sealed class NativeFile : File
    {
        private readonly string absoluteFilePath;

        public NativeFile(string absoluteFilePath)
        {
            this.absoluteFilePath = absoluteFilePath;
        }

        public bool Exists
        {
            get
            {
                return IOFile.Exists(absoluteFilePath);
            }
        }

        public IOStream GetReadingStream()
        {
            return IOFile.Open(absoluteFilePath, IOFileMode.Open, IOFileAccess.Read);
        }

        public IOStream GetWritingStream()
        {
            return IOFile.Open(absoluteFilePath, IOFileMode.Create, IOFileAccess.Write);
        }

        public void MoveTo(File destination)
        {
            IOFile.Move(sourceFileName: absoluteFilePath,
                destFileName: (destination as NativeFile).absoluteFilePath);
        }

        public void Delete()
        {
            IOFile.Delete(absoluteFilePath);
        }
    }

    public sealed class NativeFileFactory : FileFactory
    {
        private readonly string rootPath;

        public NativeFileFactory(string rootPath)
        {
            this.rootPath = rootPath;
        }

        public File Create(string relativeFilePath)
        {
            var absoluteFilePath = IOPath.Combine(rootPath, relativeFilePath);
            return new NativeFile(absoluteFilePath);
        }
    }
}