﻿CREATE TABLE [dbo].[Taxes]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY,
	[TaxPct] FLOAT NOT NULL,
	[IsDefault] INT NOT NULL DEFAULT 0,
	[Created] DATETIME2 NOT NULL DEFAULT getdate(),
	[Updated] DATETIME2 NOT NULL DEFAULT getdate()
)
