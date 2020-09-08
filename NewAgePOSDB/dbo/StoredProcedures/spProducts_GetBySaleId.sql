CREATE PROCEDURE [dbo].[spProducts_GetBySaleId]
	@saleId int
AS
BEGIN
	SET NOCOUNT ON;

	SELECT p.*
	FROM dbo.SaleLines s
	INNER JOIN dbo.Products p ON s.ProductId = p.Id
	WHERE s.ProductId IS NOT NULL AND s.SaleId = @saleId;
END