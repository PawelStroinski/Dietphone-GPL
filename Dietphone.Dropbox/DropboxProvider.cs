using System.Collections.Generic;
using System.Linq;
using System.Text;
using DropNetRT;
using DropNetRT.Models;

namespace Dietphone.Tools
{
    public partial class DropboxProvider : CloudProvider
    {
        private const string PATH = "/";
        private readonly DropNetClient client;

        public DropboxProvider(CloudToken token)
        {
            client = new DropNetClient(apiKey: API_KEY, appSecret: APP_SECRET);
            client.UseSandbox = true;
            if (token.Secret != string.Empty || token.Token != string.Empty)
            {
                var accessToken = new UserLogin { Secret = token.Secret, Token = token.Token };
                client.SetUserToken(accessToken);
            }
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

        public string GetTokenAcquiringUrl(string callbackUrl)
        {
            var requestToken = client.GetRequestToken().Result;
            return client.BuildAuthorizeUrl(requestToken, callbackUrl);
        }

        public CloudToken GetAcquiredToken()
        {
            var accessToken = client.GetAccessToken().Result;
            return new CloudToken { Secret = accessToken.Secret, Token = accessToken.Token };
        }
    }
}
