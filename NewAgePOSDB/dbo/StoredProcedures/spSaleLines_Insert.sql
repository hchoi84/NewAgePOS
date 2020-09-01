CREATE PROCEDURE [dbo].[spSaleLines_Insert]
	@saleId int, 
	@productId int, 
	@qty int
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @cost money;
	DECLARE @price money;

	SELECT @cost = Cost, @price = Price
	FROM dbo.Products
	WHERE Id = @productId;

	INSERT INTO dbo.SaleLines (SaleId, ProductId, Qty, DiscPct, Cost, Price)
	VALUES (@saleId, @productId, @qty, 15, @cost, @price);
END