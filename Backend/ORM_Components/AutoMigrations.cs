
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORM_Components
{
    public static class AutoMigrations
    {
        private static async Task<bool> CheckIfTableExistsAsync(DbContext context, string tableName)
        {
            var connection = (NpgsqlConnection)context.Database.GetDbConnection();
            await connection.OpenAsync();

            var exists = false;

            var command = new NpgsqlCommand(
                $"SELECT EXISTS (SELECT FROM pg_tables WHERE schemaname = 'public' AND tablename = '{tableName}');",
                connection);

            exists = (bool)await command.ExecuteScalarAsync();

            await connection.CloseAsync();
            return exists;
        }

        public static async Task EnsureDatabaseInitializedAsync(DataContext dataContext)
        {
            var context = dataContext;

            List<string> tableNamesArray = new List<string>()
            {
                "clientTable",
                "courierTable",
                "orderItemsTable",
                "orderTable",
                "restaurantTable",
                "restaurantFoodItemsTable",
                "reviewTable",
            };

            for (int i = 0; i < tableNamesArray.Count; i++)
            {
                var tableExists = await CheckIfTableExistsAsync(context, tableNamesArray[i]);

                if (!tableExists)
                {
                    await context.Database.MigrateAsync();
                }
            }
        }
    }
}
