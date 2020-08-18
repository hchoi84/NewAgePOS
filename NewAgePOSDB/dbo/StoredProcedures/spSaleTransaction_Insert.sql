CREATE PROCEDURE [dbo].[spSaleTransaction_Insert]
	@saleId int,
	@amount int,
	@paymentType varchar(15)
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO dbo.SaleTransactions (SaleId, Amount, PaymentType)
	VALUES (@saleId, @amount, @paymentType);
END