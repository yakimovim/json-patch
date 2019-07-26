using System;
using System.Diagnostics;

namespace EdlinSoftware.JsonPatch.Utilities
{
    /// <summary>
    /// Base class for exceptions int the library.
    /// </summary>
    public abstract class JsonPatchException : Exception
    {
        [DebuggerStepThrough]
        protected JsonPatchException()
        {
        }

        [DebuggerStepThrough]
        protected JsonPatchException(string message) : base(message)
        {
        }

        [DebuggerStepThrough]
        protected JsonPatchException(string message, Exception inner) : base(message, inner)
        {
        }
    }

    /// <summary>
    /// Represents problem with JSON pointer.
    /// </summary>
    public abstract class JsonPatchPointerException : JsonPatchException
    {
        [DebuggerStepThrough]
        protected JsonPatchPointerException()
        {
        }

        [DebuggerStepThrough]
        protected JsonPatchPointerException(string message) : base(message)
        {
        }

        [DebuggerStepThrough]
        protected JsonPatchPointerException(string message, Exception inner) : base(message, inner)
        {
        }
    }

    /// <summary>
    /// Represents problem during patching process.
    /// </summary>
    public abstract class JsonPatchProcessingException : JsonPatchException
    {
        [DebuggerStepThrough]
        protected JsonPatchProcessingException()
        {
        }

        [DebuggerStepThrough]
        protected JsonPatchProcessingException(string message) : base(message)
        {
        }

        [DebuggerStepThrough]
        protected JsonPatchProcessingException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}