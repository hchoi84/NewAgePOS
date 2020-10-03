CREATE TABLE [dbo].[TransferRequestItems]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY,
	[TransferRequestId] INT NOT NULL,
	[Sku] VARCHAR(15) NOT NULL,
	[Qty] INT NOT NULL,
	[Created] DATETIME2 NOT NULL DEFAULT getdate(),
	[Updated] DATETIME2 NOT NULL DEFAULT getdate(),
	CONSTRAINT [FK_TransferRequestItems_TransferRequests] FOREIGN KEY (TransferRequestId) REFERENCES TransferRequests(Id)
)
