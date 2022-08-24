using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.SqlServer
{
    public class SqlServerHealthCheck : IHealthCheck
    {
        private readonly Func<SqlConnection> _connectionFactory;
        private readonly Action<SqlCommand> _configureCommand;

        public SqlServerHealthCheck(string sqlserverconnectionstring, string sql, Action<SqlConnection>? beforeOpenConnectionConfigurer)
            : this(CreateDefaultConnectionFactory(sqlserverconnectionstring, beforeOpenConnectionConfigurer), CreateDefaultCommandDelegate(sql))
        { }

        internal SqlServerHealthCheck(Func<SqlConnection> connectionFactory, Action<SqlCommand> configureCommand)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            _configureCommand = configureCommand ?? throw new ArgumentNullException(nameof(configureCommand));
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                using var connection = _connectionFactory();
                if (connection == null)
                {
                    throw new InvalidOperationException("SQL connection cannot be null.");
                }

                if (connection.State != ConnectionState.Open)
                {
                    await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
                }

                using (var command = connection.CreateCommand())
                {
                    _configureCommand.Invoke(command);

                    // TODO: Allow users to determine which responses are considered healthy
                    _ = await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
                }

                return HealthCheckResult.Healthy();
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }

        private static Func<SqlConnection> CreateDefaultConnectionFactory(string sqlserverconnectionstring, Action<SqlConnection>? beforeOpenConnectionConfigurer)
        {
            if (sqlserverconnectionstring == null)
            {
                throw new ArgumentNullException(nameof(sqlserverconnectionstring));
            }

            return () =>
            {
                var connection = new SqlConnection(sqlserverconnectionstring);
                beforeOpenConnectionConfigurer?.Invoke(connection);

                return connection;
            };
        }

        private static Action<SqlCommand> CreateDefaultCommandDelegate(string sql)
        {
            if (sql == null)
            {
                throw new ArgumentNullException(nameof(sql));
            }

            return command => command.CommandText = sql;
        }
    }
}
