namespace PostgresMigrator
{
    using System;
    using System.Reflection;
    using System.Threading.Tasks;
    using FluentMigrator.Runner;
    using FluentMigrator.Runner.Initialization;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using PostgresMigrator.Configuration;

    public static class Migrator
    {
        public static async Task Migrate(params Assembly[] assemblies)
        {
            var serviceProvider = CreateServices(assemblies);

            using var scope = serviceProvider.CreateScope();

            var migrationOptions = scope.ServiceProvider.GetRequiredService<MigrationOptions>();
            if (migrationOptions.EnsureDatabase)
            {
                var ensureDatabase = new EnsureDatabase(migrationOptions);
                await ensureDatabase.EnsureDatabaseAsync().ConfigureAwait(false);
            }

            var runner = serviceProvider.GetRequiredService<IMigrationRunner>();
            runner.MigrateUp();
        }

        private static IServiceProvider CreateServices(Assembly[] assembliesToScan)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .Build();

            var migrationOptions = new MigrationOptions();
            configuration.GetSection("MigrationOptions").Bind(migrationOptions);
            migrationOptions.Validate();

            return new ServiceCollection()
                .AddScoped(provider => migrationOptions)
                .AddFluentMigratorCore()
                .Configure<RunnerOptions>(config => config.Profile = Environment.GetEnvironmentVariable(EnvironmentVariableNames.Profile))
                .ConfigureRunner(config => config
                    .AddPostgres()
                    .WithGlobalConnectionString(migrationOptions.MigrationConnectionString)
                    .WithVersionTable(new VersionTable())
                    .ScanIn(assembliesToScan).For.All())
                    .AddLogging(lb => lb.AddFluentMigratorConsole())
                    .BuildServiceProvider(false);
        }
    }
}
