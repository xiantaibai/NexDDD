using Microsoft.EntityFrameworkCore;

namespace NexDDD.Provider.EntityFrameworkCore.Idempotency;

/// <summary>
/// 幂等表初始化器。
/// 负责幂等执行模块中的 IdempotencyTableInitializer 能力，并限定其与调用方及基础设施组件的协作边界。
/// </summary>
internal static class IdempotencyTableInitializer
{
    /// <summary>
    /// 获取初始化 SQL。
    /// </summary>
    /// <param name="providerName">数据库提供程序名称。</param>
    /// <returns></returns>
    /// <exception cref="NotSupportedException"></exception>
    public static string GetCreateTableSql(string? providerName)
    {
        if (string.IsNullOrWhiteSpace(providerName))
        {
            throw new NotSupportedException("无法识别当前数据库提供程序。");
        }

        if (providerName.Contains("MySql", StringComparison.OrdinalIgnoreCase))
        {
            return """
CREATE TABLE IF NOT EXISTS `nexddd_idempotency_records` (
  `Id` bigint NOT NULL,
  `TypeCode` varchar(128) NOT NULL,
  `Key` varchar(512) NOT NULL,
  `KeyHash` varchar(128) NOT NULL,
  `RequestId` varchar(64) NOT NULL,
  `CreateTime` datetime(6) NOT NULL,
  `ExpireTime` datetime(6) NOT NULL,
  `Status` int NOT NULL,
  `LastModifyTime` datetime(6) NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `UX_nexddd_idempotency_records_TypeCode_KeyHash` (`TypeCode`, `KeyHash`),
  KEY `IX_nexddd_idempotency_records_ExpireTime` (`ExpireTime`),
  KEY `IX_nexddd_idempotency_records_Status_ExpireTime` (`Status`, `ExpireTime`)
);
""";
        }

        if (providerName.Contains("SqlServer", StringComparison.OrdinalIgnoreCase))
        {
            return """
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[nexddd_idempotency_records]') AND type = 'U')
BEGIN
    CREATE TABLE [dbo].[nexddd_idempotency_records] (
        [Id] bigint NOT NULL,
        [TypeCode] nvarchar(128) NOT NULL,
        [Key] nvarchar(512) NOT NULL,
        [KeyHash] nvarchar(128) NOT NULL,
        [RequestId] varchar(64) NOT NULL,
        [CreateTime] datetime2 NOT NULL,
        [ExpireTime] datetime2 NOT NULL,
        [Status] int NOT NULL,
        [LastModifyTime] datetime2 NULL,
        CONSTRAINT [PK_nexddd_idempotency_records] PRIMARY KEY ([Id])
    );
    CREATE UNIQUE INDEX [UX_nexddd_idempotency_records_TypeCode_KeyHash] ON [dbo].[nexddd_idempotency_records]([TypeCode], [KeyHash]);
    CREATE INDEX [IX_nexddd_idempotency_records_ExpireTime] ON [dbo].[nexddd_idempotency_records]([ExpireTime]);
    CREATE INDEX [IX_nexddd_idempotency_records_Status_ExpireTime] ON [dbo].[nexddd_idempotency_records]([Status], [ExpireTime]);
END
""";
        }

        if (providerName.Contains("Npgsql", StringComparison.OrdinalIgnoreCase)
            || providerName.Contains("PostgreSQL", StringComparison.OrdinalIgnoreCase))
        {
            return """
CREATE TABLE IF NOT EXISTS nexddd_idempotency_records (
  "Id" bigint PRIMARY KEY,
  "TypeCode" varchar(128) NOT NULL,
  "Key" varchar(512) NOT NULL,
  "KeyHash" varchar(128) NOT NULL,
  "RequestId" varchar(64) NOT NULL,
  "CreateTime" timestamp without time zone NOT NULL,
  "ExpireTime" timestamp without time zone NOT NULL,
  "LastModifyTime" timestamp without time zone NULL,
  "Status" integer NOT NULL
);
CREATE UNIQUE INDEX IF NOT EXISTS "UX_nexddd_idempotency_records_TypeCode_KeyHash" ON nexddd_idempotency_records ("TypeCode", "KeyHash");
CREATE INDEX IF NOT EXISTS "IX_nexddd_idempotency_records_ExpireTime" ON nexddd_idempotency_records ("ExpireTime");
CREATE INDEX IF NOT EXISTS "IX_nexddd_idempotency_records_Status_ExpireTime" ON nexddd_idempotency_records ("Status", "ExpireTime");
""";
        }

        throw new NotSupportedException($"当前数据库提供程序暂不支持自动初始化幂等表：{providerName}");
    }
}
