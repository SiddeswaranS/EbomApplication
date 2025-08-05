using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Data.SqlClient;
using EBOM.Data;

namespace EBOM.Common.Configuration
{
    public static class DatabaseInitializer
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider, IHostEnvironment environment)
        {
            var isDevelopment = environment.IsDevelopment();
            var databaseName = isDevelopment ? "EBOM_Dev" : "EBOM_Prod";
            
            // First, try to create the database if it doesn't exist
            await CreateDatabaseIfNotExistsAsync(databaseName);
            
            // Then apply migrations
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<EbomDbContext>();
            
            try
            {
                await context.Database.MigrateAsync();
                Console.WriteLine($"Database {databaseName} migrations applied successfully.");
                
                // Seed data
                await DatabaseSeeder.SeedAsync(serviceProvider);
                Console.WriteLine($"Database {databaseName} seeded successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error applying migrations: {ex.Message}");
                throw;
            }
        }
        
        private static async Task CreateDatabaseIfNotExistsAsync(string databaseName)
        {
            var masterConnectionString = DatabaseConfiguration.GetMasterConnectionString();
            
            using var connection = new SqlConnection(masterConnectionString);
            try
            {
                await connection.OpenAsync();
                
                // Check if database exists
                var checkDbCommand = new SqlCommand(
                    $"SELECT database_id FROM sys.databases WHERE Name = @dbname", 
                    connection);
                checkDbCommand.Parameters.AddWithValue("@dbname", databaseName);
                
                var databaseId = await checkDbCommand.ExecuteScalarAsync();
                
                if (databaseId == null)
                {
                    // Try to create the database
                    var createDbCommand = new SqlCommand($"CREATE DATABASE [{databaseName}]", connection);
                    await createDbCommand.ExecuteNonQueryAsync();
                    Console.WriteLine($"Database {databaseName} created successfully.");
                }
                else
                {
                    Console.WriteLine($"Database {databaseName} already exists.");
                }
            }
            catch (SqlException ex) when (ex.Number == 262) // CREATE DATABASE permission denied
            {
                Console.WriteLine($"Warning: Cannot create database {databaseName}. Permission denied.");
                Console.WriteLine("Please ensure the database exists or run with appropriate permissions.");
                // Don't throw - let EF Core handle the connection
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking/creating database: {ex.Message}");
                throw;
            }
        }
    }
}