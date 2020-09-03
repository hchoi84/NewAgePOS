CREATE PROCEDURE [dbo].[spTransactions_Insert]
	@saleId int,
	@amount float,
	@method varchar(15),
	@type VARCHAR(15),
	@message VARCHAR(200)
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO dbo.Transactions (SaleId, Amount, Method, Type, Message)
	OUTPUT inserted.Id
	VALUES (@saleId, @amount, @method, @type, @message);
END