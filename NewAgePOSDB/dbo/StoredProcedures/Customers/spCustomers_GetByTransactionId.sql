CREATE PROCEDURE [dbo].[spCustomers_GetByTransactionId]
	@transactionId int
AS
BEGIN
	SET NOCOUNT ON;

	SELECT c.*
	FROM dbo.Transactions t
	INNER JOIN dbo.Sales s ON s.Id = t.SaleId
	INNER JOIN dbo.Customers c ON c.Id = s.CustomerId
	WHERE t.Id = @transactionId;
END