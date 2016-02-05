using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EssentialDotNetConfiguration
{
    [TestClass]
    public class ConfigurationTests
    {
    

        [TestInitialize]
        public void TestInitialize()
        {
            Program.Main();
        }

        [TestMethod]
        public void InMemoryCollectionOnly_SetConfigurationSucessfully()
        {
            Assert.AreEqual<string>(Program.DefaultConnectionString,
                Program.Configuration[$"AppConfiguration:ConnectionString"]);

            Assert.AreEqual<int>(400, 
                Program.Configuration.Get<int>(
                    "AppConfiguration:MainWindow:Height"));
            Assert.AreEqual<int>(42,
                Program.Configuration.Get<int>("AppConfiguration:MainWindow:ScreenBufferSize", 80));

            AppConfiguration appConfiguration = 
                Program.Configuration.Get<AppConfiguration>(nameof(AppConfiguration));
            System.Diagnostics.Trace.Assert(600 == appConfiguration.MainWindow.Width);
        }

        [TestMethod]
        public void FullConfiguration()
        {
            string[] args = { "/Top=42", "-Left=42" };
            Program.Main(args);
            InMemoryCollectionOnly_SetConfigurationSucessfully();
            Assert.AreEqual<int>(42,
                Program.Configuration.Get<int>("AppConfiguration:MainWindow:Left"));
            Assert.AreEqual<int>(42,
                Program.Configuration.Get<int>("Top"));

        }
    }
}
