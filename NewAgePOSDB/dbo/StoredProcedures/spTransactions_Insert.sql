CREATE PROCEDURE [dbo].[spTransactions_Insert]
	@saleId int,
	@amount float,
	@paymentType varchar(15),
	@reason VARCHAR(15),
	@message VARCHAR(200)
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO dbo.Transactions (SaleId, Amount, PaymentType, Reason, Message)
	OUTPUT inserted.Id
	VALUES (@saleId, @amount, @paymentType, @reason, @message);
END