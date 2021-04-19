using System;
using System.Runtime.Serialization;

namespace Krauler
{
    /// <summary>
    ///     Contains information about the crawl address.
    /// </summary>
    public struct ServerHeader
    {
        public Uri Uri;
        public bool Locked;
        public ushort MaxTrials;

        [IgnoreDataMember] public int Port => Uri.Port;

        [IgnoreDataMember] public string Host => Uri.Host;

        [IgnoreDataMember] public string AbsolutePath => Uri.AbsolutePath;

        [IgnoreDataMember] public string AbsoluteUri => Uri.AbsoluteUri;

        [IgnoreDataMember] public string PathAndQuery => Uri.PathAndQuery;

        [IgnoreDataMember] public string Query => Uri.Query;

        public static readonly ServerHeader DefaultTargetNoProxy = new()
        {
            Uri = new Uri("localhost:8080"),
            Locked = false,
            MaxTrials = 10
        };
    }
}