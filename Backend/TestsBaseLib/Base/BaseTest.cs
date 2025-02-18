using Microsoft.EntityFrameworkCore;
using ORM_Components;
using ORM_Components.Tables;

namespace TestsBaseLib.Base;

public class BaseTest
{
    protected async Task<DataContext> GetDbContext()
    {
        var model = new ModelBuilder();
        model.Entity<UserTable>().Property(x => x.roles)
            .HasConversion<string>(x => string.Join(",", x),
                x => x.Split(",", StringSplitOptions.RemoveEmptyEntries));

        model.Entity<UserTable>().HasKey(x => x.Id);
        model.Entity<UserTable>().Property(x => x.Id).HasColumnType("uuid");
        model.Entity<UserTable>().Property(x => x.address).HasColumnType("text");
        model.Entity<UserTable>().Property(x => x.avatarImage).HasColumnType("text");
        model.Entity<UserTable>().Property(x => x.email).HasColumnType("text");
        model.Entity<UserTable>().Property(x => x.login).HasColumnType("text");
        model.Entity<UserTable>().Property(x => x.password).HasColumnType("text");
        model.Entity<UserTable>().Property(x => x.name).HasColumnType("text");
        model.Entity<UserTable>().Property(x => x.phone_number).HasColumnType("text");

        var builder = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .UseModel(model.FinalizeModel());

        var context = new DataContext(builder.Options);
        await context.Database.EnsureCreatedAsync();

        return context;
    }
}
