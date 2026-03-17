-- ERP System Database Schema
-- SQL Server Database

-- Create Schemas
CREATE SCHEMA [Inventory];
GO

CREATE SCHEMA [Orders];
GO

CREATE SCHEMA [Notifications];
GO

CREATE SCHEMA [Configuration];
GO

-- =============================================
-- Core Tables (dbo schema)
-- =============================================

-- Audit Logs Table
CREATE TABLE [dbo].[AuditLogs] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    [UserId] UNIQUEIDENTIFIER NOT NULL,
    [UserName] NVARCHAR(256) NOT NULL,
    [ActionType] NVARCHAR(50) NOT NULL,
    [EntityName] NVARCHAR(256) NOT NULL,
    [EntityId] NVARCHAR(128) NOT NULL,
    [OldValues] NVARCHAR(MAX) NULL,
    [NewValues] NVARCHAR(MAX) NULL,
    [Timestamp] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [IpAddress] NVARCHAR(50) NULL
);

CREATE INDEX [IX_AuditLogs_UserId] ON [dbo].[AuditLogs] ([UserId]);
CREATE INDEX [IX_AuditLogs_EntityName] ON [dbo].[AuditLogs] ([EntityName]);
CREATE INDEX [IX_AuditLogs_Timestamp] ON [dbo].[AuditLogs] ([Timestamp]);
GO

-- Outbox Messages Table (Transactional Outbox Pattern)
CREATE TABLE [dbo].[OutboxMessages] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    [EventType] NVARCHAR(256) NOT NULL,
    [Payload] NVARCHAR(MAX) NOT NULL,
    [OccurredOn] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [ProcessedOn] DATETIME2 NULL,
    [Status] NVARCHAR(20) NOT NULL DEFAULT 'Pending',
    [Error] NVARCHAR(2000) NULL,
    [RetryCount] INT NOT NULL DEFAULT 0
);

CREATE INDEX [IX_OutboxMessages_Status_OccurredOn] ON [dbo].[OutboxMessages] ([Status], [OccurredOn]);
GO

-- Processed Commands Table (Idempotency)
CREATE TABLE [dbo].[ProcessedCommands] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    [CommandId] UNIQUEIDENTIFIER NOT NULL,
    [UserId] UNIQUEIDENTIFIER NULL,
    [ProcessedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

CREATE UNIQUE INDEX [IX_ProcessedCommands_CommandId] ON [dbo].[ProcessedCommands] ([CommandId]);
GO

-- =============================================
-- Notifications Schema
-- =============================================

CREATE TABLE [Notifications].[Notifications] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    [UserId] UNIQUEIDENTIFIER NOT NULL,
    [Title] NVARCHAR(200) NOT NULL,
    [Message] NVARCHAR(2000) NOT NULL,
    [IsRead] BIT NOT NULL DEFAULT 0,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [ReadAt] DATETIME2 NULL
);

CREATE INDEX [IX_Notifications_UserId] ON [Notifications].[Notifications] ([UserId]);
CREATE INDEX [IX_Notifications_UserId_IsRead] ON [Notifications].[Notifications] ([UserId], [IsRead]);
GO

-- =============================================
-- Configuration Schema
-- =============================================

CREATE TABLE [Configuration].[SystemSettings] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    [Key] NVARCHAR(256) NOT NULL,
    [Value] NVARCHAR(MAX) NOT NULL,
    [Description] NVARCHAR(500) NULL,
    [Category] NVARCHAR(100) NULL,
    [IsEncrypted] BIT NOT NULL DEFAULT 0,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [LastModifiedAt] DATETIME2 NULL
);

CREATE UNIQUE INDEX [IX_SystemSettings_Key] ON [Configuration].[SystemSettings] ([Key]);
GO

