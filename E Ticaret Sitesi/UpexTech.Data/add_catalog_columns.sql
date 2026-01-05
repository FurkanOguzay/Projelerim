-- Add missing catalog columns to Products table
-- Check and add SKU column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Products]') AND name = 'SKU')
BEGIN
    ALTER TABLE [dbo].[Products] ADD [SKU] NVARCHAR(100) NULL;
    PRINT 'Added SKU column';
END
ELSE
BEGIN
    PRINT 'SKU column already exists';
END
GO

-- Check and add Barcode column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Products]') AND name = 'Barcode')
BEGIN
    ALTER TABLE [dbo].[Products] ADD [Barcode] NVARCHAR(100) NULL;
    PRINT 'Added Barcode column';
END
ELSE
BEGIN
    PRINT 'Barcode column already exists';
END
GO

-- Check and add PurchasePrice column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Products]') AND name = 'PurchasePrice')
BEGIN
    ALTER TABLE [dbo].[Products] ADD [PurchasePrice] DECIMAL(18,2) NOT NULL DEFAULT 0;
    PRINT 'Added PurchasePrice column';
END
ELSE
BEGIN
    PRINT 'PurchasePrice column already exists';
END
GO

-- Check and add CriticalStockLevel column
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Products]') AND name = 'CriticalStockLevel')
BEGIN
    ALTER TABLE [dbo].[Products] ADD [CriticalStockLevel] INT NOT NULL DEFAULT 10;
    PRINT 'Added CriticalStockLevel column';
END
ELSE
BEGIN
    PRINT 'CriticalStockLevel column already exists';
END
GO

PRINT 'Migration completed successfully!';
