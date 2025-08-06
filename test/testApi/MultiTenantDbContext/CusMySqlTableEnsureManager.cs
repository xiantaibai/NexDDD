using ShardingCore.Core.VirtualRoutes.TableRoutes.RouteTails.Abstractions;
using ShardingCore.TableExists.Abstractions;
using System.Data.Common;

namespace testApi.MultiTenantDbContext;

/// <summary>
/// 自定表 初始化
/// </summary>
public class CusMySqlTableEnsureManager : AbstractTableEnsureManager
{

    public CusMySqlTableEnsureManager(IRouteTailFactory routeTailFactory) : base(routeTailFactory)
    {
    }
    private const string Tables = "Tables";
    private const string TABLE_SCHEMA = "TABLE_SCHEMA";
    private const string TABLE_NAME = "TABLE_NAME";

    public override ISet<string> DoGetExistTables(DbConnection connection, string dataSourceName)
    {
        var database = connection.Database;
        ISet<string> result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // 使用 SQL 查询直接获取表信息
        string query = @"
        SELECT TABLE_NAME
        FROM INFORMATION_SCHEMA.TABLES
        WHERE TABLE_SCHEMA = @Database";

        using (var command = connection.CreateCommand())
        {
            command.CommandText = query;
            var databaseParam = command.CreateParameter();
            databaseParam.ParameterName = "@Database";
            databaseParam.Value = database;
            command.Parameters.Add(databaseParam);

            // connection.Open();
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var tableName = reader.GetString(0);
                    result.Add(tableName);
                }
            }
        }

        return result;
    }
}