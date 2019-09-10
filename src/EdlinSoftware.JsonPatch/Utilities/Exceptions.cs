using System;
using System.Diagnostics;

namespace EdlinSoftware.JsonPatch.Utilities
{
    /// <summary>
    /// Exception of Json patcher.
    /// </summary>
    public class JsonPatchException : Exception
    {
        [DebuggerStepThrough]
        public JsonPatchException(string message) : base(message)
        {
        }

        [DebuggerStepThrough]
        public JsonPatchException(string message, Exception inner) : base(message, inner)
        {
        }
    }

    internal static class JsonPatchMessages
    {
        public static readonly string UnknownPathPointer = "Unknown type of path pointer.";
        public static readonly string PatchOperationShouldBeJsonObject = "Patch operation should be a Json object.";
    }
}