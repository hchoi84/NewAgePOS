CREATE PROCEDURE [dbo].[spSaleLines_GetBySaleId]
	@saleId int
AS
BEGIN
	SET NOCOUNT ON;

	SELECT sl.*, p.Sku, p.Upc, p.Cost, p.Price, p.AllName
	FROM dbo.SaleLines sl
	INNER JOIN dbo.Products p ON sl.ProductId = p.Id
	WHERE sl.SaleId = @saleId;
END