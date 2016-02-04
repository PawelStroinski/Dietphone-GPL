using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Dietphone.Tools
{
    public class PostSender
    {
        public Dictionary<string, string> Inputs { get; set; }
        public event EventHandler<PostSenderCompletedEventArgs> Completed;
        private readonly string targetUrl;
        private PostSenderCompletedEventArgs completedEventArgs;

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
            var doubleEncoding = Inputs
                .Select(kvp => new KeyValuePair<string, string>(kvp.Key, WebUtility.UrlEncode(kvp.Value)));
            var content = new FormUrlEncodedContent(doubleEncoding);
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
