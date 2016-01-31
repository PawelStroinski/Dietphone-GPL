using Dietphone.Models;

namespace Dietphone.Tools
{
    public class DropboxProviderFactory : CloudProviderFactory
    {
        private readonly Factories factories;

        public DropboxProviderFactory(Factories factories)
        {
            this.factories = factories;
        }

        public CloudProvider Create()
        {
            var settings = factories.Settings;
            var token = new CloudToken { Secret = settings.CloudSecret, Token = settings.CloudToken };
            return new DropboxProvider(token);
        }
    }
}
