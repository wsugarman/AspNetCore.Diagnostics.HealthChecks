using HealthChecks.SqlServer;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods to configure <see cref="SqlServerHealthCheck"/>.
    /// </summary>
    public static class SqlServerHealthCheckBuilderExtensions
    {
        private const string HEALTH_QUERY = "SELECT 1;";
        private const string NAME = "sqlserver";

        /// <summary>
        /// Add a health check for SqlServer services.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="connectionString">The Sql Server connection string to be used.</param>
        /// <param name="healthQuery">The query to be executed.Optional. If <c>null</c> select 1 is used.</param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'sqlserver' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
        /// <param name="beforeOpenConnectionConfigurer">An optional action executed before the connection is Open on the healthcheck.</param>
        /// <returns>The specified <paramref name="builder"/>.</returns>
        public static IHealthChecksBuilder AddSqlServer(
            this IHealthChecksBuilder builder,
            string connectionString,
            string? healthQuery = default,
            string? name = default,
            HealthStatus? failureStatus = default,
            IEnumerable<string>? tags = default,
            TimeSpan? timeout = default,
            Action<SqlConnection>? beforeOpenConnectionConfigurer = default)
        {
            return builder.AddSqlServer(_ => connectionString, healthQuery, name, failureStatus, tags, timeout, beforeOpenConnectionConfigurer);
        }

        /// <summary>
        /// Add a health check for SqlServer services.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="connectionStringFactory">A factory to build the SQL Server connection string to use.</param>
        /// <param name="healthQuery">The query to be executed.Optional. If <c>null</c> select 1 is used.</param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'sqlserver' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
        /// <param name="beforeOpenConnectionConfigurer">An optional action executed before the connection is Open on the healthcheck.</param>
        /// <returns>The specified <paramref name="builder"/>.</returns>
        public static IHealthChecksBuilder AddSqlServer(
            this IHealthChecksBuilder builder,
            Func<IServiceProvider, string> connectionStringFactory,
            string? healthQuery = default,
            string? name = default,
            HealthStatus? failureStatus = default,
            IEnumerable<string>? tags = default,
            TimeSpan? timeout = default,
            Action<SqlConnection>? beforeOpenConnectionConfigurer = default)
        {
            if (connectionStringFactory == null)
            {
                throw new ArgumentNullException(nameof(connectionStringFactory));
            }

            return builder.Add(new HealthCheckRegistration(
                name ?? NAME,
                sp => new SqlServerHealthCheck(connectionStringFactory(sp), healthQuery ?? HEALTH_QUERY, beforeOpenConnectionConfigurer),
                failureStatus,
                tags,
                timeout));
        }

        /// <summary>
        /// Add a health check for SqlServer services.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="connectionFactory">An asynchronous factory to build the SQL Server connection.</param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'sqlserver' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
        /// <param name="configureCommand">An optional action for configuring the <see cref="SqlCommand"/> used to determine if the service is healthy.</param>
        /// <returns>The specified <paramref name="builder"/>.</returns>
        public static IHealthChecksBuilder AddSqlServer(
            this IHealthChecksBuilder builder,
            Func<CancellationToken, ValueTask<SqlConnection>> connectionFactory,
            string? name = default,
            HealthStatus? failureStatus = default,
            IEnumerable<string>? tags = default,
            TimeSpan? timeout = default,
            Action<SqlCommand>? configureCommand = default)
        {
            if (connectionFactory == null)
            {
                throw new ArgumentNullException(nameof(connectionFactory));
            }

            configureCommand ??= c => c.CommandText = HEALTH_QUERY;

            return builder.Add(new HealthCheckRegistration(
                name ?? NAME,
                sp => new SqlServerHealthCheck(connectionFactory, configureCommand),
                failureStatus,
                tags,
                timeout));
        }

        /// <summary>
        /// Add a health check for SqlServer services.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="connectionFactory">An asynchronous factory to build the SQL Server connection.</param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'sqlserver' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
        /// <param name="configureCommand">An optional action for configuring the <see cref="SqlCommand"/> used to determine if the service is healthy.</param>
        /// <returns>The specified <paramref name="builder"/>.</returns>
        public static IHealthChecksBuilder AddSqlServer(
            this IHealthChecksBuilder builder,
            Func<IServiceProvider, CancellationToken, ValueTask<SqlConnection>> connectionFactory,
            string? name = default,
            HealthStatus? failureStatus = default,
            IEnumerable<string>? tags = default,
            TimeSpan? timeout = default,
            Action<IServiceProvider, SqlCommand>? configureCommand = default)
        {
            if (connectionFactory == null)
            {
                throw new ArgumentNullException(nameof(connectionFactory));
            }

            configureCommand ??= (sp, c) => c.CommandText = HEALTH_QUERY;

            return builder.Add(new HealthCheckRegistration(
                name ?? NAME,
                sp => new SqlServerHealthCheck(t => connectionFactory(sp, t), c => configureCommand(sp, c)),
                failureStatus,
                tags,
                timeout));
        }
    }
}
