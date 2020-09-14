CREATE PROCEDURE [dbo].[spGiftCards_Insert]
	@code VARCHAR(20),
	@amount MONEY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO dbo.GiftCards (Code, Amount)
	VALUES (@code, @amount);

	SELECT @@IDENTITY;
END
