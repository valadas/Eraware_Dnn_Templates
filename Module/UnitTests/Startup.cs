[assembly: Xunit.TestFramework("UnitTests.Startup", "UnitTests")]
namespace UnitTests
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