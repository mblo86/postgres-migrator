namespace PostgresMigrator.Configuration
{
    using System;

    public class MigrationConfigurationException : Exception
    {
        public MigrationConfigurationException(string message)
            : base(message)
        {
        }

        public MigrationConfigurationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public MigrationConfigurationException()
        {
        }
    }
}
