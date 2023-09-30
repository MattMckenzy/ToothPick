namespace ToothPick.Services
{
    /// <summary>
    /// An interface that defines a means to access a rest service's configuration.
    /// </summary>
    public interface IRestServiceProvider
    {
        /// <summary>
        /// Retrieves the scope needed to access the service.
        /// </summary>
        /// <returns>The scope.</returns>
        Task<string> GetScope();

        /// <summary>
        /// Retrieves the header needed to be authorized on the service.
        /// </summary>
        /// <returns>The header.</returns>
        Task<string> GetHeader();

        /// <summary>
        /// Retrieves the token needed to be authorized on the service.
        /// </summary>
        /// <returns>The token.</returns>
        Task<string> GetToken();

        /// <summary>
        /// Retrieves the service uri configuration value.
        /// </summary>
        /// <returns>The service uri.</returns>
        Task<Uri> GetServiceUri();
    }
}