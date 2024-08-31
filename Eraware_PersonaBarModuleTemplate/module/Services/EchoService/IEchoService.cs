// MIT License
// Copyright $ext_companyname$

namespace $ext_rootnamespace$.Services.EchoService
{
    /// <summary>
    /// Provides services to echo messages back.
    /// </summary>
    public interface IEchoService
    {
        /// <summary>
        /// Echoes the provided message back.
        /// </summary>
        /// <param name="dto">The details about the message to echo back.</param>
        /// <returns>A viewmodel containing the message back.</returns>
        EchoViewModel Echo(EchoDto dto);
    }
}
