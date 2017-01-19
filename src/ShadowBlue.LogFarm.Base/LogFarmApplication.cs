using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using ShadowBlue.LogFarm.Base.Properties;

namespace ShadowBlue.LogFarm.Base
{
    public static class LogFarmApplication
    {
        public static class TestCategories
        {
            public const string Unit = "Unit";
            public const string Integration = "Integration";
            public const string Exploratory = "Exploratorty";
        }
    }
}
