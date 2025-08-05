using Microsoft.Extensions.DependencyInjection;
using EBOM.Data;
using EBOM.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace EBOM.Common.Configuration
{
    public static class DatabaseSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<EbomDbContext>();
            
            try
            {
                // Seed EntityDataTypes if not exists
                await SeedEntityDataTypesAsync(context);
                
                // Seed default users if not exists
                await SeedDefaultUsersAsync(context);
                
                // Seed other initial data as needed
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error seeding database: {ex.Message}");
                throw;
            }
        }
        
        private static async Task SeedEntityDataTypesAsync(EbomDbContext context)
        {
            if (await context.EntityDataTypes.AnyAsync())
                return;
                
            var dataTypes = new List<EntityDataType>
            {
                new EntityDataType { DataTypeID = 1, DataTypeName = "STRING", DataTypeDescription = "Text data type" },
                new EntityDataType { DataTypeID = 2, DataTypeName = "NUMBER", DataTypeDescription = "Numeric data type" },
                new EntityDataType { DataTypeID = 3, DataTypeName = "DATE", DataTypeDescription = "Date data type" },
                new EntityDataType { DataTypeID = 4, DataTypeName = "BOOLEAN", DataTypeDescription = "Boolean data type" },
                new EntityDataType { DataTypeID = 5, DataTypeName = "DECIMAL", DataTypeDescription = "Decimal data type" }
            };
            
            await context.EntityDataTypes.AddRangeAsync(dataTypes);
            Console.WriteLine("EntityDataTypes seeded successfully.");
        }
        
        private static async Task SeedDefaultUsersAsync(EbomDbContext context)
        {
            if (await context.UserMasters.AnyAsync())
                return;
                
            var defaultUsers = new List<UserMaster>
            {
                new UserMaster
                {
                    UserID = 1,
                    UserName = "admin",
                    UserEmail = "admin@ebom.com",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = 1
                },
                new UserMaster
                {
                    UserID = 2,
                    UserName = "user",
                    UserEmail = "user@ebom.com",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = 1
                }
            };
            
            await context.UserMasters.AddRangeAsync(defaultUsers);
            Console.WriteLine("Default users seeded successfully.");
        }
    }
}