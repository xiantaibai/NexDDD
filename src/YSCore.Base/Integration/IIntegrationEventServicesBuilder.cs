using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YSCore.Base.Integration
{
    public interface IIntegrationEventServicesBuilder
    {
        IServiceCollection Services { get; }
    }
}