CREATE TABLE [Configuration].[FeatureFlags] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    [Name] NVARCHAR(100) NOT NULL,
    [Description] NVARCHAR(500) NULL,
    [IsEnabled] BIT NOT NULL DEFAULT 0,
    [EnabledFrom] DATETIME2 NULL,
    [EnabledUntil] DATETIME2 NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [LastModifiedAt] DATETIME2 NULL
);

CREATE UNIQUE INDEX [IX_FeatureFlags_Name] ON [Configuration].[FeatureFlags] ([Name]);
GO

-- =============================================
-- Inventory Schema
-- =============================================

CREATE TABLE [Inventory].[Products] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    [Sku] NVARCHAR(50) NOT NULL,
    [Name] NVARCHAR(200) NOT NULL,
    [Description] NVARCHAR(2000) NULL,
    [Category_Code] NVARCHAR(20) NOT NULL,
    [Category_Name] NVARCHAR(100) NOT NULL,
    [UnitPrice_Amount] DECIMAL(18, 4) NOT NULL,
    [UnitPrice_Currency] NVARCHAR(3) NOT NULL,
    [StockQuantity_Value] INT NOT NULL DEFAULT 0,
    [ReorderLevel] INT NOT NULL DEFAULT 0,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [SupplierId] UNIQUEIDENTIFIER NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [CreatedBy] NVARCHAR(256) NULL,
    [LastModifiedAt] DATETIME2 NULL,
    [LastModifiedBy] NVARCHAR(256) NULL,
    [Version] INT NOT NULL DEFAULT 0
);

CREATE UNIQUE INDEX [IX_Products_Sku] ON [Inventory].[Products] ([Sku]);
CREATE INDEX [IX_Products_Category_Code] ON [Inventory].[Products] ([Category_Code]);
CREATE INDEX [IX_Products_IsActive] ON [Inventory].[Products] ([IsActive]);
GO

CREATE TABLE [Inventory].[StockMovements] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    [ProductId] UNIQUEIDENTIFIER NOT NULL,
    [MovementType] INT NOT NULL,
    [Quantity] INT NOT NULL,
    [PreviousQuantity] INT NOT NULL,
    [NewQuantity] INT NOT NULL,
    [Reason] NVARCHAR(500) NOT NULL,
    [ReferenceId] UNIQUEIDENTIFIER NULL,
    [OccurredAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT [FK_StockMovements_Products] FOREIGN KEY ([ProductId]) REFERENCES [Inventory].[Products]([Id]) ON DELETE CASCADE
);

CREATE INDEX [IX_StockMovements_ProductId] ON [Inventory].[StockMovements] ([ProductId]);
CREATE INDEX [IX_StockMovements_OccurredAt] ON [Inventory].[StockMovements] ([OccurredAt]);
GO

-- =============================================
-- Orders Schema
-- =============================================

CREATE TABLE [Orders].[Orders] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    [OrderNumber] NVARCHAR(50) NOT NULL,
    [CustomerId] UNIQUEIDENTIFIER NOT NULL,
    [Status_Id] INT NOT NULL,
    [Status_Name] NVARCHAR(50) NOT NULL,
    [ShippingAddress_Street] NVARCHAR(200) NOT NULL,
    [ShippingAddress_City] NVARCHAR(100) NOT NULL,
    [ShippingAddress_State] NVARCHAR(100) NULL,
    [ShippingAddress_Country] NVARCHAR(100) NOT NULL,
    [ShippingAddress_PostalCode] NVARCHAR(20) NOT NULL,
    [BillingAddress_Street] NVARCHAR(200) NULL,
    [BillingAddress_City] NVARCHAR(100) NULL,
    [BillingAddress_State] NVARCHAR(100) NULL,
    [BillingAddress_Country] NVARCHAR(100) NULL,
    [BillingAddress_PostalCode] NVARCHAR(20) NULL,
    [SubTotal_Amount] DECIMAL(18, 4) NOT NULL,
    [SubTotal_Currency] NVARCHAR(3) NOT NULL,
    [Tax_Amount] DECIMAL(18, 4) NOT NULL,
    [Tax_Currency] NVARCHAR(3) NOT NULL,
    [ShippingCost_Amount] DECIMAL(18, 4) NOT NULL,
    [ShippingCost_Currency] NVARCHAR(3) NOT NULL,
    [Total_Amount] DECIMAL(18, 4) NOT NULL,
    [Total_Currency] NVARCHAR(3) NOT NULL,
    [Notes] NVARCHAR(2000) NULL,
    [ShippedAt] DATETIME2 NULL,
    [DeliveredAt] DATETIME2 NULL,
    [CancelledAt] DATETIME2 NULL,
    [CancellationReason] NVARCHAR(500) NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [CreatedBy] NVARCHAR(256) NULL,
    [LastModifiedAt] DATETIME2 NULL,
    [LastModifiedBy] NVARCHAR(256) NULL,
    [Version] INT NOT NULL DEFAULT 0
);

