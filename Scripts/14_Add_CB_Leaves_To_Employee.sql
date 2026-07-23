USE [obms];
GO

-- Add CB_Leaves and CB_LeavesPercentage columns to Employee table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Employee]') AND name = 'CB_Leaves')
BEGIN
    ALTER TABLE [dbo].[Employee] ADD CB_Leaves DECIMAL(18,2) NULL DEFAULT 0;
    PRINT 'Column CB_Leaves added to Employee table';
END
ELSE
BEGIN
    PRINT 'Column CB_Leaves already exists in Employee table';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Employee]') AND name = 'CB_LeavesPercentage')
BEGIN
    ALTER TABLE [dbo].[Employee] ADD CB_LeavesPercentage DECIMAL(18,2) NULL DEFAULT 0;
    PRINT 'Column CB_LeavesPercentage added to Employee table';
END
ELSE
BEGIN
    PRINT 'Column CB_LeavesPercentage already exists in Employee table';
END

GO
