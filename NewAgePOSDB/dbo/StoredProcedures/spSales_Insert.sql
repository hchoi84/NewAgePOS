CREATE PROCEDURE [dbo].[spSales_Insert]
AS
BEGIN
	SET NOCOUNT ON;
	
	DECLARE @customerId int;
	DECLARE @taxId int;

	SELECT @customerId = MIN(Id) FROM dbo.Customers;

	SELECT @taxId = MIN(Id) FROM dbo.Taxes;

	INSERT INTO dbo.Sales (CustomerId, TaxId)
	OUTPUT inserted.Id
	VALUES (@customerId, @taxId);
END
