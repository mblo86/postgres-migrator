namespace PostgresMigrator.Configuration
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    public class MigrationOptions
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string Database { get; set; }
        public bool EnsureDatabase { get; set; }
        public DatabaseUser MigrationUser { get; set; }

        [SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Instantiated with reflection")]
        public ICollection<DatabaseUser> EnsureUsers { get; set; }

        public string MigrationConnectionString => $"Host={Host};Port={Port};Database={Database};User ID={MigrationUser.Username};Password={MigrationUser.Password}";

        [SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "Exception message")]
        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(Host))
            {
                throw new MigrationConfigurationException($"'{nameof(Host)}' must be specified");
            }

            if (Port <= 0)
            {
                throw new MigrationConfigurationException($"'{nameof(Port)}' must be larger than zero (0)");
            }

            if (string.IsNullOrWhiteSpace(Database))
            {
                throw new MigrationConfigurationException($"'{nameof(Database)}' must be specified");
            }

            if (MigrationUser == null || string.IsNullOrWhiteSpace(MigrationUser.Username) || string.IsNullOrWhiteSpace(MigrationUser.Password))
            {
                throw new MigrationConfigurationException($"{nameof(MigrationUser)}' is not configured correctly. You need to specify a username and a password.");
            }
        }
    }
}
