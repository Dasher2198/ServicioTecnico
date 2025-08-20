using Microsoft.EntityFrameworkCore;
using ServicioTecnico.Data;
using ServicioTecnico.Models;

namespace ServicioTecnico.Services
{
    public static class DataSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            try
            {
                await context.Database.EnsureCreatedAsync();

                Console.WriteLine(" Iniciando sembrado de datos...");
                await SeedUsuarios(context);
                await SeedEstaciones(context);
                await SeedVehiculos(context);
                await SeedCitas(context);

                Console.WriteLine(" Datos iniciales sembrados exitosamente");
            }
            catch (Exception ex)
            {
                Console.WriteLine($" Error al sembrar datos: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Detalle: {ex.InnerException.Message}");
                }
                throw;
            }
        }

        private static async Task SeedUsuarios(AppDbContext context)
        {
            var usuariosExistentes = await context.Usuarios.CountAsync();
            Console.WriteLine($" Usuarios existentes: {usuariosExistentes}");

            // FORZAR CREACIÓN DE TÉCNICOS
            var tecnicosExistentes = await context.Usuarios.CountAsync(u => u.TipoUsuario == "tecnico" && u.Estado == "activo");

            if (tecnicosExistentes == 0)
            {
                Console.WriteLine(" Creando técnicos de prueba");

                var tecnicos = new List<Usuario>
                {
                    new Usuario
                    {
                        Nombre = "Carlos",
                        Apellidos = "Rodríguez Méndez",
                        Cedula = "123456789",
                        Email = "tecnico@revtec.cr",
                        Telefono = "8888-7777",
                        Direccion = "San José, Costa Rica",
                        TipoUsuario = "tecnico",
                        Password = "tecnico123",
                        Estado = "activo",
                        FechaRegistro = DateTime.Now
                    },
                    
                };

                foreach (var tecnico in tecnicos)
                {
                    try
                    {
                        var existe = await context.Usuarios.AnyAsync(u => u.Email == tecnico.Email);
                        if (!existe)
                        {
                            context.Usuarios.Add(tecnico);
                            await context.SaveChangesAsync();
                            Console.WriteLine($" Técnico creado: {tecnico.Email}");
                        }
                        else
                        {
                            Console.WriteLine($" Ya existe técnico: {tecnico.Email}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($" Error al crear técnico {tecnico.Email}: {ex.Message}");
                    }
                }

                var tecnicosCreados = await context.Usuarios
                    .Where(u => u.TipoUsuario == "tecnico" && u.Estado == "activo")
                    .ToListAsync();

                Console.WriteLine(" Técnicos verificados:");
                foreach (var tecnico in tecnicosCreados)
                {
                    Console.WriteLine($"   - {tecnico.Email} | {tecnico.Nombre} | {tecnico.Password}");
                }
            }

            // CREAR CLIENTES
            var clientesExistentes = await context.Usuarios.CountAsync(u => u.TipoUsuario == "cliente" && u.Estado == "activo");

            if (clientesExistentes == 0)
            {
                var clientes = new List<Usuario>
                {
                    new Usuario
                    {
                        Nombre = "María",
                        Apellidos = "González López",
                        Cedula = "555666777",
                        Email = "cliente@test.cr",
                        Telefono = "8888-1234",
                        Direccion = "Cartago, Costa Rica",
                        TipoUsuario = "cliente",
                        Password = "cliente123",
                        Estado = "activo",
                        FechaRegistro = DateTime.Now
                    }
                };

                foreach (var cliente in clientes)
                {
                    try
                    {
                        var existe = await context.Usuarios.AnyAsync(u => u.Email == cliente.Email);
                        if (!existe)
                        {
                            context.Usuarios.Add(cliente);
                            await context.SaveChangesAsync();
                            Console.WriteLine($" Cliente creado: {cliente.Email}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($" Error al crear cliente: {ex.Message}");
                    }
                }
            }

            var resumen = await context.Usuarios
                .Where(u => u.Estado == "activo")
                .GroupBy(u => u.TipoUsuario)
                .Select(g => new { TipoUsuario = g.Key, Cantidad = g.Count() })
                .ToListAsync();

            Console.WriteLine(" Resumen final:");
            foreach (var grupo in resumen)
            {
                Console.WriteLine($"   - {grupo.TipoUsuario}: {grupo.Cantidad}");
            }
        }

        private static async Task SeedEstaciones(AppDbContext context)
        {
            if (!await context.Estaciones.AnyAsync())
            {
                var estaciones = new List<Estacion>
                {
                    new Estacion
                    {
                        NombreEstacion = "REVTEC San José Centro",
                        Direccion = "Avenida Segunda, Calle 14, San José",
                        Telefono = "2221-5555",
                        Email = "sanjose@revtec.cr",
                        Provincia = "San José",
                        Canton = "San José",
                        Distrito = "Carmen",
                        HorarioAtencion = "Lunes a Viernes 7:00 AM - 5:00 PM",
                        Estado = "activa"
                    },
                    new Estacion
                    {
                        NombreEstacion = "REVTEC Cartago",
                        Direccion = "Frente al Hospital Max Peralta, Cartago",
                        Telefono = "2550-8888",
                        Email = "cartago@revtec.cr",
                        Provincia = "Cartago",
                        Canton = "Cartago",
                        Distrito = "Oriental",
                        HorarioAtencion = "Lunes a Viernes 7:00 AM - 4:00 PM",
                        Estado = "activa"
                    }
                };

                await context.Estaciones.AddRangeAsync(estaciones);
                await context.SaveChangesAsync();
                Console.WriteLine($" {estaciones.Count} estaciones creadas");
            }
        }

        private static async Task SeedVehiculos(AppDbContext context)
        {
            if (!await context.Vehiculos.AnyAsync())
            {
                var clientes = await context.Usuarios.Where(u => u.TipoUsuario == "cliente").ToListAsync();

                if (clientes.Any())
                {
                    var vehiculos = new List<Vehiculo>
                    {
                        new Vehiculo
                        {
                            NumeroPlaca = "ABC123",
                            IdPropietario = clientes[0].IdUsuario,
                            Marca = "Toyota",
                            Modelo = "Corolla",
                            Año = 2020,
                            NumeroChasis = "JTDBE32K123456789",
                            Color = "Blanco",
                            TipoCombustible = "Gasolina",
                            Cilindrada = "1.8L",
                            FechaRegistro = DateTime.Now.AddDays(-30)
                        }
                    };

                    await context.Vehiculos.AddRangeAsync(vehiculos);
                    await context.SaveChangesAsync();
                    Console.WriteLine($" {vehiculos.Count} vehículos creados");
                }
            }
        }

        private static async Task SeedCitas(AppDbContext context)
        {
            if (!await context.Citas.AnyAsync())
            {
                var vehiculos = await context.Vehiculos.ToListAsync();
                var estaciones = await context.Estaciones.ToListAsync();

                if (vehiculos.Any() && estaciones.Any())
                {
                    var citas = new List<Cita>
                    {
                        new Cita
                        {
                            IdVehiculo = vehiculos[0].IdVehiculo,
                            IdEstacion = estaciones[0].IdEstacion,
                            FechaCita = DateTime.Today,
                            HoraCita = new TimeSpan(9, 0, 0),
                            EstadoCita = "programada",
                            Observaciones = "Cita de prueba para hoy",
                            FechaCreacion = DateTime.Now.AddDays(-1)
                        }
                    };

                    await context.Citas.AddRangeAsync(citas);
                    await context.SaveChangesAsync();
                    Console.WriteLine($" {citas.Count} citas creadas");
                }
            }
        }
    }
}