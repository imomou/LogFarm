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
       
        }
    }
}
