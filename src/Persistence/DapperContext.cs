using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Data;

namespace UTC2_Tool.Context
{
    public class DapperContext
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;
        public DapperContext(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration["PostgreSQL"];
        }
        public IDbConnection CreateConnection()
            => new NpgsqlConnection(_connectionString);
    }
}
