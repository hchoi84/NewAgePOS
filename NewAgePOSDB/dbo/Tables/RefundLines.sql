CREATE TABLE [dbo].[RefundLines]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY,
	[SaleLineId] INT NOT NULL,
	[TransactionId] INT,
	[Qty] INT NOT NULL,
	[Created] DATETIME2 NOT NULL DEFAULT(getdate()),
	[Updated] DATETIME2 NOT NULL DEFAULT(getdate()),
	CONSTRAINT [FK_RefundLines_SaleLines] FOREIGN KEY (SaleLineId) REFERENCES SaleLines(Id),
	CONSTRAINT [FK_RefundLines_Transactions] FOREIGN KEY (TransactionId) REFERENCES Transactions(Id)
)
