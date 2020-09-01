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
