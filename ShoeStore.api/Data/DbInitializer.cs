using Dapper;
using Microsoft.Data.Sqlite;
using System;
using System.IO;

namespace ShoeStore.api.Data;

public static class DbInitializer
{
    public static void Initialize()
    {
        string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Shoe.db");
        string connStr = $"Data Source={dbPath}";

        using var connection = new SqliteConnection(connStr);
        connection.Open();

        connection.Execute(@"CREATE TABLE IF NOT EXISTS customers (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            login TEXT NOT NULL UNIQUE,
            password TEXT NOT NULL,
            role TEXT DEFAULT 'user',
            first_name TEXT NOT NULL,
            last_name TEXT NOT NULL,
            phone TEXT NOT NULL,
            email TEXT)");

        connection.Execute(@"CREATE TABLE IF NOT EXISTS products (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            name TEXT NOT NULL,
            brand TEXT NOT NULL,
            category TEXT NOT NULL,
            price REAL NOT NULL,
            discount_price REAL,
            color TEXT,
            stock_quantity INTEGER DEFAULT 0,
            is_active INTEGER DEFAULT 1)");

        var userCount = connection.QueryFirstOrDefault<int>("SELECT COUNT(*) FROM customers");
        if (userCount == 0)
        {
            connection.Execute("INSERT INTO customers (login, password, role, first_name, last_name, phone, email) VALUES ('admin', 'admin123', 'admin', 'Admin', 'System', 'admin', 'admin@shop.com')");
            connection.Execute("INSERT INTO customers (login, password, role, first_name, last_name, phone, email) VALUES ('user', 'user123', 'user', 'User', 'Test', 'user', 'user@shop.com')");
        }

        var productCount = connection.QueryFirstOrDefault<int>("SELECT COUNT(*) FROM products");
        if (productCount == 0)
        {
            connection.Execute("INSERT INTO products (name, brand, category, price, discount_price, color, stock_quantity) VALUES ('Air Max 270', 'Nike', 'Кроссовки', 8999, 8099, 'Белый', 25)");
            connection.Execute("INSERT INTO products (name, brand, category, price, discount_price, color, stock_quantity) VALUES ('Gazelle', 'Adidas', 'Кроссовки', 7499, NULL, 'Синий', 18)");
            connection.Execute("INSERT INTO products (name, brand, category, price, discount_price, color, stock_quantity) VALUES ('Timberland Premium', 'Timberland', 'Ботинки', 15999, 13999, 'Коричневый', 8)");
            connection.Execute("INSERT INTO products (name, brand, category, price, discount_price, color, stock_quantity) VALUES ('Classic Leather', 'Reebok', 'Кроссовки', 6999, NULL, 'Черный', 15)");
            connection.Execute("INSERT INTO products (name, brand, category, price, discount_price, color, stock_quantity) VALUES ('Old Skool', 'Vans', 'Кеды', 5999, 4999, 'Черно-белый', 30)");
        }
    }
}