-- Procedimientos almacenados de la aplicación web Biblioteca (Semana 08).
-- Reutiliza la base BibliotecaDB de la semana 7: no crea la base ni las tablas.
-- Los nombres van en plural (usp_Libros_*, usp_Socios_*) para no tocar los
-- procedimientos de la semana 7 (usp_Libro_*, usp_Socio_*), que usa otra aplicación.
-- Se puede ejecutar varias veces: CREATE OR ALTER reemplaza lo que ya existe.
USE BibliotecaDB;
GO

-- =============================================================
-- STORED PROCEDURES: Libros
-- =============================================================

-- Libros activos con el nombre y la nacionalidad de su autor.
CREATE OR ALTER PROCEDURE dbo.usp_Libros_Listar
AS
BEGIN
    SET NOCOUNT ON;

    SELECT l.LibroId, l.Titulo, l.ISBN, l.AutorId, l.Ejemplares,
           a.Nombre AS AutorNombre, a.Nacionalidad AS AutorNacionalidad
    FROM Libros l
    INNER JOIN Autores a ON a.AutorId = l.AutorId
    WHERE l.Activo = 1
    ORDER BY l.Titulo;
END
GO

-- Busca solo en el título (no en el autor). Si llega vacío o NULL, devuelve todos.
CREATE OR ALTER PROCEDURE dbo.usp_Libros_BuscarPorTitulo
    @Titulo NVARCHAR(150)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT l.LibroId, l.Titulo, l.ISBN, l.AutorId, l.Ejemplares,
           a.Nombre AS AutorNombre, a.Nacionalidad AS AutorNacionalidad
    FROM Libros l
    INNER JOIN Autores a ON a.AutorId = l.AutorId
    WHERE l.Activo = 1
      AND l.Titulo LIKE N'%' + ISNULL(@Titulo, N'') + N'%'
    ORDER BY l.Titulo;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Libros_ObtenerPorId
    @LibroId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT l.LibroId, l.Titulo, l.ISBN, l.AutorId, l.Ejemplares,
           a.Nombre AS AutorNombre, a.Nacionalidad AS AutorNacionalidad
    FROM Libros l
    INNER JOIN Autores a ON a.AutorId = l.AutorId
    WHERE l.LibroId = @LibroId AND l.Activo = 1;
END
GO

-- Activo toma su valor por defecto (1).
CREATE OR ALTER PROCEDURE dbo.usp_Libros_Insertar
    @Titulo     NVARCHAR(150),
    @ISBN       VARCHAR(13),
    @AutorId    INT,
    @Ejemplares INT
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO Libros (Titulo, ISBN, AutorId, Ejemplares)
    VALUES (@Titulo, @ISBN, @AutorId, @Ejemplares);
END
GO

-- Solo actualiza libros activos: uno dado de baja ya no se edita.
CREATE OR ALTER PROCEDURE dbo.usp_Libros_Actualizar
    @LibroId    INT,
    @Titulo     NVARCHAR(150),
    @ISBN       VARCHAR(13),
    @AutorId    INT,
    @Ejemplares INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE Libros
    SET Titulo = @Titulo,
        ISBN = @ISBN,
        AutorId = @AutorId,
        Ejemplares = @Ejemplares
    WHERE LibroId = @LibroId AND Activo = 1;
END
GO

-- Eliminación lógica: el libro se desactiva, pero sigue en el historial de préstamos.
CREATE OR ALTER PROCEDURE dbo.usp_Libros_Eliminar
    @LibroId INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE Libros SET Activo = 0 WHERE LibroId = @LibroId;
END
GO

-- Devuelve cuántos libros (distintos a @LibroId) ya usan ese ISBN.
-- Cuenta también los inactivos, porque la restricción UNIQUE los incluye.
-- Al crear se envía @LibroId = 0.
CREATE OR ALTER PROCEDURE dbo.usp_Libros_ExisteIsbn
    @ISBN    VARCHAR(13),
    @LibroId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT COUNT(*)
    FROM Libros
    WHERE ISBN = @ISBN AND LibroId <> @LibroId;
END
GO

-- =============================================================
-- STORED PROCEDURES: Socios
-- =============================================================

-- Socios activos con la cantidad de libros que todavía no devuelven
-- (FechaDevolucion NULL). Los socios sin préstamos aparecen con 0.
CREATE OR ALTER PROCEDURE dbo.usp_Socios_Listar
AS
BEGIN
    SET NOCOUNT ON;

    SELECT s.SocioId, s.DNI, s.Nombre, s.Email,
           (SELECT COUNT(*)
            FROM Prestamos p
            INNER JOIN DetallePrestamo d ON d.PrestamoId = p.PrestamoId
            WHERE p.SocioId = s.SocioId
              AND d.FechaDevolucion IS NULL) AS LibrosPendientes
    FROM Socios s
    WHERE s.Activo = 1
    ORDER BY s.Nombre;
END
GO

-- Un correo vacío o solo con espacios se guarda como NULL.
CREATE OR ALTER PROCEDURE dbo.usp_Socios_Insertar
    @DNI    VARCHAR(8),
    @Nombre NVARCHAR(100),
    @Email  NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO Socios (DNI, Nombre, Email)
    VALUES (@DNI, @Nombre, NULLIF(LTRIM(RTRIM(@Email)), N''));
END
GO

-- Devuelve cuántos socios ya usan ese DNI (también los inactivos, por la restricción UNIQUE).
CREATE OR ALTER PROCEDURE dbo.usp_Socios_ExisteDni
    @DNI VARCHAR(8)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT COUNT(*)
    FROM Socios
    WHERE DNI = @DNI;
END
GO

-- =============================================================
-- STORED PROCEDURES: Autores
-- =============================================================

-- Autores activos para la lista desplegable del formulario de libros.
CREATE OR ALTER PROCEDURE dbo.usp_Autores_Listar
AS
BEGIN
    SET NOCOUNT ON;

    SELECT AutorId, Nombre, Nacionalidad
    FROM Autores
    WHERE Activo = 1
    ORDER BY Nombre;
END
GO

-- =============================================================
-- STORED PROCEDURES: Préstamos
-- =============================================================

-- Reporte de préstamos entre dos fechas: una fila por préstamo.
-- INNER JOIN entre Prestamos, DetallePrestamo, Libros y Socios; los títulos se unen con '|'.
-- No filtra por Activo para que el historial muestre también libros o socios dados de baja.
CREATE OR ALTER PROCEDURE dbo.usp_Prestamos_Reporte
    @Desde DATE,
    @Hasta DATE
AS
BEGIN
    SET NOCOUNT ON;

    SELECT p.PrestamoId,
           s.Nombre AS SocioNombre,
           s.DNI AS SocioDNI,
           STRING_AGG(l.Titulo, N'|') WITHIN GROUP (ORDER BY l.Titulo) AS Libros,
           SUM(CASE WHEN d.FechaDevolucion IS NULL THEN 1 ELSE 0 END) AS LibrosPendientes,
           p.FechaPrestamo, p.FechaLimite, p.Estado
    FROM Prestamos p
    INNER JOIN DetallePrestamo d ON d.PrestamoId = p.PrestamoId
    INNER JOIN Libros l ON l.LibroId = d.LibroId
    INNER JOIN Socios s ON s.SocioId = p.SocioId
    WHERE p.FechaPrestamo BETWEEN @Desde AND @Hasta
    GROUP BY p.PrestamoId, s.Nombre, s.DNI, p.FechaPrestamo, p.FechaLimite, p.Estado
    ORDER BY p.FechaPrestamo DESC, p.PrestamoId DESC;
END
GO
