CREATE TABLE [dbo].[SaleTransactions]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY,
	[SaleId] INT NOT NULL,
	[Amount] MONEY NOT NULL,
	[PaymentType] VARCHAR(15) NOT NULL,
	[Reason] VARCHAR(15) NOT NULL,
	[Message] VARCHAR(200),
	[Created] DATE NOT NULL DEFAULT getdate(),
	[Updated] DATE NOT NULL DEFAULT getdate(),
	CONSTRAINT [FK_SaleTransactions_Sales] FOREIGN KEY (SaleId) REFERENCES Sales(Id)
)
