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
        Throw,
        /// <summary>
        /// Skip the error and continue processing.
        /// </summary>
        Skip
    }

    /// <summary>
    /// Provides information about error handling type.
    /// </summary>
    internal interface IErrorHandlingTypeProvider
    {
        /// <summary>
        /// Type of error handling. Null means global error handling type.
        /// </summary>
        ErrorHandlingTypes? ErrorHandlingType { get; }
    }
}