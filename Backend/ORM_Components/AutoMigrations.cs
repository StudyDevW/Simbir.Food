
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Middleware_Components.Services;
using Npgsql;
using ORM_Components.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ORM_Components
{
    public class AutoMigrations
    {
        private readonly DataContext _dbcontext;
        private readonly ILogger _logger;
        private readonly ICacheService _cache;
        public AutoMigrations(DataContext context, ICacheService cache)
        {
            _logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger("Migrations");
            _dbcontext = context;
            _cache = cache;
        }

        private async Task<bool> CheckIfTableExistsAsync(DbContext context, string tableName)
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

        private void BackupDatabase(string nameDatabase)
        {
            switch (nameDatabase)
            {
                case "userTable":
                    _cache.WriteKeyInStorageObject($"backup_{nameDatabase}", _dbcontext.userTable.AsNoTracking().ToList(), DateTime.UtcNow.AddDays(5));
                    break;
                case "courierTable":
                    _cache.WriteKeyInStorageObject($"backup_{nameDatabase}", _dbcontext.courierTable.AsNoTracking().ToList(), DateTime.UtcNow.AddDays(5));
                    break;
                case "orderItemsTable":
                    _cache.WriteKeyInStorageObject($"backup_{nameDatabase}", _dbcontext.orderItemsTable.AsNoTracking().ToList(), DateTime.UtcNow.AddDays(5));
                    break;
                case "orderTable":
                    _cache.WriteKeyInStorageObject($"backup_{nameDatabase}", _dbcontext.orderTable.AsNoTracking().ToList(), DateTime.UtcNow.AddDays(5));
                    break;
                case "restaurantTable":
                    _cache.WriteKeyInStorageObject($"backup_{nameDatabase}", _dbcontext.restaurantTable.AsNoTracking().ToList(), DateTime.UtcNow.AddDays(5));
                    break;
                case "restaurantFoodItemsTable":
                    _cache.WriteKeyInStorageObject($"backup_{nameDatabase}", _dbcontext.restaurantFoodItemsTable.AsNoTracking().ToList(), DateTime.UtcNow.AddDays(5));
                    break;
                case "reviewTable":
                    _cache.WriteKeyInStorageObject($"backup_{nameDatabase}", _dbcontext.reviewTable.AsNoTracking().ToList(), DateTime.UtcNow.AddDays(5));
                    break;
            }
        }

        private void CompleteBackup()
        {
            if (_cache.CheckExistKeysStorage<List<UserTable>>("backup_userTable"))
            {
                foreach (var userTableBackup in _cache.GetKeyFromStorage<List<UserTable>>("backup_userTable"))
                {
                    _dbcontext.userTable.Add(userTableBackup);
                    _dbcontext.SaveChanges();
                }

                _cache.DeleteKeyFromStorage("backup_userTable");
            }

            if (_cache.CheckExistKeysStorage<List<CourierTable>>("backup_courierTable"))
            {
                foreach (var courierTableBackup in _cache.GetKeyFromStorage<List<CourierTable>>("backup_courierTable"))
                {
                    _dbcontext.courierTable.Add(courierTableBackup);
                    _dbcontext.SaveChanges();
                }

                _cache.DeleteKeyFromStorage("backup_courierTable");
            }

            if (_cache.CheckExistKeysStorage<List<OrderItemsTable>>("backup_orderItemsTable"))
            {
                foreach (var orderItemsTableBackup in _cache.GetKeyFromStorage<List<OrderItemsTable>>("backup_orderItemsTable"))
                {
                    _dbcontext.orderItemsTable.Add(orderItemsTableBackup);
                    _dbcontext.SaveChanges();
                }
         
                _cache.DeleteKeyFromStorage("backup_orderItemsTable");
            }

            if (_cache.CheckExistKeysStorage<List<OrderTable>>("backup_orderTable"))
            {
                foreach (var orderTableBackup in _cache.GetKeyFromStorage<List<OrderTable>>("backup_orderTable"))
                {
                    _dbcontext.orderTable.Add(orderTableBackup);
                    _dbcontext.SaveChanges();
                }
           
                _cache.DeleteKeyFromStorage("backup_orderTable");
            }

            if (_cache.CheckExistKeysStorage<List<RestaurantTable>>("backup_restaurantTable"))
            {
                foreach (var restaurantTableBackup in _cache.GetKeyFromStorage<List<RestaurantTable>>("backup_restaurantTable"))
                {
                    _dbcontext.restaurantTable.Add(restaurantTableBackup);
                    _dbcontext.SaveChanges();
                }

                _cache.DeleteKeyFromStorage("backup_restaurantTable");
            }

            if (_cache.CheckExistKeysStorage<List<RestaurantFoodItemsTable>>("backup_restaurantFoodItemsTable"))
            {
                foreach (var restaurantFoodItemsTableBackup in _cache.GetKeyFromStorage<List<RestaurantFoodItemsTable>>("backup_restaurantFoodItemsTable"))
                {
                    _dbcontext.restaurantFoodItemsTable.Add(restaurantFoodItemsTableBackup);
                    _dbcontext.SaveChanges();
                }

                _cache.DeleteKeyFromStorage("backup_restaurantFoodItemsTable");
            }

            if (_cache.CheckExistKeysStorage <List<ReviewTable>>("backup_reviewTable"))
            {
                foreach (var reviewTableBackup in _cache.GetKeyFromStorage<List<ReviewTable>>("backup_reviewTable"))
                {
                    _dbcontext.reviewTable.Add(reviewTableBackup);
                    _dbcontext.SaveChanges();
                }

                _cache.DeleteKeyFromStorage("backup_reviewTable");
            }
        }

        public async Task EnsureDatabaseInitializedAsync()
        {

            List<string> tableNamesArray = new List<string>()
            {
                "userTable",
                "courierTable",
                "orderItemsTable",
                "orderTable",
                "restaurantTable",
                "restaurantFoodItemsTable",
                "reviewTable",
            };

            List<string> tablesToBackup = new List<string>()  
            {
                "userTable",
                "courierTable",
                "orderItemsTable",
                "orderTable",
                "restaurantTable",
                "restaurantFoodItemsTable",
                "reviewTable",
            };

            List<string> tablesNotExist = new List<string>();

            for (int i = 0; i < tableNamesArray.Count; i++)
            {
                var tableExists = await CheckIfTableExistsAsync(_dbcontext, tableNamesArray[i]);

                if (!tableExists)
                {
                    _logger.LogWarning($"Таблица {tableNamesArray[i]} отсутствует");

                    tablesNotExist.Add(tableNamesArray[i]);
                    tablesToBackup.Remove(tableNamesArray[i]);
                }
            }

            //Если хоть одна таблица отсутствует
            if (tablesNotExist.Count > 0)
            {
                //Выполняем бекап всех данных из таблиц
                if (tablesToBackup.Count > 0)
                {
                    _logger.LogInformation($"Есть таблицы для резервного копирования");

                    for (int i = 0; i < tablesToBackup.Count; i++)
                        BackupDatabase(tablesToBackup[i]);

                    _logger.LogInformation($"Все таблицы скопированы");
                }


                await _dbcontext.Database.EnsureDeletedAsync();
                await _dbcontext.Database.MigrateAsync();

                _logger.LogInformation($"Миграции выполнены успешно");

                CompleteBackup();

                _logger.LogInformation($"Если было резервное копирование, то данные восстановлены");

                tablesNotExist.Clear();
            }
          
        }

   
    }
}
