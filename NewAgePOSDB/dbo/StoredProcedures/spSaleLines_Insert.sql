CREATE PROCEDURE [dbo].[spSaleLines_Insert]
	@saleId int, 
	@productId int, 
	@giftCardId int,
	@qty int
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @price money;

	IF (@giftCardId IS NULL)
		BEGIN
			DECLARE @cost money;

			SELECT @cost = Cost, @price = Price
			FROM dbo.Products
			WHERE Id = @productId;

			INSERT INTO dbo.SaleLines (SaleId, ProductId, Qty, DiscPct, Cost, Price)
			VALUES (@saleId, @productId, @qty, 15, @cost, @price);
		END
	ELSE
		BEGIN
			SELECT @price = Amount
			FROM dbo.GiftCards
			WHERE Id = @giftCardId;

			INSERT INTO dbo.SaleLines (SaleId, GiftCardId, Qty, DiscPct, Cost, Price)
			VALUES (@saleId, @giftCardId, @qty, 0, 0, @price);
		END
END