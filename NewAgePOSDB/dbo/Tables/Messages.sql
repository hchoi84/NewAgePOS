﻿CREATE TABLE [dbo].[Messages]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY,
	[SaleId] INT NOT NULL,
	[Message] VARCHAR(100) NOT NULL,
	[Created] DATETIME2 NOT NULL DEFAULT getdate(),
	[Updated] DATETIME2 NOT NULL DEFAULT getdate(),
	CONSTRAINT [FK_Messages_Sales] FOREIGN KEY (SaleId) REFERENCES Sales(Id)
)