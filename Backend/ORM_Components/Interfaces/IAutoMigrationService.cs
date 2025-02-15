namespace ORM_Components.Interfaces
{
    public interface IAutoMigrationService
    {
        public Task EnsureDatabaseInitializedAsync();
    }
}
