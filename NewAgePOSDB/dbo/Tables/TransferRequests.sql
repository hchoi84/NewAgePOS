CREATE TABLE [dbo].[TransferRequests]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY,
	[Description] VARCHAR(20) NOT NULL,
	[CreatorName] VARCHAR(10) NOT NULL,
	[Status] VARCHAR(10) NOT NULL,
	[Created] DATETIME2 NOT NULL DEFAULT getdate(),
	[Updated] DATETIME2 NOT NULL DEFAULT getdate()
)
