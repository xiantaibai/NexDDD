using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YSCore.Base.MultiTenants
{

    public class TenantSettings<T> where T : Tenant
    {
        public ConnectionStrings Defaults { get; set; }

        public bool DisableAutomaticPipelineRegistration { get; set; } = false;

        public List<T> Tenants { get; set; }
    }

    public class Tenant
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public string ConnectionString { get; set; }
    }

    public class ConnectionStrings
    {
        public string DBProvider { get; set; }
        public string ConnectionString { get; set; }
    }
}
