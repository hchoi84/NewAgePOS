CREATE DATABASE posdb;
GO

USE posdb;
GO

CREATE TABLE [dbo].[Products]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY,
	[Sku] VARCHAR(15) NOT NULL,
	[Upc] VARCHAR(20) NOT NULL,
	[Cost] MONEY NOT NULL,
	[Price] MONEY NOT NULL,
	[AllName] VARCHAR(150) NOT NULL,
	[Source] VARCHAR(10) NOT NULL DEFAULT('API'),
	[Created] DATE NOT NULL DEFAULT getdate(),
	[Updated] DATE NOT NULL DEFAULT getdate()
)
GO

CREATE TABLE [dbo].[Customers]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY,
	[FirstName] VARCHAR(50) NOT NULL,
	[LastName] VARCHAR(50) NOT NULL,
	[EmailAddress] VARCHAR(100) NOT NULL,
	[PhoneNumber] VARCHAR(10) NOT NULL,
	[Created] DATE NOT NULL DEFAULT getdate(),
	[Updated] DATE NOT NULL DEFAULT getdate()
)
GO

CREATE TABLE [dbo].[Taxes]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY,
	[TaxPct] FLOAT NOT NULL,
	[IsDefault] INT NOT NULL DEFAULT 0,
	[Created] DATE NOT NULL DEFAULT getdate(),
	[Updated] DATE NOT NULL DEFAULT getdate()
)
GO

CREATE TABLE [dbo].[Sales]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY,
	[CustomerId] INT NOT NULL,
	[TaxId] INT NOT NULL,
	[IsComplete] INT NOT NULL DEFAULT(0),
	[Message] VARCHAR(200),
	[Created] DATE NOT NULL DEFAULT getdate(),
	[Updated] DATE NOT NULL DEFAULT getdate(),
	CONSTRAINT [FK_Sales_Customers] FOREIGN KEY (CustomerId) REFERENCES Customers(Id),
	CONSTRAINT [FK_Sales_Taxes] FOREIGN KEY (TaxId) REFERENCES Taxes(Id)
)
GO

CREATE TABLE [dbo].[SaleLines]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY,
	[SaleId] INT NOT NULL,
	[ProductId] INT NOT NULL,
	[Cost] MONEY NOT NULL,
	[Price] MONEY NOT NULL,
	[Qty] INT NOT NULL,
	[DiscPct] FLOAT NOT NULL DEFAULT 0,
	[Created] DATE NOT NULL DEFAULT getdate(),
	[Updated] DATE NOT NULL DEFAULT getdate(),
	CONSTRAINT [FK_SaleLines_Sales] FOREIGN KEY (SaleId) REFERENCES Sales(Id),
	CONSTRAINT [FK_SaleLines_Products] FOREIGN KEY (ProductId) REFERENCES Products(Id)
)
GO

CREATE TABLE [dbo].[Transactions]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY,
	[SaleId] INT NOT NULL,
	[Amount] MONEY NOT NULL,
	[Method] VARCHAR(15) NOT NULL,
	[Type] VARCHAR(15) NOT NULL,
	[Message] VARCHAR(200),
	[Created] DATE NOT NULL DEFAULT getdate(),
	[Updated] DATE NOT NULL DEFAULT getdate(),
	CONSTRAINT [FK_Transactions_Sales] FOREIGN KEY (SaleId) REFERENCES Sales(Id)
)
GO

CREATE TABLE [dbo].[RefundLines]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY,
	[SaleLineId] INT NOT NULL,
	[TransactionId] INT NOT NULL,
	[Qty] INT NOT NULL,
	[Created] DATE NOT NULL DEFAULT(getdate()),
	[Updated] DATE NOT NULL DEFAULT(getdate()),
	CONSTRAINT [FK_RefundLines_SaleLines] FOREIGN KEY (SaleLineId) REFERENCES SaleLines(Id),
	CONSTRAINT [FK_RefundLines_Transactions] FOREIGN KEY (TransactionId) REFERENCES Transactions(Id)
)
GO

CREATE PROCEDURE [dbo].[spCustomers_GetBySaleId]
	@saleId int
AS
BEGIN
 SET NOCOUNT ON;

 SELECT Id, FirstName, LastName, EmailAddress, PhoneNumber
 FROM dbo.Customers
 WHERE Id = 
	(SELECT CustomerId
	FROM dbo.Sales
	WHERE Id = @saleId);
END
GO

CREATE PROCEDURE [dbo].[spCustomers_Insert]
	@firstName    VARCHAR (50),
  @lastName     VARCHAR (50),
  @emailAddress VARCHAR (100),
  @phoneNumber  VARCHAR (10)
AS
BEGIN
	SET NOCOUNT ON;

  DECLARE @id int;

  SELECT @id = Id
  FROM dbo.Customers
  WHERE EmailAddress = @emailAddress OR PhoneNumber = @phoneNumber;

  IF (@id IS NULL)
    BEGIN
      INSERT INTO dbo.Customers (FirstName, LastName, EmailAddress, PhoneNumber)
      OUTPUT inserted.Id
      VALUES (@firstName, @lastName, @emailAddress, @phoneNumber);
    END
  ELSE
    BEGIN
      SELECT @id
    END
END
GO

CREATE PROCEDURE [dbo].[spCustomers_Update]
	@id int,
	@firstName VARCHAR(50),
	@lastName VARCHAR(50),
	@emailAddress VARCHAR(100),
	@phoneNumber VARCHAR(10)
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE dbo.Customers
	SET FirstName = @firstName, LastName = @lastName, EmailAddress = @emailAddress, PhoneNumber = @phoneNumber
	WHERE Id = @id;
