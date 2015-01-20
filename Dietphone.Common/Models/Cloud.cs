using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Dietphone.Tools;

namespace Dietphone.Models
{
    public interface Cloud
    {
        bool ShouldExport();
        void Export();
        void MakeItExport();
        List<string> ListImports();
        void Import(string name);
    }

    public class CloudImpl : Cloud
    {
        public const byte ADD_DAYS_TO_TODAY = 7;
        private readonly CloudProviderFactory providerFactory;
        private readonly Factories factories;
        private readonly ExportAndImport exportAndImport;
        private CloudProvider provider;

        public CloudImpl(CloudProviderFactory providerFactory, Factories factories, ExportAndImport exportAndImport)
        {
            this.providerFactory = providerFactory;
            this.factories = factories;
            this.exportAndImport = exportAndImport;
        }

        public bool ShouldExport()
        {
            var settings = this.factories.Settings;
            if (settings.CloudSecret == string.Empty && settings.CloudToken == string.Empty)
                return false;
            if (settings.CloudExportDue > DateTime.Today)
                return false;
            return true;
        }

        public void Export()
        {
            if (!ShouldExport())
                return;
            CreateProvider();
            ExportAndUploadFile();
            UpdateDate();
        }

        public void MakeItExport()
        {
            var settings = this.factories.Settings;
            settings.CloudExportDue = DateTime.MinValue;
        }

        public List<string> ListImports()
        {
            CreateProvider();
            return ListNamesFromNewest();
        }

        public void Import(string name)
        {
            CreateProvider();
            DownloadAndImportFile(name);
        }

        private void CreateProvider()
        {
            provider = providerFactory.Create();
        }

        private void ExportAndUploadFile()
        {
            var name = string.Format("{0}.xml", DateTime.Today.ToString("s").Substring(0, 10));
            var data = exportAndImport.Export();
            provider.UploadFile(name, data);
        }

        private void UpdateDate()
        {
            factories.Settings.CloudExportDue = DateTime.Today.AddDays(ADD_DAYS_TO_TODAY);
        }

        private List<string> ListNamesFromNewest()
        {
            return provider
                .ListFiles()
                .Where(name => Regex.IsMatch(name, "^\\d{4}-\\d{2}-\\d{2}\\.xml$", RegexOptions.IgnoreCase))
                .OrderByDescending(name => name)
                .ToList();
        }

        private void DownloadAndImportFile(string name)
        {
            var data = provider.DownloadFile(name);
            exportAndImport.Import(data);
        }
    }    
}
