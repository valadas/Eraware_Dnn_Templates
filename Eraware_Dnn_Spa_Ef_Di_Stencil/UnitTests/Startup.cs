using Xunit.Abstractions;
using Xunit.Sdk;

[assembly: Xunit.TestFramework("UnitTests.Startup", "UnitTests")]
namespace UnitTests
{
    public class Startup : XunitTestFramework
    {
        public Startup(IMessageSink messageSink) : base(messageSink)
        {
            Effort.Provider.EffortProviderConfiguration.RegisterProvider();
        }    
    }
}
