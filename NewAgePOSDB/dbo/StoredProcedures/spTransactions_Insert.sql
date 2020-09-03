CREATE PROCEDURE [dbo].[spTransactions_Insert]
	@saleId int,
	@giftCardId int,
	@amount float,
	@method varchar(15),
	@type VARCHAR(15),
	@message VARCHAR(200)
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO dbo.Transactions (SaleId, GiftCardId, Amount, Method, Type, Message)
	OUTPUT inserted.Id
	VALUES (@saleId, @giftCardId, @amount, @method, @type, @message);
END