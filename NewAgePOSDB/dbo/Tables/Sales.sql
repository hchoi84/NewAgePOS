﻿CREATE TABLE [dbo].[Sales]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY,
	[CustomerId] INT NOT NULL,
	[TaxId] INT NOT NULL,
	[IsComplete] INT NOT NULL DEFAULT(0),
	[Message] VARCHAR(200),
	[Created] DATETIME2 NOT NULL DEFAULT getdate(),
	[Updated] DATETIME2 NOT NULL DEFAULT getdate(),
	CONSTRAINT [FK_Sales_Customers] FOREIGN KEY (CustomerId) REFERENCES Customers(Id),
	CONSTRAINT [FK_Sales_Taxes] FOREIGN KEY (TaxId) REFERENCES Taxes(Id)
)
