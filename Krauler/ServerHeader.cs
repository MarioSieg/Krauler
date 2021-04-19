using System;

namespace Krauler
{
    /// <summary>
    /// Contains information about the crawl address.
    /// </summary>
    public struct ServerHeader
    {
        public Uri Uri;
        public bool Locked;

        public int Port => Uri.Port;
        public string Host => Uri.Host;
        public string AbsolutePath => Uri.AbsolutePath;
        public string AbsoluteUri => Uri.AbsoluteUri;
        public string PathAndQuery => Uri.PathAndQuery;
        public string Query => Uri.Query;

        public static readonly ServerHeader DefaultTargetNoProxy = new()
        {
            Uri = new Uri("localhost:8080"),
            Locked = false
        };
}
}
