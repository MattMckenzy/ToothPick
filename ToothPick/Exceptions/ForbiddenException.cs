namespace ToothPick.Exceptions
{
    /// <summary>
    /// An exception to be used when there is an issue with forbidden downstream communication.
    /// </summary>
    [Serializable]
    public class ForbiddenException : CommunicationException
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public ForbiddenException()
        {
        }

        /// <summary>
        /// Default constructor with message.
        /// </summary>
        /// <param name="message">The message to set in the exception.</param>
        public ForbiddenException(string message) : base(message)
        {
        }

        /// <summary>
        /// Default constructor with message and inner exception.
        /// </summary>
        /// <param name="message">The message to set in the exception.</param>
        /// <param name="innerException">The inner exception to set in the exception.</param>
        public ForbiddenException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
