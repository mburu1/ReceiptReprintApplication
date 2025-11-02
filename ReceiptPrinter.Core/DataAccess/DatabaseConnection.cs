using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using ReceiptPrinter.Logging;
using ReceiptPrinter.Logging.Interfaces;

namespace ReceiptPrinter.Core.DataAccess
{
    internal class DatabaseConnection : IDisposable
    {
        private readonly string _connectionString;
        private SqlConnection _connection;
        private readonly object _lock = new object();
        private bool _disposed = false;
        private static readonly ILogger _logger = LogManager.GetLogger();

        public DatabaseConnection(string connectionStringName = "receiptreprint.Properties.Settings.KINOTIConnectionString")
        {
            try
            {
                _connectionString = ConfigurationManager.ConnectionStrings[connectionStringName]?.ConnectionString;
                if (string.IsNullOrEmpty(_connectionString))
                {
                    throw new ConfigurationErrorsException($"Connection string '{connectionStringName}' is missing or empty.");
                }
            }
            catch (Exception ex)
            {
                _logger.Fatal("Failed to load connection string", ex);
                throw;
            }
        }

        public async Task<SqlConnection> GetOpenConnectionAsync()
        {
            lock (_lock)
            {
                if (_connection == null)
                {
                    _connection = new SqlConnection(_connectionString);
                }
            }

            try
            {
                if (_connection.State != ConnectionState.Open)
                {
                    await _connection.OpenAsync();
                    _logger.Debug("Database connection opened successfully.");
                }
                return _connection;
            }
            catch (SqlException ex)
            {
                _logger.Error("Failed to open database connection", ex);
                throw new InvalidOperationException("Database connection failed.", ex);
            }
        }

        public async Task<SqlCommand> CreateCommandAsync(string procedureName, params SqlParameter[] parameters)
        {
            var connection = await GetOpenConnectionAsync();
            var cmd = new SqlCommand(procedureName, connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            if (parameters != null)
            {
                cmd.Parameters.AddRange(parameters);
            }

            return cmd;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                lock (_lock)
                {
                    if (_connection != null)
                    {
                        if (_connection.State == ConnectionState.Open)
                        {
                            _connection.Close();
                            _logger.Debug("Database connection closed.");
                        }
                        _connection.Dispose();
                    }
                }
                _disposed = true;
            }
        }
    }
}
