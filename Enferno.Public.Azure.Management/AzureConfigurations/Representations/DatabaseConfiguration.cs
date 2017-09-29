namespace Enferno.Public.Azure.Management.AzureConfigurations.Representations
{
    public struct DatabaseConfiguration
    {
        public string Name;
        public string ServerName;
        public string Edition;
        public int MaximumDatabaseSizeInGB;
        public string DatabaseServerAdminUser;
        public string DatabaseServerAdminPassword;
    }
}