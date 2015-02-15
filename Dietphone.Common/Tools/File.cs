using System.IO;

namespace Dietphone.Tools
{
    public interface File
    {
        bool Exists { get; }
        Stream GetReadingStream();
        Stream GetWritingStream();
        void MoveTo(File destination);
        void Delete();
    }

    public interface FileFactory
    {
        File Create(string relativeFilePath);
    }
}
