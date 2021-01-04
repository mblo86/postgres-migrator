namespace PostgresMigrator
{
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;
    using Npgsql;
    using NpgsqlTypes;
    using PostgresMigrator.Configuration;

    internal class EnsureDatabase
    {
        private readonly MigrationOptions options;

        internal EnsureDatabase(MigrationOptions options)
        {
            this.options = options;
        }

        internal async Task EnsureDatabaseAsync()
        {
            if (await DatabaseExistsAsync(options.Database).ConfigureAwait(false))
            {
                return;
            }

            await CreateDatabaseAsync(options.Database).ConfigureAwait(false);

            if (options.EnsureUsers == null)
            {
                return;
            }

            foreach (var user in options.EnsureUsers)
            {
                if (await UserExistsAsync(options.Database, user.Username).ConfigureAwait(false))
                {
                    continue;
                }

                await CreateUserAsync(options.Database, user.Username, user.Password).ConfigureAwait(false);
            }
        }

        [SuppressMessage(
            "Security",
            "CA2100:Review SQL queries for security vulnerabilities",
            Justification = "All SQL texts are defined in this class.")]
        private static NpgsqlCommand CreateCommand(NpgsqlConnection connection, string sql, params NpgsqlParameter[] parameters)
        {
            var cmd = new NpgsqlCommand(sql, connection);

            if (parameters != null && parameters.Length > 0)
            {
                cmd.Parameters.AddRange(parameters);
            }

            return cmd;
        }

        private static NpgsqlParameter CreateParameter(string name, NpgsqlDbType dataType, object value) =>
           new NpgsqlParameter(name, dataType) { Value = value };

        private async Task<bool> DatabaseExistsAsync(string database)
        {
            using var connection = new NpgsqlConnection(CreateConnectionString("postgres"));
            using var cmd = CreateCommand(
                connection,
                "SELECT EXISTS(SELECT datname FROM pg_catalog.pg_database WHERE datname = @DatabaseName)",
                CreateParameter("DatabaseName", NpgsqlDbType.Varchar, database));

            await connection.OpenAsync().ConfigureAwait(false);
            var result = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
            await connection.CloseAsync().ConfigureAwait(false);

            return (bool)result;
        }

        private async Task CreateDatabaseAsync(string database)
        {
            using var connection = new NpgsqlConnection(CreateConnectionString("postgres"));
            using var cmd = CreateCommand(
                connection,
                $"CREATE DATABASE {database} OWNER {options.MigrationUser.Username}");

            await connection.OpenAsync().ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            await connection.CloseAsync().ConfigureAwait(false);
        }

        private async Task<bool> UserExistsAsync(string database, string username)
        {
            using var connection = new NpgsqlConnection(CreateConnectionString(database));
            using var cmd = CreateCommand(
                connection,
                "SELECT EXISTS(SELECT rolname FROM pg_roles WHERE rolname = @Username)",
                CreateParameter("Username", NpgsqlDbType.Varchar, username));

            await connection.OpenAsync().ConfigureAwait(false);
            var result = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
            await connection.CloseAsync().ConfigureAwait(false);

            return (bool)result;
        }

        private async Task CreateUserAsync(string database, string username, string password)
        {
            using var connection = new NpgsqlConnection(CreateConnectionString(database));
            using var cmd = CreateCommand(
                connection,
                $"CREATE USER {username} WITH PASSWORD '{password}'");

            await connection.OpenAsync().ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            await connection.CloseAsync().ConfigureAwait(false);
        }

        private string CreateConnectionString(string database)
        {
            return $"Host={options.Host};Port={options.Port};User ID={options.MigrationUser.Username};Password={options.MigrationUser.Password};Database={database}";
        }
    }
}
