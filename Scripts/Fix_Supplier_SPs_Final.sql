-- Fix Supplier Report Stored Procedures - Final Version
-- This script fixes the missing END statement in BfSupplierStatement2

-- Drop and recreate BfSupplierStatement2 with proper END statement
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BfSupplierStatement2]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[BfSupplierStatement2]
GO

CREATE PROCEDURE [dbo].[BfSupplierStatement2]
    @begindt DATETIME,
    @enddt DATETIME,
    @branch VARCHAR(50),
    @client DECIMAL(18,0),
    @Category VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Clear temp table
    TRUNCATE TABLE rSupplierStatement;
    
    -- Insert opening balance
    INSERT INTO rSupplierStatement (Transdate, InvoiceNo, Branch, InvoiceType, DebitAmount, CreditAmount, Supplier)
    SELECT @begindt, 'Balance b/f', @branch, '', 0, 0, @client;
    
    -- Insert transactions
    INSERT INTO rSupplierStatement (Transdate, InvoiceNo, Branch, InvoiceType, DebitAmount, CreditAmount, Supplier)
    SELECT 
        TransDate,
        InvoiceNo,
        Branch,
        InvoiceType,
        SUM(DebitAmount) as DebitAmount,
        SUM(CreditAmount) as CreditAmount,
        Supplier
    FROM SupplierStatement
    WHERE Supplier = @client 
        AND TransDate < @begindt 
        AND ItemCategory = @Category
    GROUP BY TransDate, InvoiceNo, Branch, InvoiceType, Supplier
    
    UNION ALL
    
    SELECT 
        TransDate,
        InvoiceNo,
        Branch,
        InvoiceType,
        SUM(DebitAmount) as DebitAmount,
        SUM(CreditAmount) as CreditAmount,
        Supplier
    FROM SupplierStatement
    WHERE Supplier = @client 
        AND TransDate >= @begindt 
        AND TransDate <= @enddt 
        AND ItemCategory = @Category
    GROUP BY TransDate, InvoiceNo, Branch, InvoiceType, Supplier
    ORDER BY TransDate;
END
GO
