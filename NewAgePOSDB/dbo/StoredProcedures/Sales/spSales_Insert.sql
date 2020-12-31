CREATE PROCEDURE [dbo].[spSales_Insert]
	@userId varchar(10)
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @CustomerId int;
	DECLARE @TaxId int;

	SELECT @CustomerId = Id FROM dbo.Customers WHERE EmailAddress = 'guest@email.com';

	SELECT @TaxId = Id FROM dbo.Taxes WHERE IsDefault = 1

	INSERT INTO dbo.Sales (UserId, CustomerId, TaxId)
	OUTPUT inserted.Id
	VALUES (@userId, @customerId, @TaxId);
END