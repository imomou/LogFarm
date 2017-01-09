using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.ObjectBuilder2;
using Moq;
using NUnit.Framework;
using ShadowBlue.LogFarm.Base;
using ShadowBlue.Repository;
using ShadowBlue.Repository.Models;

namespace ShadowBlue.Domain.Test
{
    [TestFixture(Category = LogFarmApplication.TestCategories.Exploratory)]
    public class ElmahDynamoDbErrorLogFixture
    {
        [Test]
        public void LogTest()
        {
            var container = new AutoMoq.AutoMoqer();

            var mock = container.Resolve<IRepository<ElmahError>>();

            var target = container.Resolve<DynamoDbRepository<ElmahError>>();

            var elmahError = new ElmahError
            {
                ApplicationName = "TestApplication",
                Cookies = new Dictionary<string, string>
                {
                    {
                        "TestKeyCookie", "TestKeyValue"
                    }
                }
            };

            //target.Add();

            //mock.Add
        }
    }
}
