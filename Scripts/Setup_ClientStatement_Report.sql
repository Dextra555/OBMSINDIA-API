-- Setup Script for Client Statement Report
-- This script creates/updates the GetClientStatement stored procedure
-- Run this script on your main database to fix the empty data issue
-- Note: The rClientStatement table already exists with the correct structure

-- ============================================
-- Step 2: Create/Update GetClientStatement Stored Procedure
-- ============================================
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'GetClientStatement' AND type = 'P')
DROP PROCEDURE [dbo].[GetClientStatement]
GO

CREATE PROCEDURE [dbo].[GetClientStatement]
    @begindt DATETIME,
    @enddt DATETIME,
    @branch VARCHAR(50),
    @client VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    PRINT '=== GetClientStatement Starting ===';
    PRINT '  Branch: ' + ISNULL(@branch, 'NULL');
    PRINT '  Client: ' + ISNULL(@client, 'NULL');
    PRINT '  Start Date: ' + CONVERT(VARCHAR, @begindt, 120);
    PRINT '  End Date: ' + CONVERT(VARCHAR, @enddt, 120);
    
    -- Clear existing data for the specified branch and client
    DELETE FROM rClientStatement 
    WHERE Branch = @branch
        AND ClientCode = @client;
    
    -- Count available data before processing
    DECLARE @availableInvoices INT = 0;
    DECLARE @availableReceipts INT = 0;
    DECLARE @availableCreditNotes INT = 0;
    
    SELECT @availableInvoices = COUNT(*) FROM InvoiceDetails 
    WHERE LTRIM(RTRIM(Branch)) = LTRIM(RTRIM(@branch)) 
        AND LTRIM(RTRIM(Client)) = LTRIM(RTRIM(@client))
        AND InvoiceDate BETWEEN @begindt AND @enddt 
        AND (IsDeleted = 'N' OR IsDeleted = '0' OR IsDeleted IS NULL);
        
    SELECT @availableReceipts = COUNT(*) FROM Receipts 
    WHERE LTRIM(RTRIM(Branch)) = LTRIM(RTRIM(@branch))
        AND LTRIM(RTRIM(PaymentFrom)) = LTRIM(RTRIM(@client))
        AND ReceiptDate BETWEEN @begindt AND @enddt 
        AND (IsDeleted = 0 OR IsDeleted IS NULL);
        
    SELECT @availableCreditNotes = COUNT(*) FROM Receipts 
    WHERE LTRIM(RTRIM(Branch)) = LTRIM(RTRIM(@branch))
        AND LTRIM(RTRIM(PaymentFrom)) = LTRIM(RTRIM(@client))
        AND ReceiptDate BETWEEN @begindt AND @enddt 
        AND (IsDeleted = 0 OR IsDeleted IS NULL)
        AND CreditNoteAmount > 0;
    
    PRINT 'Available data - Invoices: ' + CAST(@availableInvoices AS VARCHAR) + 
           ', Receipts: ' + CAST(@availableReceipts AS VARCHAR) + 
           ', Credit Notes: ' + CAST(@availableCreditNotes AS VARCHAR);
    
    -- Insert client statement data into rClientStatement table
    -- Invoices
    INSERT INTO rClientStatement (Branch, ClientCode, ClientName, InvoiceNo, InvoiceDate, TransDate, DueDate, Particulars,
                                   InvoiceAmount, TaxAmount, TotalAmount, DebitAmount, CreditAmount,
                                   BalanceAmount, TransAmount, ReportDate, PeriodStartDate, PeriodEndDate)
    SELECT
        @branch as Branch,
        @client as ClientCode,
        ISNULL(cm.Name, '') as ClientName,
        id.InvoiceNo,
        CAST(id.InvoiceDate AS DATE) as InvoiceDate,
        CAST(id.InvoiceDate AS DATE) as TransDate,
        DATEADD(DAY, 30, CAST(id.InvoiceDate AS DATE)) as DueDate,
        'Invoice No: ' + LTRIM(RTRIM(id.InvoiceNo)) + ' - Service Charges' as Particulars,
        id.InvoiceAmount as InvoiceAmount,
        id.TaxAmount as TaxAmount,
        id.InvoiceAmount as TotalAmount,
        id.InvoiceAmount as DebitAmount,
        0 as CreditAmount,
        id.InvoiceAmount as BalanceAmount,
        id.InvoiceAmount as TransAmount,
        CAST(@begindt AS DATE) as ReportDate,
        CAST(@begindt AS DATE) as PeriodStartDate,
        CAST(@enddt AS DATE) as PeriodEndDate
    FROM InvoiceDetails id
    LEFT JOIN ClientMaster cm ON cm.Code = id.Client AND cm.Branch = id.Branch
    WHERE LTRIM(RTRIM(id.Branch)) = LTRIM(RTRIM(@branch))
        AND LTRIM(RTRIM(id.Client)) = LTRIM(RTRIM(@client))
        AND id.InvoiceDate BETWEEN @begindt AND @enddt
        AND (id.IsDeleted = 'N' OR id.IsDeleted = '0' OR id.IsDeleted IS NULL);
    
    -- Receipts
    INSERT INTO rClientStatement (Branch, ClientCode, ClientName, InvoiceNo, InvoiceDate, TransDate, DueDate, Particulars,
                                   InvoiceAmount, TaxAmount, TotalAmount, DebitAmount, CreditAmount,
                                   BalanceAmount, TransAmount, ReportDate, PeriodStartDate, PeriodEndDate, PaymentDate)
    SELECT
        @branch as Branch,
        @client as ClientCode,
        ISNULL(cm.Name, '') as ClientName,
        CAST(r.VoucherNo AS VARCHAR) + '-' + CAST(r.ID AS VARCHAR) as InvoiceNo,
        CAST(r.ReceiptDate AS DATE) as InvoiceDate,
        CAST(r.ReceiptDate AS DATE) as TransDate,
        CAST(r.ReceiptDate AS DATE) as DueDate,
        'Receipt - ' + ISNULL(LTRIM(RTRIM(r.Particulars)), 'Payment Received') as Particulars,
        0 as InvoiceAmount,
        0 as TaxAmount,
        r.ReceiptAmount as TotalAmount,
        0 as DebitAmount,
        r.ReceiptAmount as CreditAmount,
        -r.ReceiptAmount as BalanceAmount,
        r.ReceiptAmount as TransAmount,
        CAST(@begindt AS DATE) as ReportDate,
        CAST(@begindt AS DATE) as PeriodStartDate,
        CAST(@enddt AS DATE) as PeriodEndDate,
        CAST(r.ReceiptDate AS DATE) as PaymentDate
    FROM Receipts r
    LEFT JOIN ClientMaster cm ON cm.Code = r.PaymentFrom AND cm.Branch = r.Branch
    WHERE LTRIM(RTRIM(r.Branch)) = LTRIM(RTRIM(@branch))
        AND LTRIM(RTRIM(r.PaymentFrom)) = LTRIM(RTRIM(@client))
        AND r.ReceiptDate BETWEEN @begindt AND @enddt
        AND (r.IsDeleted = 0 OR r.IsDeleted IS NULL);
    
    -- Credit Notes
    INSERT INTO rClientStatement (Branch, ClientCode, ClientName, InvoiceNo, InvoiceDate, TransDate, DueDate, Particulars,
                                   InvoiceAmount, TaxAmount, TotalAmount, DebitAmount, CreditAmount,
                                   BalanceAmount, TransAmount, ReportDate, PeriodStartDate, PeriodEndDate, PaymentDate)
    SELECT
        @branch as Branch,
        @client as ClientCode,
        ISNULL(cm.Name, '') as ClientName,
        'CN-' + CAST(r.VoucherNo AS VARCHAR) + '-' + CAST(r.ID AS VARCHAR) as InvoiceNo,
        CAST(r.ReceiptDate AS DATE) as InvoiceDate,
        CAST(r.ReceiptDate AS DATE) as TransDate,
        CAST(r.ReceiptDate AS DATE) as DueDate,
        'Credit Note - ' + ISNULL(LTRIM(RTRIM(r.Particulars)), 'Credit Adjustment') as Particulars,
        0 as InvoiceAmount,
        0 as TaxAmount,
        r.CreditNoteAmount as TotalAmount,
        0 as DebitAmount,
        r.CreditNoteAmount as CreditAmount,
        -r.CreditNoteAmount as BalanceAmount,
        r.CreditNoteAmount as TransAmount,
        CAST(@begindt AS DATE) as ReportDate,
        CAST(@begindt AS DATE) as PeriodStartDate,
        CAST(@enddt AS DATE) as PeriodEndDate,
        CAST(r.ReceiptDate AS DATE) as PaymentDate
    FROM Receipts r
    LEFT JOIN ClientMaster cm ON cm.Code = r.PaymentFrom AND cm.Branch = r.Branch
    WHERE LTRIM(RTRIM(r.Branch)) = LTRIM(RTRIM(@branch))
        AND LTRIM(RTRIM(r.PaymentFrom)) = LTRIM(RTRIM(@client))
        AND r.ReceiptDate BETWEEN @begindt AND @enddt
        AND (r.IsDeleted = 0 OR r.IsDeleted IS NULL)
        AND r.CreditNoteAmount > 0;
    
    -- Return the inserted data
    SELECT 
        Branch,
        ClientCode,
        ClientName,
        InvoiceNo,
        InvoiceDate,
        TransDate,
        Particulars,
        InvoiceAmount,
        TaxAmount,
        TotalAmount,
        AmountPaid,
        BalanceAmount,
        DebitAmount,
        CreditAmount,
        TransAmount,
        Status,
        PaymentDate
    FROM rClientStatement
    WHERE Branch = @branch
        AND ClientCode = @client
        AND TransDate BETWEEN CAST(@begindt AS DATE) AND CAST(@enddt AS DATE)
    ORDER BY TransDate, InvoiceNo;
    
    -- Report results
    DECLARE @insertedCount INT = 0;
    SELECT @insertedCount = COUNT(*) FROM rClientStatement WHERE Branch = @branch AND ClientCode = @client AND TransDate BETWEEN CAST(@begindt AS DATE) AND CAST(@enddt AS DATE);
    
    PRINT '=== RESULTS ===';
    PRINT 'Inserted ' + CAST(@insertedCount AS VARCHAR) + ' rows into rClientStatement table.';
    PRINT 'GetClientStatement completed successfully.';
END
GO

PRINT '===========================================';
PRINT 'Client Statement Report Setup Complete';
PRINT '===========================================';
PRINT 'GetClientStatement stored procedure: Created/Updated';
PRINT 'You can now test the Client Statement Report.';
