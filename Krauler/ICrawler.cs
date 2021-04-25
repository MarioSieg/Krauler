namespace Krauler
{
    public interface ICrawler
    {
        /// <summary>
        ///     Called when the crawler is created.
        /// </summary>
        public void OnInitialize();

        /// <summary>
        ///     Called when the crawler should start crawling :D
        /// </summary>
        public void OnDispatch();

        /// <summary>
        ///     Called when the crawler is destroyed.
        /// </summary>
        public void OnDestroy();
    }
}