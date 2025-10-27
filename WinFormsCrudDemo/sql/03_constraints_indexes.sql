USE DemoEmpresa;
GO

/* 1) CHECK: Salario >= 0 (o NULL) */
IF NOT EXISTS (
  SELECT 1
  FROM sys.check_constraints
  WHERE name = 'CK_Empleados_Salario'
)
BEGIN
  ALTER TABLE dbo.Empleados
  ADD CONSTRAINT CK_Empleados_Salario
  CHECK (Salario IS NULL OR Salario >= 0);
END
GO

/* 2) DEFAULT en FechaAlta = GETDATE() (por si no existiera) */
DECLARE @dfname sysname;
SELECT @dfname = dc.name
FROM sys.default_constraints dc
JOIN sys.columns c
  ON dc.parent_object_id = c.object_id
 AND dc.parent_column_id = c.column_id
WHERE dc.parent_object_id = OBJECT_ID('dbo.Empleados')
  AND c.name = 'FechaAlta';

IF @dfname IS NULL
BEGIN
  ALTER TABLE dbo.Empleados
  ADD CONSTRAINT DF_Empleados_FechaAlta DEFAULT (GETDATE()) FOR FechaAlta;
END
GO

/* 3) Índice para búsquedas por Nombre */
IF NOT EXISTS (
  SELECT 1 FROM sys.indexes
  WHERE name = 'IX_Empleados_Nombre' AND object_id = OBJECT_ID('dbo.Empleados')
)
BEGIN
  CREATE INDEX IX_Empleados_Nombre
  ON dbo.Empleados (Nombre);
END
GO

/* 4) Índice por FechaAlta para filtros de fecha */
IF NOT EXISTS (
  SELECT 1 FROM sys.indexes
  WHERE name = 'IX_Empleados_FechaAlta' AND object_id = OBJECT_ID('dbo.Empleados')
)
BEGIN
  CREATE INDEX IX_Empleados_FechaAlta
  ON dbo.Empleados (FechaAlta);
END
GO

/* 5) Auditoría simple: UpdatedAt + trigger */
IF COL_LENGTH('dbo.Empleados','UpdatedAt') IS NULL
BEGIN
  ALTER TABLE dbo.Empleados ADD UpdatedAt DATETIME2(0) NULL;
END
GO

IF OBJECT_ID('dbo.trg_Empleados_SetUpdatedAt','TR') IS NULL
BEGIN
  EXEC('
    CREATE TRIGGER dbo.trg_Empleados_SetUpdatedAt
    ON dbo.Empleados
    AFTER UPDATE
    AS
    BEGIN
      SET NOCOUNT ON;
      UPDATE e
      SET UpdatedAt = SYSDATETIME()
      FROM dbo.Empleados e
      JOIN inserted i ON e.IdEmpleado = i.IdEmpleado;
    END
  ');
END
GO
