CREATE PROCEDURE [dbo].[spSales_UpdateCustomerIdToGuest]
	@saleId int
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @customerId int;

	SELECT @customerId = id
	FROM dbo.Customers
	WHERE EmailAddress = 'guest@email.com';

	UPDATE dbo.Sales
	SET CustomerId = @customerId;
END
