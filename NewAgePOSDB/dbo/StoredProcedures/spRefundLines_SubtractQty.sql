CREATE PROCEDURE [dbo].[spRefundLines_SubtractQty]
	@id int,
	@subtractQty int
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @qty int;

	SELECT @qty = Qty
	FROM dbo.RefundLines
	WHERE Id = @id;

	IF (@qty <= @subtractQty)
		BEGIN
			DELETE FROM dbo.RefundLines WHERE Id = @id;
		END
	ELSE
		BEGIN
			UPDATE dbo.RefundLines
			SET Qty -= @subtractQty
			WHERE Id = @id;
		END
END