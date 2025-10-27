# 🧾 WinForms CRUD Demo – SQL Server + ADO.NET + Stored Procedures + Logging

Proyecto de escritorio **C# (.NET Framework 4.8)** que implementa un sistema completo de gestión de empleados  
(**Create, Read, Update, Delete**) conectado a una base de datos **SQL Server LocalDB**.  

Este proyecto reproduce un flujo empresarial real con buenas prácticas:  
**procedimientos almacenados, validaciones de interfaz, logging de errores, constraints SQL e índices de rendimiento.**

---

## 🚀 Características principales

✅ **CRUD funcional completo**: alta, modificación, búsqueda y eliminación de empleados.  
✅ **Interfaz WinForms** intuitiva con `DataGridView` y validaciones visuales (`ErrorProvider`).  
✅ **Procedimientos almacenados (SPs)** para todas las operaciones con la base de datos.  
✅ **Validaciones dobles**: en la interfaz (UI) y en la base de datos.  
✅ **Logging persistente** con rotación diaria (`logs/app_YYYYMMDD.log`).  
✅ **Restricciones y triggers SQL** para mantener integridad de datos.  
✅ **Exportación a CSV** compatible con Excel (formato europeo `sep=;`).  
✅ Arquitectura clara: **UI + Data + SQL + Utils**, ideal como base para proyectos empresariales.

---

## 🧩 Estructura del proyecto

```
WinFormsCrudDemo/
│
├── Data/                  # Acceso a datos mediante ADO.NET
│   └── Db.cs              # CRUD conectado a SQL Server vía Stored Procedures
│
├── Utils/                 # Utilidades comunes
│   └── Logger.cs          # Logging de errores con rotación diaria
│
├── sql/                   # Scripts SQL
│   ├── 01_schema.sql                  # Creación de la base de datos y tabla Empleados
│   ├── 02_procs.sql                   # Procedimientos almacenados (CRUD)
│   └── 03_constraints_indexes.sql     # Constraints, índices y trigger UpdatedAt
│
├── MainForm.cs            # Interfaz principal (UI + validaciones)
├── Program.cs             # Punto de entrada con manejo global de excepciones
└── README.md
```

---

## 🗃️ Base de datos

**Servidor:** `(localdb)\MSSQLLocalDB`  
**Base de datos:** `DemoEmpresa`  
**Tabla principal:** `Empleados`

### Campos
| Campo | Tipo | Descripción |
|--------|------|-------------|
| `IdEmpleado` | `INT IDENTITY` | Clave primaria |
| `Nombre` | `NVARCHAR(100)` | Obligatorio |
| `Puesto` | `NVARCHAR(100)` | Opcional |
| `Salario` | `DECIMAL(10,2)` | Puede ser nulo, debe ser ≥ 0 |
| `FechaAlta` | `DATETIME` | Por defecto GETDATE() |
| `UpdatedAt` | `DATETIME2` | Actualización automática vía trigger |

---

## 🧠 Procedimientos almacenados (Stored Procedures)

Los accesos a la base de datos se realizan exclusivamente mediante SPs:

- `sp_Empleados_GetAll`
- `sp_Empleados_SearchByName`
- `sp_Empleados_Insert`
- `sp_Empleados_Update`
- `sp_Empleados_Delete`

👉 Todos se encuentran en `/sql/02_procs.sql`.

---

## 🔒 Constraints, índices y auditoría

**Scripts:** `/sql/03_constraints_indexes.sql`

Incluye:
- `CHECK (Salario >= 0)`  
- `DEFAULT (GETDATE())` en `FechaAlta`  
- Índices en `Nombre` y `FechaAlta`  
- Trigger `trg_Empleados_SetUpdatedAt` para mantener `UpdatedAt` en cada UPDATE.

---

## 🧱 Validaciones y control de errores

### En la interfaz (UI)
- `ErrorProvider` indica errores directamente en los campos.
- `MessageBox` muestra advertencias claras y mensajes de éxito.
- Validaciones antes del guardado (nombre obligatorio, salario numérico ≥ 0, fecha coherente).

### En la base de datos
- Constraints garantizan integridad aunque la app falle.

### Logging
- Excepciones registradas automáticamente en `logs/app_YYYYMMDD.log`.
- Carpeta creada dinámicamente en `bin/Debug/logs/`.
- Incluye hora, contexto y excepción completa.

---

## 📊 Exportar datos

El proyecto permite exportar los datos visibles del `DataGridView` a **CSV europeo (`sep=;`)**  
para abrir directamente en Excel con acentos y columnas bien formateadas.

Ejemplo:
```
sep=;
"ID";"Nombre";"Puesto";"Salario (€)";"Fecha Alta"
1;"Marcos Pérez";"Programador";"23000,00";"12/09/2025"
```

---

## ⚙️ Ejecución

1. Clona el repositorio:
   ```bash
   git clone https://github.com/marcosdev97/WinFormsCrudDemo.git
   ```

2. Abre el proyecto en **Visual Studio 2022**.

3. Ejecuta los scripts SQL (`01_schema.sql`, `02_procs.sql`, `03_constraints_indexes.sql`)  
   en tu instancia `(localdb)\MSSQLLocalDB`.

4. Revisa la cadena de conexión en `App.config`:
   ```xml
   <add name="DemoDb"
        connectionString="Server=(localdb)\MSSQLLocalDB;Database=DemoEmpresa;Trusted_Connection=True;" />
   ```

5. Compila y ejecuta con **F5**.

---

## 📂 Carpeta `logs`

Se crea automáticamente al producirse un error.  
Ubicación típica:
```
bin/Debug/logs/app_YYYYMMDD.log
```

Cada archivo contiene las trazas del día:
```
[2025-10-27 22:14:35] [Db.Insert] System.Data.SqlClient.SqlException: 
Violation of CHECK constraint 'CK_Empleados_Salario'...
```

---

## 💡 Próximas mejoras planificadas

- **v1.6:** Implementar capa de servicios y repositorio (`EmpleadosService`, `IEmpleadosRepository`)  
  para desacoplar la UI de la base de datos.
- **v1.7:** Migración opcional a **Entity Framework 6** manteniendo la misma interfaz.
- **v1.8:** Implementar exportación a Excel (XLSX) con ClosedXML.

---

## ✨ Autor

**Marcos Pérez**  
Desarrollador C# / Unity / XR  
[LinkedIn](https://www.linkedin.com/in/marcos-p%C3%A9rez-gonz%C3%A1lez/)

---

## 📜 Licencia

Proyecto educativo y demostrativo de libre uso (MIT License).
