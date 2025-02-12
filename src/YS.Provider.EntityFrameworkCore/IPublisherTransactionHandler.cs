using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YSCore.Provider.EntityFrameworkCore
{
    public interface IPublisherTransactionHandler
    {
        IDbContextTransaction BeginTransaction(DbContext context);
    }
}
