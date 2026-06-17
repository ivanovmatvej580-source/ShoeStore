-- ============================================
-- База данных для магазина обуви StyleStep
-- ============================================

-- Удаляем таблицы, если они существуют (для чистой установки)
DROP TABLE IF EXISTS order_items;
DROP TABLE IF EXISTS orders;
DROP TABLE IF EXISTS products;
DROP TABLE IF EXISTS customers;

-- ============================================
-- Таблица покупателей
-- ============================================
CREATE TABLE customers (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    first_name TEXT NOT NULL,
    last_name TEXT NOT NULL,
    phone TEXT NOT NULL UNIQUE,
    email TEXT,
    birth_date TEXT,
    gender TEXT,
    shoe_size REAL,
    registration_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- ============================================
-- Таблица товаров
-- ============================================
CREATE TABLE products (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    name TEXT NOT NULL,
    brand TEXT NOT NULL,
    category TEXT NOT NULL,
    price REAL NOT NULL,
    discount_price REAL,
    color TEXT,
    stock_quantity INTEGER DEFAULT 0,
    is_active INTEGER DEFAULT 1
);

-- ============================================
-- Таблица заказов
-- ============================================
CREATE TABLE orders (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    customer_id INTEGER,
    order_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    status TEXT DEFAULT 'pending',
    total_amount REAL NOT NULL,
    FOREIGN KEY (customer_id) REFERENCES customers(id)
);

-- ============================================
-- Таблица товаров в заказе (для детализации)
-- ============================================
CREATE TABLE order_items (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    order_id INTEGER NOT NULL,
    product_id INTEGER NOT NULL,
    quantity INTEGER NOT NULL,
    price_at_time REAL NOT NULL,
    size TEXT,
    FOREIGN KEY (order_id) REFERENCES orders(id) ON DELETE CASCADE,
    FOREIGN KEY (product_id) REFERENCES products(id)
);

-- ============================================
-- Индексы для ускорения запросов
-- ============================================
CREATE INDEX idx_orders_customer ON orders(customer_id);
CREATE INDEX idx_orders_date ON orders(order_date);
CREATE INDEX idx_orders_status ON orders(status);
CREATE INDEX idx_products_category ON products(category);
CREATE INDEX idx_products_brand ON products(brand);
CREATE INDEX idx_products_active ON products(is_active);

-- ============================================
-- Начальные данные
-- ============================================

-- Администратор (для входа)
INSERT INTO customers (first_name, last_name, phone, email) 
VALUES ('Admin', 'System', 'admin', 'admin@shop.com');

-- Товары
INSERT INTO products (name, brand, category, price, discount_price, color, stock_quantity) 
VALUES 
    ('Air Max 270', 'Nike', 'Кроссовки', 8999, 8099, 'Белый', 25),
    ('Gazelle', 'Adidas', 'Кроссовки', 7499, NULL, 'Синий', 18),
    ('Timberland', 'Timberland', 'Ботинки', 15999, 13999, 'Коричневый', 8),
    ('Кеды', 'Converse', 'Кеды', 4999, 4499, 'Красный', 30),
    ('Сапоги', 'Ralf Ringer', 'Сапоги', 8999, 7999, 'Черный', 10),
    ('Classic Leather', 'Reebok', 'Кроссовки', 6999, 5999, 'Белый', 15),
    ('Superstar', 'Adidas', 'Кроссовки', 8999, NULL, 'Черный', 12),
    ('Chuck Taylor', 'Converse', 'Кеды', 5499, 4999, 'Черный', 20),
    ('Air Force 1', 'Nike', 'Кроссовки', 9999, 8999, 'Белый', 22),
    ('Ботинки', 'Timberland', 'Ботинки', 17999, 15999, 'Черный', 5);

-- Тестовые покупатели (для демонстрации)
INSERT INTO customers (first_name, last_name, phone, email, birth_date, gender, shoe_size) 
VALUES 
    ('Иван', 'Петров', '+79001234567', 'ivan@mail.ru', '1990-05-15', 'Мужской', 42),
    ('Мария', 'Иванова', '+79007654321', 'maria@mail.ru', '1995-08-20', 'Женский', 38),
    ('Алексей', 'Сидоров', '+79009876543', 'alex@mail.ru', '1988-12-01', 'Мужской', 44);

-- Тестовые заказы
INSERT INTO orders (customer_id, order_date, status, total_amount) 
VALUES 
    (2, '2026-01-15 10:30:00', 'confirmed', 16998),
    (2, '2026-01-20 14:20:00', 'pending', 8999),
    (3, '2026-01-25 09:15:00', 'confirmed', 24998),
    (1, '2026-02-01 16:45:00', 'confirmed', 8099);

-- Тестовые товары в заказах
INSERT INTO order_items (order_id, product_id, quantity, price_at_time, size) 
VALUES 
    (1, 1, 1, 8099, '42'),
    (1, 2, 1, 7499, '41'),
    (2, 4, 1, 4499, '38'),
    (2, 1, 1, 8999, '42'),
    (3, 3, 1, 15999, '43'),
    (3, 4, 2, 4999, '39'),
    (4, 1, 1, 8099, '41');