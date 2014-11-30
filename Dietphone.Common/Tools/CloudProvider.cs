using System.Collections.Generic;

namespace Dietphone.Tools
{
    public interface CloudProvider
    {
        void UploadFile(string name, string data);
        List<string> ListFiles();
        string DownloadFile(string name);
        string GetTokenAcquiringUrl(string callbackUrl);
        CloudToken GetAcquiredToken();
    }

    public interface CloudProviderFactory
    {
        CloudProvider Create();
    }

    public class CloudToken
    {
        public string Token { get; set; }
        public string Secret { get; set; }
    }
}
