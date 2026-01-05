-- Create Payments table for SQLite
CREATE TABLE IF NOT EXISTS Payments (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    UserId INTEGER NOT NULL,
    OrderId INTEGER,
    Amount REAL NOT NULL,
    PaymentMethod INTEGER NOT NULL,
    Status INTEGER NOT NULL DEFAULT 1,
    Channel INTEGER NOT NULL,
    PaymentDate TEXT NOT NULL,
    ReferenceNumber TEXT,
    Description TEXT,
    BankName TEXT,
    AccountName TEXT,
    InstallmentCount INTEGER,
    IsIncoming INTEGER NOT NULL DEFAULT 1,
    IsActive INTEGER NOT NULL DEFAULT 1,
    CreatedAt TEXT NOT NULL DEFAULT (datetime('now')),
    UpdatedAt TEXT,
    FOREIGN KEY (UserId) REFERENCES Users(Id),
    FOREIGN KEY (OrderId) REFERENCES Orders(Id)
);

-- Create indexes
CREATE INDEX IF NOT EXISTS IX_Payments_UserId ON Payments(UserId);
CREATE INDEX IF NOT EXISTS IX_Payments_OrderId ON Payments(OrderId);
CREATE INDEX IF NOT EXISTS IX_Payments_PaymentDate ON Payments(PaymentDate);

-- Insert sample payment data
INSERT OR IGNORE INTO Payments (UserId, OrderId, Amount, PaymentMethod, Status, Channel, PaymentDate, ReferenceNumber, Description, BankName, AccountName, InstallmentCount, IsIncoming, IsActive, CreatedAt)
SELECT 
    1, NULL, 2850.00, 1, 2, 1, datetime('now', '-1 day'), 'TS-9982', 'Sipariş #TS-9982 ödemesi', NULL, 'Iyzico POS', 3, 1, 1, datetime('now')
WHERE NOT EXISTS (SELECT 1 FROM Payments WHERE ReferenceNumber = 'TS-9982');

INSERT OR IGNORE INTO Payments (UserId, OrderId, Amount, PaymentMethod, Status, Channel, PaymentDate, ReferenceNumber, Description, BankName, AccountName, InstallmentCount, IsIncoming, IsActive, CreatedAt)
SELECT 
    1, NULL, 45000.00, 2, 2, 2, datetime('now', '-1 day'), 'TS-9978', 'Sipariş #TS-9978 ödemesi', 'Garanti BBVA', 'Banka Havalesi', NULL, 1, 1, datetime('now')
WHERE NOT EXISTS (SELECT 1 FROM Payments WHERE ReferenceNumber = 'TS-9978');
