// MIT License
// Copyright $ext_companyname$

using System;

namespace $ext_rootnamespace$.Providers
{
    /// <summary>
    /// Provides a testable version of DateTime.
    /// </summary>
    internal class DateTimeProvider : IDateTimeProvider
    {
        /// <inheritdoc/>
        public DateTime GetUtcNow() => DateTime.UtcNow;
    }
}
