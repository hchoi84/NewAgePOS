CREATE PROCEDURE [dbo].[spRefundLines_GetBySaleId]
	@saleId int
AS
BEGIN
	SET NOCOUNT ON;

	SELECT rl.*
	FROM dbo.SaleLines sl
	INNER JOIN dbo.RefundLines rl ON sl.Id = rl.SaleLineId
	WHERE SaleId = @saleId;
END