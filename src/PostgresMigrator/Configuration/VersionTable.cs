namespace PostgresMigrator.Configuration
{
    using FluentMigrator.Runner.VersionTableInfo;

    [VersionTableMetaData]
    public class VersionTable : IVersionTableMetaData
    {
        public object ApplicationContext { get; set; }
        public bool OwnsSchema => false;
        public string SchemaName => "migration";
        public string TableName => "db_version";
        public string ColumnName => "version";
        public string DescriptionColumnName => "description";
        public string UniqueIndexName => "ix_db_version_version";
        public string AppliedOnColumnName => "applied_on";
    }
}
