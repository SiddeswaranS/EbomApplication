using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Data.SqlClient;
using EBOM.Data;

namespace EBOM.Common.Configuration
{
    public static class DatabaseInitializer
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider, IHostEnvironment environment)
        {
            var isDevelopment = environment.IsDevelopment();
            var databaseName = isDevelopment ? "EBOM_Dev" : "EBOM_Prod";
            
            // First, ensure the database exists (this will use Windows Auth if needed)
            await CreateDatabaseIfNotExistsAsync(databaseName);
            
            // Then apply migrations
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<EbomDbContext>();
            
            try
            {
                // Check if we can connect to the database
                var canConnect = await context.Database.CanConnectAsync();
                if (!canConnect)
                {
                    Console.WriteLine($"Cannot connect to database {databaseName}. Attempting to create with Windows Authentication...");
                    await CreateDatabaseWithWindowsAuth(databaseName);
                }
                
                // Now apply migrations
                await context.Database.MigrateAsync();
                Console.WriteLine($"Database {databaseName} migrations applied successfully.");
                
                // Seed data
                await DatabaseSeeder.SeedAsync(serviceProvider);
                Console.WriteLine($"Database {databaseName} seeded successfully.");
            }
            catch (SqlException ex) when (ex.Number == 262) // CREATE DATABASE permission denied
            {
                Console.WriteLine($"EF Migration failed due to permissions. Attempting database creation with Windows Authentication...");
                await CreateDatabaseWithWindowsAuth(databaseName);
                
                // Retry the migration
                await context.Database.MigrateAsync();
                Console.WriteLine($"Database {databaseName} migrations applied successfully after Windows Auth creation.");
                
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
            // Try Windows Authentication first to avoid permission issues
            try
            {
                Console.WriteLine($"Attempting to create database {databaseName} with Windows Authentication...");
                await CreateDatabaseWithWindowsAuth(databaseName);
                return; // Success, exit early
            }
            catch (Exception winAuthEx)
            {
                Console.WriteLine($"Windows Authentication failed: {winAuthEx.Message}");
                Console.WriteLine("Falling back to SQL Server authentication...");
            }
            
            // Fallback to regular SQL authentication
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
                    
                    // Grant permissions to FUJITECDEV user
                    await GrantPermissionsToUser(connection, databaseName, "FUJITECDEV");
                }
                else
                {
                    Console.WriteLine($"Database {databaseName} already exists.");
                }
            }
            catch (SqlException ex) when (ex.Number == 262) // CREATE DATABASE permission denied
            {
                Console.WriteLine($"SQL Authentication also lacks CREATE DATABASE permission.");
                throw new InvalidOperationException(
                    "Cannot create database. Please run the application as Administrator or create the database manually.", ex);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking/creating database: {ex.Message}");
                throw;
            }
        }
        
        private static async Task CreateDatabaseWithWindowsAuth(string databaseName)
        {
            // Use Windows Authentication for local admin access
            var adminConnectionString = DatabaseConfiguration.GetWindowsAuthConnectionString();
            
            using var adminConnection = new SqlConnection(adminConnectionString);
            try
            {
                await adminConnection.OpenAsync();
                Console.WriteLine("Connected using Windows Authentication.");
                
                // Check if database exists
                var checkDbCommand = new SqlCommand(
                    $"SELECT database_id FROM sys.databases WHERE Name = @dbname", 
                    adminConnection);
                checkDbCommand.Parameters.AddWithValue("@dbname", databaseName);
                
                var databaseId = await checkDbCommand.ExecuteScalarAsync();
                
                if (databaseId == null)
                {
                    // Create the database
                    var createDbCommand = new SqlCommand($"CREATE DATABASE [{databaseName}]", adminConnection);
                    await createDbCommand.ExecuteNonQueryAsync();
                    Console.WriteLine($"Database {databaseName} created successfully using Windows Authentication.");
                    
                    // Grant permissions to FUJITECDEV user
                    await GrantPermissionsToUser(adminConnection, databaseName, "FUJITECDEV");
                }
                else
                {
                    Console.WriteLine($"Database {databaseName} already exists.");
                    // Still grant permissions in case they're missing
                    await GrantPermissionsToUser(adminConnection, databaseName, "FUJITECDEV");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating database with Windows Authentication: {ex.Message}");
                Console.WriteLine("Please ensure:");
                Console.WriteLine("1. You are running as an administrator");
                Console.WriteLine("2. SQL Server allows Windows Authentication");
                Console.WriteLine("3. Your Windows account has sysadmin privileges in SQL Server");
                throw;
            }
        }
        
        private static async Task GrantPermissionsToUser(SqlConnection connection, string databaseName, string userName)
        {
            try
            {
                // Create login if it doesn't exist
                var createLoginCommand = new SqlCommand($@"
                    IF NOT EXISTS (SELECT * FROM sys.server_principals WHERE name = '{userName}')
                    BEGIN
                        CREATE LOGIN [{userName}] FROM WINDOWS;
                    END", connection);
                await createLoginCommand.ExecuteNonQueryAsync();
                
                // Switch to the new database and create user
                var useDbCommand = new SqlCommand($"USE [{databaseName}]", connection);
                await useDbCommand.ExecuteNonQueryAsync();
                
                // Create user in database if it doesn't exist
                var createUserCommand = new SqlCommand($@"
                    IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = '{userName}')
                    BEGIN
                        CREATE USER [{userName}] FOR LOGIN [{userName}];
                    END", connection);
                await createUserCommand.ExecuteNonQueryAsync();
                
                // Grant db_owner permissions
                var grantPermissionsCommand = new SqlCommand($@"
                    ALTER ROLE db_owner ADD MEMBER [{userName}];", connection);
                await grantPermissionsCommand.ExecuteNonQueryAsync();
                
                Console.WriteLine($"Granted full permissions to user {userName} on database {databaseName}.");
                
                // Switch back to master
                var useMasterCommand = new SqlCommand("USE [master]", connection);
                await useMasterCommand.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error granting permissions: {ex.Message}");
                // Don't throw - permissions might already exist
            }
        }
    }
}