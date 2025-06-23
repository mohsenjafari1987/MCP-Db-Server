namespace Mcp.Db.Contract.Models
{
    public class McpQueryResult
    {
        /// <summary>
        /// List of column names in result set
        /// </summary>
        public required List<string> Columns { get; init; }

        /// <summary>
        /// List of row values, each row is a list of values (nullable)
        /// </summary>
        public required List<List<object?>> Rows { get; init; }
    }
}
