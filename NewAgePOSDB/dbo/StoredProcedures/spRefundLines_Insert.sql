CREATE PROCEDURE [dbo].[spRefundLines_Insert]
	@saleLineId int,
	@refundQty int
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @qty int;

	SELECT @qty = Qty 
	FROM dbo.RefundLines 
	WHERE SaleLineId = @saleLineId AND TransactionId IS NULL;

	IF (@qty IS NULL)
		BEGIN
			INSERT INTO dbo.RefundLines (SaleLineId, Qty)
			VALUES (@saleLineId, @refundQty);
		END
	ELSE
		BEGIN
			UPDATE dbo.RefundLines
			SET Qty += @refundQty
			WHERE SaleLineId = @saleLineId AND TransactionId IS NULL;
		END
END