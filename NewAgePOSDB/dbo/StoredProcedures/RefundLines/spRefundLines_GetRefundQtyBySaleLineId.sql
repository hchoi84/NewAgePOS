CREATE PROCEDURE [dbo].[spRefundLines_GetRefundQtyBySaleLineId]
	@saleLineId int
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @refundedQty int;

	SELECT @refundedQty = SUM(Qty) FROM dbo.RefundLines WHERE SaleLineId = @saleLineId;

	IF (@refundedQty IS NULL)
		SELECT 0;
	ELSE
		SELECT @refundedQty;
END