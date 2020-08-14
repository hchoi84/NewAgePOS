CREATE PROCEDURE [dbo].[spSales_Insert]
	@customerId int,
	@taxId int
AS
BEGIN
	SET NOCOUNT ON;

	SELECT TOP 1 @customerId = Id FROM dbo.Customers;

	SELECT TOP 1 @taxId = Id FROM dbo.Taxes;

	INSERT INTO dbo.Sales (CustomerId, TaxId)
	OUTPUT inserted.Id
	VALUES (@customerId, @taxId);
END
