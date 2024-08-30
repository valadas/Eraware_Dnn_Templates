// MIT License
// Copyright $ext_companyname$

namespace $ext_rootnamespace$.Services.EchoService
{
    /// <inheritdoc cref="IEchoService"/>
    internal class EchoService : IEchoService
    {
        /// <inheritdoc/>
        public EchoViewModel Echo(EchoDto dto)
        {
            return new EchoViewModel { Message = dto.Message };
        }
    }
}
