﻿CREATE TABLE [dbo].[Products]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY,
	[Sku] VARCHAR(15) NOT NULL,
	[Upc] VARCHAR(20) NOT NULL,
	[Cost] MONEY NOT NULL,
	[Price] MONEY NOT NULL,
	[AllName] VARCHAR(150) NOT NULL,
	[Created] DATETIME2 NOT NULL DEFAULT getdate(),
	[Updated] DATETIME2 NOT NULL DEFAULT getdate()
)
