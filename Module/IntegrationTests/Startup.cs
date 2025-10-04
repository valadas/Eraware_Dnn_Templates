[assembly: Xunit.TestFramework("IntegrationTests.Startup", "IntegrationTests")]
namespace IntegrationTests
{
    using Xunit.Abstractions;
    using Xunit.Sdk;

    public class Startup : XunitTestFramework
    {
        public Startup(IMessageSink messageSink)
            : base(messageSink)
        {
            Effort.Provider.EffortProviderConfiguration.RegisterProvider();
        }
    }
}
