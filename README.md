# Biblioteca.Web

Aplicación ASP.NET Core MVC (.NET 10) para la biblioteca: mantenimiento de libros, registro de socios y reporte de préstamos por fechas.
Usa Dapper y procedimientos almacenados sobre la base `BibliotecaDB` de la semana anterior (no crea una base nueva).

## Requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server Express en `.\SQLEXPRESS` con la base `BibliotecaDB` ya creada (scripts de la semana 7: `01_BibliotecaDB.sql` y `02_Procedimientos.sql`).

## Cómo ejecutar

1. Crear los procedimientos almacenados de esta semana (el script se puede ejecutar varias veces):

   ```bash
   cd Scripts
   sqlcmd -S ".\SQLEXPRESS" -E -C -b -f 65001 -i 01_Procedimientos_Web.sql
   ```

2. Ejecutar la web (desde la raíz del repositorio):

   ```bash
   dotnet run --launch-profile http
   ```

3. Abrir <http://localhost:5265>

La cadena de conexión `BibliotecaDB` está en `appsettings.json` y se lee con `IConfiguration` en cada repositorio.

## Procedimientos almacenados (`Scripts/01_Procedimientos_Web.sql`)

Usan nombres en plural para no chocar con los de la semana 7 (`usp_Libro_*`, `usp_Socio_*`), que la aplicación WPF sigue usando.

| Procedimiento                | Uso                                                                 |
|------------------------------|---------------------------------------------------------------------|
| `usp_Libros_Listar`          | Libros activos con el nombre del autor (`INNER JOIN Autores`)       |
| `usp_Libros_BuscarPorTitulo` | Igual que el listado, filtrando por título (`LIKE`)                 |
| `usp_Libros_ObtenerPorId`    | Un libro activo con su autor (Details, Edit, Delete)                |
| `usp_Libros_Insertar`        | Registra un libro                                                    |
| `usp_Libros_Actualizar`      | Modifica un libro                                                    |
| `usp_Libros_Eliminar`        | Eliminación lógica (`Activo = 0`), nunca `DELETE`                    |
| `usp_Libros_ExisteIsbn`      | Valida que el ISBN no se repita                                      |
| `usp_Socios_Listar`          | Socios activos con la cantidad de libros que aún no devuelven        |
| `usp_Socios_Insertar`        | Registra un socio                                                    |
| `usp_Socios_ExisteDni`       | Valida que el DNI no se repita                                       |
| `usp_Autores_Listar`         | Autores activos para la lista desplegable                            |
| `usp_Prestamos_Reporte`      | Préstamos entre dos fechas (`INNER JOIN` de Prestamos, DetallePrestamo, Libros y Socios) |

## Estructura del proyecto

| Carpeta        | Contenido                                                                          |
|----------------|------------------------------------------------------------------------------------|
| `Models`       | `Libro`, `Socio`, `PrestamoReporte` (y `Autor` para el combo), con DataAnnotations |
| `Repositorios` | `LibroRepositorio`, `SocioRepositorio`, `AutorRepositorio`, `PrestamoRepositorio`: Dapper + procedimientos |
| `Controllers`  | `LibrosController`, `SociosController`, `PrestamosController`: acciones `async`, sin SQL |
| `Views`        | Vistas Razor con `@model`; `_LibroFila` es la vista parcial de la fila de libro    |

## Explicación: recorrido de una petición

Acción elegida: **búsqueda de libros por título** (`LibrosController.Index`).

1. **Ruta.** El usuario escribe "ciudad" en el buscador y el formulario (`method="get"`) pide `GET /Libros?titulo=ciudad`.
   La ruta por defecto `{controller=Home}/{action=Index}/{id?}` de `Program.cs` elige `LibrosController` y la acción `Index`. El model binding toma `titulo` del query string.
2. **Controlador.** `Index(string? titulo)` recibe por constructor un `ILibroRepositorio`.
   El contenedor de dependencias crea un `LibroRepositorio` por petición, porque está registrado con `AddScoped` en `Program.cs`.
   Como `titulo` tiene texto, la acción llama a `await _libroRepositorio.BuscarPorTituloAsync("ciudad")`. El controlador no escribe SQL ni abre conexiones.
3. **Repositorio.** `LibroRepositorio` leyó la cadena `BibliotecaDB` de `appsettings.json` mediante `IConfiguration`.
   Abre un `SqlConnection` y ejecuta, con Dapper, `QueryAsync<Libro>("usp_Libros_BuscarPorTitulo", new { Titulo = titulo }, commandType: CommandType.StoredProcedure)`. El valor viaja como parámetro, nunca concatenado.
4. **Procedimiento almacenado.** `usp_Libros_BuscarPorTitulo` hace `INNER JOIN` entre `Libros` y `Autores` y filtra `Activo = 1` y `Titulo LIKE '%ciudad%'`.
   Devuelve `a.Nombre AS AutorNombre`. Dapper llena cada `Libro` emparejando los nombres de columna con los de las propiedades, y por eso el alias coincide con la propiedad.
5. **Vista.** La acción guarda el texto buscado en `ViewData["Busqueda"]` y devuelve `View(libros)`.
   `Views/Libros/Index.cshtml` es fuertemente tipada (`@model IEnumerable<Libro>`) y dibuja cada fila con la vista parcial `_LibroFila`. Todo se muestra dentro de `_Layout.cshtml`.
   Si la petición viene de la búsqueda en vivo (`fetch` con la cabecera `X-Requested-With`), la acción devuelve solo `PartialView("_LibrosResultados", libros)`, que reutiliza la misma `_LibroFila`.

**Cómo se pasan los datos a la vista y por qué:**

- **Modelo** (`View(libros)`): la lista de libros es el dato principal de la página. Con `@model`, la vista es fuertemente tipada y los errores de nombres se detectan al compilar.
- **ViewData** (`ViewData["Busqueda"]`): el texto buscado es un dato auxiliar que no forma parte de `Libro`. Solo sirve para volver a llenar la caja de búsqueda y el mensaje de "sin resultados" en esta misma petición.
  También se usa `ViewData` para la lista desplegable de autores (`ViewData["Autores"]`) y para el rango de fechas del reporte (`ViewData["Desde"]`, `ViewData["Hasta"]`).
- **TempData**: no se usa en el listado. Sí se usa después de crear, editar o eliminar: la acción POST redirige al listado (patrón Post/Redirect/Get) y `TempData["Mensaje"]` sobrevive a esa redirección. `ViewData` se pierde en una redirección; `TempData` dura hasta que `_Layout.cshtml` lo lee y lo muestra.
