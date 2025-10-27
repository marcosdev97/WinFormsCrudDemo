USE DemoEmpresa;
GO
CREATE OR ALTER PROCEDURE sp_Empleados_GetAll AS
BEGIN
  SET NOCOUNT ON;
  SELECT IdEmpleado,Nombre,Puesto,Salario,FechaAlta
  FROM dbo.Empleados ORDER BY IdEmpleado ASC;
END
GO

CREATE OR ALTER PROCEDURE sp_Empleados_SearchByName @q NVARCHAR(50)
AS
BEGIN
  SET NOCOUNT ON;
  SELECT IdEmpleado,Nombre,Puesto,Salario,FechaAlta
  FROM dbo.Empleados
  WHERE Nombre LIKE '%'+@q+'%'
  ORDER BY IdEmpleado ASC;
END
GO

CREATE OR ALTER PROCEDURE sp_Empleados_Insert
  @Nombre NVARCHAR(50),
  @Puesto NVARCHAR(50)=NULL,
  @Salario DECIMAL(10,2)=NULL,
  @FechaAlta DATE
AS
BEGIN
  SET NOCOUNT ON;
  INSERT INTO dbo.Empleados (Nombre,Puesto,Salario,FechaAlta)
  VALUES (@Nombre,@Puesto,@Salario,@FechaAlta);
  SELECT CAST(SCOPE_IDENTITY() AS INT) AS NewId;
END
GO

CREATE OR ALTER PROCEDURE sp_Empleados_Update
  @IdEmpleado INT,
  @Nombre NVARCHAR(50),
  @Puesto NVARCHAR(50)=NULL,
  @Salario DECIMAL(10,2)=NULL,
  @FechaAlta DATE
AS
BEGIN
  SET NOCOUNT ON;
  UPDATE dbo.Empleados
  SET Nombre=@Nombre, Puesto=@Puesto, Salario=@Salario, FechaAlta=@FechaAlta
  WHERE IdEmpleado=@IdEmpleado;
END
GO

CREATE OR ALTER PROCEDURE sp_Empleados_Delete @IdEmpleado INT
AS
BEGIN
  SET NOCOUNT ON;
  DELETE FROM dbo.Empleados WHERE IdEmpleado=@IdEmpleado;
END
GO
