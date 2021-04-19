namespace Krauler
{
    /// <summary>
    ///     Base class for all crawlers.
    /// </summary>
    public interface ICrawler
    {
        /// <summary>
        ///     Name of the crawler.
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     Short description on what it crawls.
        /// </summary>
        public string Description { get; }

        /// <summary>
        ///     Target address/server header.
        /// </summary>
        public ServerHeader ServerHeader { get; }

        /// <summary>
        ///     Called when the crawler is created.
        /// </summary>
        public void OnInitialize();

        /// <summary>
        ///     Called when the crawler should start crawling :D
        /// </summary>
        public void OnDispatch(int i);

        /// <summary>
        ///     Called when the crawler is destroyed.
        /// </summary>
        public void OnDestroy();
    }
}