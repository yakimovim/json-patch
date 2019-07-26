namespace EdlinSoftware.JsonPatch.Utilities
{
    /// <summary>
    /// Types of error handing.
    /// </summary>
    public enum ErrorHandlingTypes
    {
        /// <summary>
        /// Throw an exception on any error.
        /// </summary>
        Error,
        /// <summary>
        /// Skip the error and continue processing.
        /// </summary>
        Skip
    }
}