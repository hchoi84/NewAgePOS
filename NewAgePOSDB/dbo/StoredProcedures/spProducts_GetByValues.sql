CREATE PROCEDURE [dbo].[spProducts_GetByValues]
	@sku VARCHAR(15),
	@upc VARCHAR(20),
	@cost MONEY,
	@price MONEY,
	@allName VARCHAR(150)
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @id int;

	SELECT @id = Id
	FROM dbo.Products
	WHERE Sku = @sku AND Upc = @upc AND Cost = @cost AND Price = @price AND AllName = @allName;

	IF (@id IS NULL)
		BEGIN
			INSERT INTO dbo.Products (Sku, Upc, Cost, Price, AllName)
			OUTPUT inserted.Id
			VALUES (@sku, @upc, @cost, @price, @allName);
		END
	ELSE
		BEGIN
			SELECT @id AS Id
		END
END
