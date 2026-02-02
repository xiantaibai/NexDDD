using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Update;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure.Internal;
using Pomelo.EntityFrameworkCore.MySql.Migrations;
using ShardingCore;
using ShardingCore.Core.DbContextCreator;
using ShardingCore.Core.RuntimeContexts;
using ShardingCore.Core.ServiceProviders;
using ShardingCore.Helpers;
using ShardingCore.Sharding.Abstractions;

namespace testApi.MultiTenantDbContext
{
    public class ShardingMySqlMigrationsSqlGenerator : MySqlMigrationsSqlGenerator
    {
        private readonly IShardingRuntimeContext _shardingRuntimeContext;


        public ShardingMySqlMigrationsSqlGenerator(IShardingRuntimeContext shardingRuntimeContext, MigrationsSqlGeneratorDependencies dependencies, ICommandBatchPreparer commandBatchPreparer, IMySqlOptions options)
            : base(dependencies, commandBatchPreparer, options)
        {
            _shardingRuntimeContext = shardingRuntimeContext;
        }
        protected override void Generate(
            MigrationOperation operation,
            IModel model,
            MigrationCommandListBuilder builder)
        {
            // 移除外键约束
            
            var oldCmds = builder.GetCommandList().ToList();
            base.Generate(operation, model, builder);
            var newCmds = builder.GetCommandList().ToList();
            var addCmds = newCmds.Where(x => !oldCmds.Contains(x)).ToList();

            MigrationHelper.Generate(_shardingRuntimeContext, operation, builder, Dependencies.SqlGenerationHelper, addCmds);
        }

    }
}
