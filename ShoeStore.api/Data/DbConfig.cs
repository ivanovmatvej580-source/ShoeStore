using System;
using System.IO;

namespace ShoeStore.api.Data;

public static class DbConfig
{
    private static string? _connectionString;

    public static string ConnectionString
    {
        get
        {
            if (_connectionString == null)
            {
                string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Shoe.db");
                _connectionString = $"Data Source={dbPath}";
            }
            return _connectionString;
        }
    }
}