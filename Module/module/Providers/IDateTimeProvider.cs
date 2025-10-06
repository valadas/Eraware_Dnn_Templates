// MIT License
// Copyright $ext_companyname$

using System;

namespace $ext_rootnamespace$.Providers
{
    /// <summary>
    /// Provides a testable version of DateTime.
    /// </summary>
    public interface IDateTimeProvider
    {
        /// <summary>
        /// Gets the current UTC date and time.
        /// </summary>
        /// <returns>The current UTC date and time.</returns>
        DateTime GetUtcNow();
    }
}
