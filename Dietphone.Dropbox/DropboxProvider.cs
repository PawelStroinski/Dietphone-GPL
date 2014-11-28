using System.Collections.Generic;
using System.Linq;
using System.Text;
using DropNetRT;
using DropNetRT.Models;

namespace Dietphone.Tools
{
    public class DropboxProvider : CloudProvider
    {
        private const string API_KEY = "unjuv12op8rn1hv";
        private const string APP_SECRET = "rzs4wvyeqhyxhlm";
        private const string PATH = "/";
        private readonly DropNetClient client;

        public DropboxProvider(string secret, string token)
        {
            client = new DropNetClient(apiKey: API_KEY, appSecret: APP_SECRET);
            client.UseSandbox = true;
            var accessToken = new UserLogin { Secret = secret, Token = token };
            client.SetUserToken(accessToken);
        }

        public void UploadFile(string name, string data)
        {
            var temp = client.Upload(path: PATH, filename: name, fileData: Encoding.UTF8.GetBytes(data)).Result;
        }

        public List<string> ListFiles()
        {
            var metadata = client.GetMetaData(PATH).Result;
            return metadata
                .Contents
                .Select(item => item.Name)
                .ToList();
        }

        public string DownloadFile(string name)
        {
            var filepath = PATH + name;
            var file = client.GetFile(filepath).Result;
            return Encoding.UTF8.GetString(file, 0, file.Length);
        }
    }
}
