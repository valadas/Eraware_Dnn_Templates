
namespace UnitTests.Common
{
    using System;
    using Xunit;
    using $ext_rootnamespace$.Common;

    public class GlobalsTests
    {
        [Fact]
        public void ModulePrefixIsValid()
        {
            var modulePrefix = Globals.ModulePrefix;
            Assert.Equal(expected: "$ext_scopeprefix$_", modulePrefix);
        }
    }
}
