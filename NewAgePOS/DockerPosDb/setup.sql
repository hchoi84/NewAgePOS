CREATE DATABASE posdb;
GO

USE posdb;
GO

CREATE TABLE [dbo].[Customers](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[FirstName] [varchar](50) NOT NULL,
	[LastName] [varchar](50) NOT NULL,
	[EmailAddress] [varchar](100) NOT NULL,
	[PhoneNumber] [varchar](10) NOT NULL,
	[Created] [datetime2](7) NOT NULL,
	[Updated] [datetime2](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[GiftCards]    Script Date: 9/16/2020 10:04:17 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[GiftCards](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Code] [varchar](20) NOT NULL,
	[Amount] [money] NOT NULL,
	[Created] [datetime2](7) NOT NULL,
	[Updated] [datetime2](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Messages]    Script Date: 9/16/2020 10:04:17 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Messages](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[SaleId] [int] NOT NULL,
	[Message] [varchar](100) NOT NULL,
	[Created] [datetime2](7) NOT NULL,
	[Updated] [datetime2](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Products]    Script Date: 9/16/2020 10:04:17 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Products](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Sku] [varchar](15) NOT NULL,
	[Upc] [varchar](20) NOT NULL,
	[Cost] [money] NOT NULL,
	[Price] [money] NOT NULL,
	[AllName] [varchar](150) NOT NULL,
	[Created] [datetime2](7) NOT NULL,
	[Updated] [datetime2](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RefundLines]    Script Date: 9/16/2020 10:04:17 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RefundLines](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[SaleLineId] [int] NOT NULL,
	[TransactionId] [int] NULL,
	[Qty] [int] NOT NULL,
	[Created] [datetime2](7) NOT NULL,
	[Updated] [datetime2](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[SaleLines]    Script Date: 9/16/2020 10:04:17 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SaleLines](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[SaleId] [int] NOT NULL,
	[ProductId] [int] NULL,
	[GiftCardId] [int] NULL,
	[Cost] [money] NOT NULL,
	[Price] [money] NOT NULL,
	[Qty] [int] NOT NULL,
	[DiscPct] [float] NOT NULL,
	[Created] [datetime2](7) NOT NULL,
	[Updated] [datetime2](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Sales]    Script Date: 9/16/2020 10:04:17 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Sales](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CustomerId] [int] NOT NULL,
	[TaxId] [int] NOT NULL,
	[IsComplete] [int] NOT NULL,
	[Created] [datetime2](7) NOT NULL,
	[Updated] [datetime2](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Taxes]    Script Date: 9/16/2020 10:04:17 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Taxes](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[TaxPct] [float] NOT NULL,
	[IsDefault] [int] NOT NULL,
	[Created] [datetime2](7) NOT NULL,
	[Updated] [datetime2](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Transactions]    Script Date: 9/16/2020 10:04:17 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Transactions](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[SaleId] [int] NOT NULL,
	[GiftCardId] [int] NULL,
	[Amount] [money] NOT NULL,
	[Method] [varchar](15) NOT NULL,
	[Type] [varchar](15) NOT NULL,
	[Created] [datetime2](7) NOT NULL,
	[Updated] [datetime2](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Customers] ADD  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[Customers] ADD  DEFAULT (getdate()) FOR [Updated]
GO
ALTER TABLE [dbo].[GiftCards] ADD  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[GiftCards] ADD  DEFAULT (getdate()) FOR [Updated]
GO
ALTER TABLE [dbo].[Messages] ADD  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[Messages] ADD  DEFAULT (getdate()) FOR [Updated]
GO
ALTER TABLE [dbo].[Products] ADD  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[Products] ADD  DEFAULT (getdate()) FOR [Updated]
GO
ALTER TABLE [dbo].[RefundLines] ADD  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[RefundLines] ADD  DEFAULT (getdate()) FOR [Updated]
GO
ALTER TABLE [dbo].[SaleLines] ADD  DEFAULT ((0)) FOR [DiscPct]
GO
ALTER TABLE [dbo].[SaleLines] ADD  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[SaleLines] ADD  DEFAULT (getdate()) FOR [Updated]
GO
ALTER TABLE [dbo].[Sales] ADD  DEFAULT ((0)) FOR [IsComplete]
GO
ALTER TABLE [dbo].[Sales] ADD  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[Sales] ADD  DEFAULT (getdate()) FOR [Updated]
GO
ALTER TABLE [dbo].[Taxes] ADD  DEFAULT ((0)) FOR [IsDefault]
GO
ALTER TABLE [dbo].[Taxes] ADD  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[Taxes] ADD  DEFAULT (getdate()) FOR [Updated]
GO
ALTER TABLE [dbo].[Transactions] ADD  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[Transactions] ADD  DEFAULT (getdate()) FOR [Updated]
GO
ALTER TABLE [dbo].[Messages]  WITH CHECK ADD  CONSTRAINT [FK_Messages_Sales] FOREIGN KEY([SaleId])
REFERENCES [dbo].[Sales] ([Id])
GO
ALTER TABLE [dbo].[Messages] CHECK CONSTRAINT [FK_Messages_Sales]
GO
ALTER TABLE [dbo].[RefundLines]  WITH CHECK ADD  CONSTRAINT [FK_RefundLines_SaleLines] FOREIGN KEY([SaleLineId])
REFERENCES [dbo].[SaleLines] ([Id])
GO
ALTER TABLE [dbo].[RefundLines] CHECK CONSTRAINT [FK_RefundLines_SaleLines]
GO
ALTER TABLE [dbo].[RefundLines]  WITH CHECK ADD  CONSTRAINT [FK_RefundLines_Transactions] FOREIGN KEY([TransactionId])
REFERENCES [dbo].[Transactions] ([Id])
GO
ALTER TABLE [dbo].[RefundLines] CHECK CONSTRAINT [FK_RefundLines_Transactions]
GO
ALTER TABLE [dbo].[SaleLines]  WITH CHECK ADD  CONSTRAINT [FK_SaleLines_GiftCards] FOREIGN KEY([GiftCardId])
REFERENCES [dbo].[GiftCards] ([Id])
GO
ALTER TABLE [dbo].[SaleLines] CHECK CONSTRAINT [FK_SaleLines_GiftCards]
GO
ALTER TABLE [dbo].[SaleLines]  WITH CHECK ADD  CONSTRAINT [FK_SaleLines_Products] FOREIGN KEY([ProductId])
REFERENCES [dbo].[Products] ([Id])
GO
ALTER TABLE [dbo].[SaleLines] CHECK CONSTRAINT [FK_SaleLines_Products]
GO
ALTER TABLE [dbo].[SaleLines]  WITH CHECK ADD  CONSTRAINT [FK_SaleLines_Sales] FOREIGN KEY([SaleId])
REFERENCES [dbo].[Sales] ([Id])
GO
ALTER TABLE [dbo].[SaleLines] CHECK CONSTRAINT [FK_SaleLines_Sales]
GO
ALTER TABLE [dbo].[Sales]  WITH CHECK ADD  CONSTRAINT [FK_Sales_Customers] FOREIGN KEY([CustomerId])
REFERENCES [dbo].[Customers] ([Id])
GO
ALTER TABLE [dbo].[Sales] CHECK CONSTRAINT [FK_Sales_Customers]
GO
ALTER TABLE [dbo].[Sales]  WITH CHECK ADD  CONSTRAINT [FK_Sales_Taxes] FOREIGN KEY([TaxId])
REFERENCES [dbo].[Taxes] ([Id])
GO
ALTER TABLE [dbo].[Sales] CHECK CONSTRAINT [FK_Sales_Taxes]
GO
ALTER TABLE [dbo].[Transactions]  WITH CHECK ADD  CONSTRAINT [FK_Transactions_GiftCards] FOREIGN KEY([GiftCardId])
REFERENCES [dbo].[GiftCards] ([Id])
GO
ALTER TABLE [dbo].[Transactions] CHECK CONSTRAINT [FK_Transactions_GiftCards]
GO
ALTER TABLE [dbo].[Transactions]  WITH CHECK ADD  CONSTRAINT [FK_Transactions_Sales] FOREIGN KEY([SaleId])
REFERENCES [dbo].[Sales] ([Id])
GO
ALTER TABLE [dbo].[Transactions] CHECK CONSTRAINT [FK_Transactions_Sales]
GO
/****** Object:  StoredProcedure [dbo].[spCustomers_GetBySaleId]    Script Date: 9/16/2020 10:04:17 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[spCustomers_GetBySaleId]
	@saleId int
AS
BEGIN
 SET NOCOUNT ON;

 SELECT *
 FROM dbo.Customers
 WHERE Id = 
	(SELECT CustomerId
	FROM dbo.Sales
	WHERE Id = @saleId);
END
GO
/****** Object:  StoredProcedure [dbo].[spCustomers_GetByTransactionId]    Script Date: 9/16/2020 10:04:17 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[spCustomers_GetByTransactionId]
	@transactionId int
AS
BEGIN
	SET NOCOUNT ON;

	SELECT c.*
	FROM dbo.Transactions t
	INNER JOIN dbo.Sales s ON s.Id = t.SaleId
	INNER JOIN dbo.Customers c ON c.Id = s.CustomerId
	WHERE t.Id = @transactionId;
END
GO
/****** Object:  StoredProcedure [dbo].[spGiftCards_GetBySaleId]    Script Date: 9/16/2020 10:04:17 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[spGiftCards_GetBySaleId]
	@saleId int
AS
BEGIN
	SET NOCOUNT ON;

	SELECT gc.*
	FROM dbo.SaleLines s
	INNER JOIN dbo.GiftCards gc ON s.GiftCardId = gc.Id
	WHERE s.GiftCardId IS NOT NULL AND s.SaleId = @saleId;
END
GO
/****** Object:  StoredProcedure [dbo].[spGiftCards_Insert]    Script Date: 9/16/2020 10:04:17 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[spGiftCards_Insert]
	@code VARCHAR(20),
	@amount MONEY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO dbo.GiftCards (Code, Amount)
	VALUES (@code, @amount);

	SELECT @@IDENTITY;
END
GO
/****** Object:  StoredProcedure [dbo].[spProducts_GetBySaleId]    Script Date: 9/16/2020 10:04:17 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[spProducts_GetBySaleId]
	@saleId int
AS
BEGIN
	SET NOCOUNT ON;

	SELECT p.*
	FROM dbo.SaleLines s
	INNER JOIN dbo.Products p ON s.ProductId = p.Id
	WHERE s.ProductId IS NOT NULL AND s.SaleId = @saleId;
END
GO
/****** Object:  StoredProcedure [dbo].[spRefundLines_GetBySaleId]    Script Date: 9/16/2020 10:04:17 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[spRefundLines_GetBySaleId]
	@saleId int
AS
BEGIN
	SET NOCOUNT ON;

	SELECT rl.*
	FROM dbo.SaleLines sl
	INNER JOIN dbo.RefundLines rl ON sl.Id = rl.SaleLineId
	WHERE SaleId = @saleId;
END
GO
/****** Object:  StoredProcedure [dbo].[spRefundLines_GetRefundQtyBySaleLineId]    Script Date: 9/16/2020 10:04:17 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[spRefundLines_GetRefundQtyBySaleLineId]
	@saleLineId int
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @refundedQty int;

	SELECT @refundedQty = SUM(Qty) FROM dbo.RefundLines WHERE SaleLineId = @saleLineId;

	IF (@refundedQty IS NULL)
		SELECT 0;
	ELSE
		SELECT @refundedQty;
END
GO
/****** Object:  StoredProcedure [dbo].[spSaleLines_Insert]    Script Date: 9/16/2020 10:04:17 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[spSaleLines_Insert]
	@saleId int, 
	@productId int, 
	@giftCardId int,
	@qty int
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @price money;

	IF (@giftCardId IS NULL)
		BEGIN
			DECLARE @cost money;

			SELECT @cost = Cost, @price = Price
			FROM dbo.Products
			WHERE Id = @productId;

			INSERT INTO dbo.SaleLines (SaleId, ProductId, Qty, DiscPct, Cost, Price)
			VALUES (@saleId, @productId, @qty, 15, @cost, @price);
		END
	ELSE
		BEGIN
			SELECT @price = Amount
			FROM dbo.GiftCards
			WHERE Id = @giftCardId;

			INSERT INTO dbo.SaleLines (SaleId, GiftCardId, Qty, DiscPct, Cost, Price)
			VALUES (@saleId, @giftCardId, @qty, 0, 0, @price);
		END
END
GO
/****** Object:  StoredProcedure [dbo].[spSales_CancelById]    Script Date: 9/16/2020 10:04:17 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[spSales_CancelById]
	@id int
AS
BEGIN
	SET NOCOUNT ON;

	DELETE FROM dbo.SaleLines WHERE SaleId = @id;

	DELETE FROM dbo.Sales WHERE Id = @id;

END
GO
/****** Object:  StoredProcedure [dbo].[spSales_Insert]    Script Date: 9/16/2020 10:04:17 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[spSales_Insert]
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @CustomerId int;
	DECLARE @TaxId int;

	SELECT @CustomerId = Id FROM dbo.Customers WHERE EmailAddress = 'guest@email.com';

	SELECT @TaxId = Id FROM dbo.Taxes WHERE IsDefault = 1

	INSERT INTO dbo.Sales (CustomerId, TaxId)
	OUTPUT inserted.Id
	VALUES (@customerId, @TaxId);
END
GO
/****** Object:  StoredProcedure [dbo].[spSales_UpdateCustomerIdToGuest]    Script Date: 9/16/2020 10:04:17 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[spSales_UpdateCustomerIdToGuest]
	@saleId int
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @customerId int;

	SELECT @customerId = Id
	FROM dbo.Customers
	WHERE EmailAddress = 'guest@email.com';

	UPDATE dbo.Sales
	SET CustomerId = @customerId;
END
GO
/****** Object:  StoredProcedure [dbo].[spTaxes_GetBySaleId]    Script Date: 9/16/2020 10:04:17 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[spTaxes_GetBySaleId]
	@saleId int
AS
BEGIN
	SET NOCOUNT ON;

	SELECT TaxPct
	FROM dbo.Taxes
	WHERE Id = 
		(SELECT TaxId
		FROM dbo.Sales
		WHERE Id = @saleId)
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Customers)
BEGIN
  INSERT INTO dbo.Customers(FirstName, LastName,  EmailAddress,         PhoneNumber)
                     VALUES ('guest', 'guest', 'guest@email.com', '5555555555');
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Taxes)
BEGIN
  INSERT INTO dbo.Taxes(TaxPct, IsDefault) VALUES (0.00, 1);
END
GO