CREATE UNIQUE INDEX [IX_Orders_OrderNumber] ON [Orders].[Orders] ([OrderNumber]);
CREATE INDEX [IX_Orders_CustomerId] ON [Orders].[Orders] ([CustomerId]);
CREATE INDEX [IX_Orders_Status_Id] ON [Orders].[Orders] ([Status_Id]);
CREATE INDEX [IX_Orders_CreatedAt] ON [Orders].[Orders] ([CreatedAt]);
GO

CREATE TABLE [Orders].[OrderItems] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    [OrderId] UNIQUEIDENTIFIER NOT NULL,
    [ProductId] UNIQUEIDENTIFIER NOT NULL,
    [ProductName] NVARCHAR(200) NOT NULL,
    [Sku] NVARCHAR(50) NOT NULL,
    [Quantity] INT NOT NULL,
    [UnitPrice_Amount] DECIMAL(18, 4) NOT NULL,
    [UnitPrice_Currency] NVARCHAR(3) NOT NULL,
    [LineTotal_Amount] DECIMAL(18, 4) NOT NULL,
    [LineTotal_Currency] NVARCHAR(3) NOT NULL,
    CONSTRAINT [FK_OrderItems_Orders] FOREIGN KEY ([OrderId]) REFERENCES [Orders].[Orders]([Id]) ON DELETE CASCADE
);

CREATE INDEX [IX_OrderItems_OrderId] ON [Orders].[OrderItems] ([OrderId]);
CREATE INDEX [IX_OrderItems_ProductId] ON [Orders].[OrderItems] ([ProductId]);
GO

-- =============================================
-- Hangfire Schema (Background Jobs)
-- =============================================

-- Hangfire will create its own tables when the application starts
-- Schema: [HangFire]

-- =============================================
-- Seed Data
-- =============================================

-- Insert default system settings
INSERT INTO [Configuration].[SystemSettings] ([Key], [Value], [Description], [Category])
VALUES
    ('App.Name', 'ERP System', 'Application name', 'General'),
    ('App.DefaultCurrency', 'USD', 'Default currency code', 'General'),
    ('App.DefaultTaxRate', '0.10', 'Default tax rate (10%)', 'Finance'),
    ('Inventory.LowStockAlertEnabled', 'true', 'Enable low stock alerts', 'Inventory'),
    ('Orders.AutoConfirmEnabled', 'false', 'Enable automatic order confirmation', 'Orders'),
    ('Notifications.EmailEnabled', 'true', 'Enable email notifications', 'Notifications');
GO

-- Insert default feature flags
INSERT INTO [Configuration].[FeatureFlags] ([Name], [Description], [IsEnabled])
VALUES
    ('RabbitMQ.Integration', 'Enable RabbitMQ for integration events', 0),
    ('Orders.ExpressCheckout', 'Enable express checkout feature', 0),
    ('Inventory.AutoReorder', 'Enable automatic inventory reordering', 0),
    ('Reports.AdvancedAnalytics', 'Enable advanced analytics in reports', 0);
GO

PRINT 'Database schema created successfully.';
GO
