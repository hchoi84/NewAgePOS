CREATE PROCEDURE [dbo].[spProducts_GetByValues]
	@sku varchar(15),
	@upc varchar(20),
	@cost money,
	@price money,
	@allName varchar(150)
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @id int;

	SELECT Top 1 @id = Id
	FROM dbo.Products
	WHERE Sku = @sku AND Upc = @upc AND Cost = @cost AND Price = @price AND AllName = @allName
	ORDER BY Id;

	IF (@id IS NULL)
		INSERT INTO dbo.Products (Sku, Upc, Cost, Price, AllName)
		OUTPUT inserted.Id
		VALUES (@sku, @upc, @cost, @price, @allName);
	ELSE
		SELECT @id;
END
