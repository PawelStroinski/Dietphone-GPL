// The Send method inspired by http://stackoverflow.com/a/23740338
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Dietphone.Tools
{
    public class PostSender
    {
        public Dictionary<string, string> Inputs { get; set; }
        public event EventHandler<PostSenderCompletedEventArgs> Completed;
        private PostSenderCompletedEventArgs completedEventArgs;
        private readonly string targetUrl;
        private const string MEDIA_TYPE = "application/x-www-form-urlencoded";

        public PostSender(string targetUrl)
        {
            Inputs = new Dictionary<string, string>();
            this.targetUrl = targetUrl;
        }

        public void SendAsync()
        {
            var worker = new VerboseBackgroundWorker();
            worker.DoWork += delegate
            {
                Send();
            };
            worker.RunWorkerCompleted += delegate
            {
                OnCompleted(this, completedEventArgs);
            };
            worker.RunWorkerAsync();
        }

        private void Send()
        {
            var client = new HttpClient();
            var encoding = string.Join("&", Inputs.Select(kvp => string.Format("{0}={1}",
                WebUtility.UrlEncode(kvp.Key),
                WebUtility.UrlEncode(WebUtility.UrlEncode(kvp.Value)))));
            var content = new StringContent(encoding, Encoding.UTF8, MEDIA_TYPE);
            client.PostAsync(targetUrl, content)
                .ContinueWith(CreateCompletedEventArgs)
                .Wait();
        }

        private void CreateCompletedEventArgs(Task<HttpResponseMessage> task)
        {
            completedEventArgs = new PostSenderCompletedEventArgs();
            completedEventArgs.Success = task.IsGeneralSuccess() && task.Result.IsSuccessStatusCode;
            completedEventArgs.Result = completedEventArgs.Success
                ? task.Result.Content.ReadAsStringAsync().Result : null;
        }

        protected void OnCompleted(object sender, PostSenderCompletedEventArgs e)
        {
            if (Completed != null)
            {
                Completed(sender, e);
            }
        }
    }

    public class PostSenderCompletedEventArgs : EventArgs
    {
        public bool Success { get; set; }
        public string Result { get; set; }
    }
}
