﻿CREATE TABLE [dbo].[Taxes]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY,
	[TaxPct] FLOAT NOT NULL,
	[Created] DATE NOT NULL DEFAULT getdate(),
	[Updated] DATE NOT NULL DEFAULT getdate()
)
