CREATE PROCEDURE [dbo].[spTransactions_GetByDateRange]
	@beginDate DATETIME2,
	@endDate DATETIME2
AS
BEGIN
	SET NOCOUNT ON;

	SELECT * 
	FROM dbo.Transactions t
	WHERE t.Created >= @beginDate 
	AND t.Created <= @endDate 
	AND (SELECT IsComplete 
			FROM dbo.Sales 
			WHERE Id = t.SaleId) = 1;
END