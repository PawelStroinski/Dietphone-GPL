namespace Dietphone.Tools
{
    public class DropboxProviderFactory : CloudProviderFactory
    {
        private readonly string secret;
        private readonly string token;

        public DropboxProviderFactory(string secret, string token)
        {
            this.secret = secret;
            this.token = token;
        }

        public CloudProvider Create()
        {
            return new DropboxProvider(secret: secret, token: token);
        }
    }
}
