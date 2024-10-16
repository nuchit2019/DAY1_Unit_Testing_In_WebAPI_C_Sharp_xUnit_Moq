using Microsoft.Data.SqlClient;
using System.Data;

namespace ProductAPI.Data
{
   
    public class DapperContext
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public DapperContext(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("LocaldbConnection");
        }

        public IDbConnection CreateConnection() => new SqlConnection(_connectionString);
        
        //public IDbConnection CreateConnection()
        //{
        //    return new SqlConnection(_connectionString);
        //}
    }
}
