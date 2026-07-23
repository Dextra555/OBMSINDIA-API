-- Create GetClientStatement Stored Procedure

-- This stored procedure generates client statement data for Crystal Reports



IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetClientStatement]') AND type in (N'P', N'PC'))

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

    

    -- Generate client statement data

    SELECT 

        TransactionDate,

        TransactionType,

        ReferenceNo,

        Description,

        DebitAmount,

        CreditAmount,

        Balance

    FROM (

        -- Invoices (Debit)

        SELECT 

            InvoiceDate as TransactionDate,

            'INVOICE' as TransactionType,

            InvoiceNo as ReferenceNo,

            'Invoice No: ' + InvoiceNo + ' - Service Charges' as Description,

            InvoiceAmount as DebitAmount,

            0 as CreditAmount,

            0 as Balance

        FROM InvoiceDetails

        WHERE Branch = @branch 

            AND Client = @client 

            AND InvoiceDate BETWEEN @begindt AND @enddt 

            AND IsDeleted = 0

        

        UNION ALL

        

        -- Receipts (Credit)

        SELECT 

            ReceiptDate as TransactionDate,

            'RECEIPT' as TransactionType,

            VoucherNo as ReferenceNo,

            'Receipt - ' + ISNULL(Particulars, 'Payment Received') as Description,

            0 as DebitAmount,

            ReceiptAmount as CreditAmount,

            0 as Balance

        FROM Receipts

        WHERE Branch = @branch 

            AND PaymentFrom = @client 

            AND ReceiptDate BETWEEN @begindt AND @enddt 

            AND IsDeleted = 0

        

        UNION ALL

        

        -- Credit Notes (Credit)

        SELECT 

            ReceiptDate as TransactionDate,

            'CREDIT NOTE' as TransactionType,

            VoucherNo as ReferenceNo,

            'Credit Note - ' + ISNULL(Particulars, 'Credit Adjustment') as Description,

            0 as DebitAmount,

            CreditNoteAmount as CreditAmount,

            0 as Balance

        FROM Receipts

        WHERE Branch = @branch 

            AND PaymentFrom = @client 

            AND ReceiptDate BETWEEN @begindt AND @enddt 

            AND IsDeleted = 0 

            AND CreditNoteAmount > 0

    ) AS Transactions

    ORDER BY TransactionDate, TransactionType;

END

GO