END
GO

CREATE PROCEDURE [dbo].[spGetRefundReceiptData]
	@transactionId int
AS
BEGIN
	SET NOCOUNT ON;

	SELECT s.Id AS SaleId, st.Id AS TransactionId, st.Amount, st.Method, st.Created, sl.DiscPct, rl.Qty AS RefundQty, sl.Price, t.TaxPct
	FROM dbo.Transactions st
	INNER JOIN dbo.RefundLines rl ON st.Id = rl.TransactionId
	INNER JOIN dbo.SaleLines sl ON rl.SaleLineId = sl.Id
	INNER JOIN dbo.Sales s ON st.SaleId = s.Id
	INNER JOIN dbo.Taxes t ON s.TaxId = t.Id
	WHERE st.Id = @transactionId;
END
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
/****** Object:  StoredProcedure [dbo].[spSaleLines_GetBySaleId]    Script Date: 8/31/2020 3:59:42 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[spSaleLines_GetBySaleId]
	@saleId int
AS
BEGIN
	SET NOCOUNT ON;

	SELECT sl.*, p.Sku, p.Upc, p.AllName
	FROM dbo.SaleLines sl
	INNER JOIN dbo.Products p ON sl.ProductId = p.Id
	WHERE sl.SaleId = @saleId;
END
GO
/****** Object:  StoredProcedure [dbo].[spSaleLines_Insert]    Script Date: 8/31/2020 3:59:42 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[spSaleLines_Insert]
	@saleId int, 
	@productId int, 
	@qty int
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @cost money;
	DECLARE @price money;

	SELECT @cost = Cost, @price = Price
	FROM dbo.Products
	WHERE Id = @productId;

	INSERT INTO dbo.SaleLines (SaleId, ProductId, Qty, DiscPct, Cost, Price)
	VALUES (@saleId, @productId, @qty, 15, @cost, @price);
END
GO
/****** Object:  StoredProcedure [dbo].[spSaleLines_Update]    Script Date: 8/31/2020 3:59:42 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[spSaleLines_Update]
	@id int,
	@qty int,
	@discPct float
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE dbo.SaleLines
	SET Qty = @qty, DiscPct = @discPct
	WHERE Id = @id
END
GO
/****** Object:  StoredProcedure [dbo].[spSales_CancelById]    Script Date: 8/31/2020 3:59:42 PM ******/
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
/****** Object:  StoredProcedure [dbo].[spSales_Insert]    Script Date: 8/31/2020 3:59:42 PM ******/
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
/****** Object:  StoredProcedure [dbo].[spSales_UpdateCustomerIdToGuest]    Script Date: 8/31/2020 3:59:42 PM ******/
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
/****** Object:  StoredProcedure [dbo].[spSearchSales]    Script Date: 8/31/2020 3:59:42 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[spSearchSales]
	@saleId int, 
	@lastName varchar(50), 
	@emailAddress varchar(100), 
	@phoneNumber varchar(10)
AS
BEGIN
	SET NOCOUNT ON;

	IF (@saleId > 0)
		BEGIN
			SELECT s.Id AS SaleId, s.Created, s.IsComplete,
						 CONCAT(c.FirstName, ' ', c.LastName) AS FullName, 
						 c.EmailAddress, c.PhoneNumber
			FROM dbo.Sales s
			INNER JOIN dbo.Customers c ON s.CustomerId = c.Id
			WHERE s.Id = @saleId;
		END
	ELSE IF (@lastName <> '')
		BEGIN
			SELECT s.Id AS SaleId, s.Created, s.IsComplete,
						 CONCAT(c.FirstName, ' ', c.LastName) AS FullName, 
						 c.EmailAddress, c.PhoneNumber
			FROM dbo.Customers c
			INNER JOIN dbo.Sales s ON c.Id = s.CustomerId
			WHERE c.LastName = @lastName;
		END
	ELSE IF (@emailAddress <> '')
		BEGIN
			SELECT s.Id AS SaleId, s.Created, s.IsComplete,
						 CONCAT(c.FirstName, ' ', c.LastName) AS FullName, 
						 c.EmailAddress, c.PhoneNumber
			FROM dbo.Customers c
			INNER JOIN dbo.Sales s ON c.Id = s.CustomerId
			WHERE c.EmailAddress = @emailAddress;
		END
	ELSE IF (@phoneNumber <> '')
		BEGIN
			SELECT s.Id AS SaleId, s.Created, s.IsComplete,
						 CONCAT(c.FirstName, ' ', c.LastName) AS FullName, 
						 c.EmailAddress, c.PhoneNumber
			FROM dbo.Customers c
			INNER JOIN dbo.Sales s ON c.Id = s.CustomerId
			WHERE c.PhoneNumber = @phoneNumber;
		END
	ELSE
		BEGIN
			RETURN NULL;
		END
END
GO
/****** Object:  StoredProcedure [dbo].[spTaxes_GetBySaleId]    Script Date: 8/31/2020 3:59:42 PM ******/
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
/****** Object:  StoredProcedure [dbo].[spTransactions_Insert]    Script Date: 8/31/2020 3:59:42 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[spTransactions_Insert]
	@saleId int,
	@amount float,
	@method varchar(15),
	@type VARCHAR(15),
	@message VARCHAR(200)
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO dbo.Transactions (SaleId, Amount, Method, Type, Message)
	OUTPUT inserted.Id
	VALUES (@saleId, @amount, @method, @type, @message);
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