CREATE PROCEDURE [dbo].[spSaleLines_GetBySaleId]
	@saleId int
AS
BEGIN
	SET NOCOUNT ON;

	SELECT sl.Id, p.Sku, p.Upc, p.Cost, p.Price,
		sl.Qty, sl.DiscAmt, sl.DiscPct, 
		((p.Price - sl.DiscAmt) * (1 - sl.DiscPct / 100) * sl.Qty) AS LineTotal,
		p.AllName
	FROM dbo.SaleLines sl
	INNER JOIN dbo.Products p ON sl.ProductId = p.Id
	WHERE SaleId = @saleId;
END
