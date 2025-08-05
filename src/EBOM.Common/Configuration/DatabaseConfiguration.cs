using System.Net;

namespace EBOM.Common.Configuration
{
    public static class DatabaseConfiguration
    {
        public static string GetConnectionString(bool isDevelopment)
        {
            var hostname = Dns.GetHostName();
            Console.WriteLine($"Detected hostname: {hostname}");
            
            var serverName = hostname switch
            {
                "HOCOM5792122" => "HOCOM5792122\\SQLEXPRESS2019",
                "DESKTOP-DG9LM6V" => "DESKTOP-DG9LM6V\\SW_MSSQLSERVER",
                _ => throw new InvalidOperationException($"Unknown hostname: {hostname}. Please add this hostname to DatabaseConfiguration.cs")
            };

            var database = isDevelopment ? "EBOM_Dev" : "EBOM_Prod";
            Console.WriteLine($"Using SQL Server: {serverName}, Database: {database}");
            
            return $"Server={serverName};Database={database};User ID=FUJITECDEV;Password=FUJITECDEV;MultipleActiveResultSets=true;TrustServerCertificate=True";
        }

        public static string GetMasterConnectionString()
        {
            var hostname = Dns.GetHostName();
            var serverName = hostname switch
            {
                "HOCOM5792122" => "HOCOM5792122\\SQLEXPRESS2019",
                "DESKTOP-DG9LM6V" => "DESKTOP-DG9LM6V\\SW_MSSQLSERVER",
                _ => throw new InvalidOperationException($"Unknown hostname: {hostname}")
            };
            
            return $"Server={serverName};Database=master;User ID=FUJITECDEV;Password=FUJITECDEV;MultipleActiveResultSets=true;TrustServerCertificate=True";
        }
        
        public static string GetWindowsAuthConnectionString()
        {
            var hostname = Dns.GetHostName();
            var serverName = hostname switch
            {
                "HOCOM5792122" => "HOCOM5792122\\SQLEXPRESS2019",
                "DESKTOP-DG9LM6V" => "DESKTOP-DG9LM6V\\SW_MSSQLSERVER",
                _ => throw new InvalidOperationException($"Unknown hostname: {hostname}")
            };
            
            return $"Server={serverName};Database=master;Integrated Security=true;TrustServerCertificate=True";
        }
    }
}