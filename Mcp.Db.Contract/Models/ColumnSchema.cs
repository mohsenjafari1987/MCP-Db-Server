namespace Mcp.Db.Contract.Models
{
    public class ColumnSchema
    {
        public required string ColumnName { get; init; }
        public required string DataType { get; init; }
        public bool IsNullable { get; init; }
        public string? DefaultValue { get; init; }
        public bool IsPrimaryKey { get; init; }
        public string? Description { get; init; } 
    }

    public class IndexSchema
    {
        public required string IndexName { get; init; }
        public required List<string> Columns { get; init; }
        public bool IsUnique { get; init; }
    }

    public class ForeignKeySchema
    {
        public required string Column { get; init; }
        public required string ReferencesTable { get; init; }
        public required string ReferencesColumn { get; init; }
        public string? OnUpdate { get; init; }
        public string? OnDelete { get; init; }
    }


    public class TableSchema
    {
        public required string TableName { get; init; }
        public string? Description { get; init; }
        public required List<ColumnSchema> Columns { get; init; }
        public required List<IndexSchema> Indexes { get; init; }
        public required List<ForeignKeySchema> Relations { get; init; }
    }

}
