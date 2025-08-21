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

                Console.WriteLine("🌱 Iniciando sembrado de datos...");
                await SeedUsuarios(context);
                await SeedEstaciones(context);
                await SeedVehiculos(context);
                await SeedCitas(context);

                Console.WriteLine("✅ Datos iniciales sembrados exitosamente");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al sembrar datos: {ex.Message}");
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
            Console.WriteLine($"📊 Usuarios existentes: {usuariosExistentes}");

            // CREAR TÉCNICOS
            var tecnicosExistentes = await context.Usuarios.CountAsync(u => u.TipoUsuario == "tecnico" && u.Estado == "activo");

            if (tecnicosExistentes == 0)
            {
                Console.WriteLine("👨‍🔧 Creando técnicos de prueba...");

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
                    new Usuario
                    {
                        Nombre = "Ana",
                        Apellidos = "López Vargas",
                        Cedula = "987654321",
                        Email = "ana.tecnico@revtec.cr",
                        Telefono = "8777-6666",
                        Direccion = "Cartago, Costa Rica",
                        TipoUsuario = "tecnico",
                        Password = "tecnico123",
                        Estado = "activo",
                        FechaRegistro = DateTime.Now
                    }
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
                            Console.WriteLine($"✅ Técnico creado: {tecnico.Email}");
                        }
                        else
                        {
                            Console.WriteLine($"ℹ️ Ya existe técnico: {tecnico.Email}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ Error al crear técnico {tecnico.Email}: {ex.Message}");
                    }
                }
            }

            // CREAR CLIENTES
            var clientesExistentes = await context.Usuarios.CountAsync(u => u.TipoUsuario == "cliente" && u.Estado == "activo");

            if (clientesExistentes == 0)
            {
                Console.WriteLine("👤 Creando clientes de prueba...");

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
                    },
                    new Usuario
                    {
                        Nombre = "José",
                        Apellidos = "Pérez Hernández",
                        Cedula = "444555666",
                        Email = "jose.cliente@test.cr",
                        Telefono = "8999-5555",
                        Direccion = "San José, Costa Rica",
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
                            Console.WriteLine($"✅ Cliente creado: {cliente.Email}");
                        }
                        else
                        {
                            Console.WriteLine($"ℹ️ Ya existe cliente: {cliente.Email}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ Error al crear cliente {cliente.Email}: {ex.Message}");
                    }
                }
            }

            var resumen = await context.Usuarios
                .Where(u => u.Estado == "activo")
                .GroupBy(u => u.TipoUsuario)
                .Select(g => new { TipoUsuario = g.Key, Cantidad = g.Count() })
                .ToListAsync();

            Console.WriteLine("📈 Resumen final:");
            foreach (var grupo in resumen)
            {
                Console.WriteLine($"   - {grupo.TipoUsuario}: {grupo.Cantidad}");
            }
        }

        private static async Task SeedEstaciones(AppDbContext context)
        {
            if (!await context.Estaciones.AnyAsync())
            {
                Console.WriteLine("🏢 Creando estaciones de servicio...");

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
                    },
                    new Estacion
                    {
                        NombreEstacion = "REVTEC Heredia",
                        Direccion = "Centro Comercial Plaza Heredia",
                        Telefono = "2260-7777",
                        Email = "heredia@revtec.cr",
                        Provincia = "Heredia",
                        Canton = "Heredia",
                        Distrito = "Central",
                        HorarioAtencion = "Lunes a Viernes 7:00 AM - 5:00 PM",
                        Estado = "activa"
                    }
                };

                await context.Estaciones.AddRangeAsync(estaciones);
                await context.SaveChangesAsync();
                Console.WriteLine($"✅ {estaciones.Count} estaciones creadas");
            }
        }

        private static async Task SeedVehiculos(AppDbContext context)
        {
            if (!await context.Vehiculos.AnyAsync())
            {
                Console.WriteLine("🚗 Creando vehículos de prueba...");

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
                        },
                        new Vehiculo
                        {
                            NumeroPlaca = "XYZ789",
                            IdPropietario = clientes[0].IdUsuario,
                            Marca = "Honda",
                            Modelo = "Civic",
                            Año = 2019,
                            NumeroChasis = "JHMFC1F39JX012345",
                            Color = "Azul",
                            TipoCombustible = "Gasolina",
                            Cilindrada = "1.5L",
                            FechaRegistro = DateTime.Now.AddDays(-45)
                        }
                    };

                    // Si hay más clientes, asignar vehículos adicionales
                    if (clientes.Count > 1)
                    {
                        vehiculos.Add(new Vehiculo
                        {
                            NumeroPlaca = "DEF456",
                            IdPropietario = clientes[1].IdUsuario,
                            Marca = "Nissan",
                            Modelo = "Sentra",
                            Año = 2021,
                            NumeroChasis = "3N1AB7AP0MY654321",
                            Color = "Gris",
                            TipoCombustible = "Gasolina",
                            Cilindrada = "1.6L",
                            FechaRegistro = DateTime.Now.AddDays(-20)
                        });
                    }

                    await context.Vehiculos.AddRangeAsync(vehiculos);
                    await context.SaveChangesAsync();
                    Console.WriteLine($"✅ {vehiculos.Count} vehículos creados");
                }
            }
        }

        private static async Task SeedCitas(AppDbContext context)
        {
            if (!await context.Citas.AnyAsync())
            {
                Console.WriteLine("📅 Creando citas de prueba...");

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
                            FechaCita = DateTime.Today.AddDays(1),
                            HoraCita = new TimeSpan(9, 0, 0),
                            EstadoCita = "programada",
                            Observaciones = "Primera inspección del vehículo",
                            FechaCreacion = DateTime.Now.AddDays(-1)
                        },
                        new Cita
                        {
                            IdVehiculo = vehiculos[0].IdVehiculo,
                            IdEstacion = estaciones[0].IdEstacion,
                            FechaCita = DateTime.Today.AddDays(-30),
                            HoraCita = new TimeSpan(14, 30, 0),
                            EstadoCita = "completada",
                            Observaciones = "Inspección completada exitosamente",
                            FechaCreacion = DateTime.Now.AddDays(-31)
                        }
                    };

                    // Agregar más citas si hay más vehículos
                    if (vehiculos.Count > 1)
                    {
                        citas.Add(new Cita
                        {
                            IdVehiculo = vehiculos[1].IdVehiculo,
                            IdEstacion = estaciones[0].IdEstacion,
                            FechaCita = DateTime.Today.AddDays(3),
                            HoraCita = new TimeSpan(11, 0, 0),
                            EstadoCita = "programada",
                            Observaciones = "Inspección de rutina",
                            FechaCreacion = DateTime.Now.AddDays(-2)
                        });
                    }

                    await context.Citas.AddRangeAsync(citas);
                    await context.SaveChangesAsync();
                    Console.WriteLine($"✅ {citas.Count} citas creadas");
                }
            }
        }
    }
}