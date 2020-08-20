CREATE TABLE [dbo].[RefundLines]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY,
	[SaleLineId] INT NOT NULL,
	[SaleTransactionId] INT NOT NULL,
	[Qty] INT NOT NULL,
	[Created] DATE NOT NULL DEFAULT(getdate()),
	[Updated] DATE NOT NULL DEFAULT(getdate()),
	CONSTRAINT [FK_RefundLines_SaleLines] FOREIGN KEY (SaleLineId) REFERENCES SaleLines(Id),
	CONSTRAINT [FK_RefundLines_SaleTransactions] FOREIGN KEY (SaleTransactionId) REFERENCES SaleTransactions(Id)
)
