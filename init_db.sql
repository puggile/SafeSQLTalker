-- Tabelle di Esempio: E-commerce scenario

CREATE TABLE IF NOT EXISTS Products (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    Category TEXT NOT NULL,
    Price DECIMAL(10,2) NOT NULL,
    StockLevel INTEGER NOT NULL
);

CREATE TABLE IF NOT EXISTS Orders (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    OrderDate DATE NOT NULL,
    CustomerRegion TEXT NOT NULL, -- Es. 'Milano', 'Roma'
    TotalAmount DECIMAL(10,2) NOT NULL
);

CREATE TABLE IF NOT EXISTS OrderItems (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    OrderId INTEGER NOT NULL,
    ProductId INTEGER NOT NULL,
    Quantity INTEGER NOT NULL,
    FOREIGN KEY(OrderId) REFERENCES Orders(Id),
    FOREIGN KEY(ProductId) REFERENCES Products(Id)
);

-- Dati Mock (Dati finti per fare bella figura nella demo)
INSERT INTO Products (Name, Category, Price, StockLevel) VALUES 
('Laptop Pro X', 'Electronics', 1200.00, 50),
('Wireless Mouse', 'Electronics', 25.50, 200),
('Espresso Coffee Machine', 'Home', 85.00, 30),
('Gaming Monitor 27"', 'Electronics', 300.00, 15),
('Ergonomic Chair', 'Office', 150.00, 10);

-- Ordini simulati su Milano (per la tua query demo)
INSERT INTO Orders (OrderDate, CustomerRegion, TotalAmount) VALUES 
(DATE('now', '-10 days'), 'Milano', 1225.50),
(DATE('now', '-5 days'), 'Milano', 300.00),
(DATE('now', '-2 days'), 'Roma', 85.00);

INSERT INTO OrderItems (OrderId, ProductId, Quantity) VALUES 
(1, 1, 1), -- Laptop
(1, 2, 1), -- Mouse
(2, 4, 1); -- Monitor