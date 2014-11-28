using System.Collections.Generic;

namespace Dietphone.Tools
{
    public interface CloudProvider
    {
        void UploadFile(string name, string data);
        List<string> ListFiles();
        string DownloadFile(string name);
    }

    public interface CloudProviderFactory
    {
        CloudProvider Create();
    }
}
