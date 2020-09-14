CREATE PROCEDURE [dbo].[spGiftCards_GetBySaleId]
	@saleId int
AS
BEGIN
	SET NOCOUNT ON;

	SELECT gc.*
	FROM dbo.SaleLines s
	INNER JOIN dbo.GiftCards gc ON s.GiftCardId = gc.Id
	WHERE s.GiftCardId IS NOT NULL AND s.SaleId = @saleId;
END