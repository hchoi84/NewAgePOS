﻿CREATE TABLE [dbo].[Transactions]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY,
	[SaleId] INT NOT NULL,
	[GiftCardId] INT,
	[Amount] MONEY NOT NULL,
	[Method] VARCHAR(15) NOT NULL,
	[Type] VARCHAR(15) NOT NULL,
	[Created] DATETIME2 NOT NULL DEFAULT getdate(),
	[Updated] DATETIME2 NOT NULL DEFAULT getdate(),
	CONSTRAINT [FK_Transactions_Sales] FOREIGN KEY (SaleId) REFERENCES Sales(Id),
	CONSTRAINT [FK_Transactions_GiftCards] FOREIGN KEY (GiftCardId) REFERENCES GiftCards(Id)
)
