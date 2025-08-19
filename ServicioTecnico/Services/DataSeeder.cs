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

                // Seed Estaciones
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
                        },
                        new Estacion
                        {
                            NombreEstacion = "REVTEC Alajuela",
                            Direccion = "200m Sur del Aeropuerto Juan Santamaría",
                            Telefono = "2430-7777",
                            Email = "alajuela@revtec.cr",
                            Provincia = "Alajuela",
                            Canton = "Alajuela",
                            Distrito = "San José",
                            HorarioAtencion = "Lunes a Sábado 7:00 AM - 5:00 PM",
                            Estado = "activa"
                        },
                        new Estacion
                        {
                            NombreEstacion = "REVTEC Heredia",
                            Direccion = "Costado Norte del Mercado Central, Heredia",
                            Telefono = "2260-9999",
                            Email = "heredia@revtec.cr",
                            Provincia = "Heredia",
                            Canton = "Heredia",
                            Distrito = "Heredia",
                            HorarioAtencion = "Lunes a Viernes 7:30 AM - 4:30 PM",
                            Estado = "activa"
                        },
                        new Estacion
                        {
                            NombreEstacion = "REVTEC Puntarenas",
                            Direccion = "Avenida 3, Calle 1-3, Puntarenas Centro",
                            Telefono = "2661-3333",
                            Email = "puntarenas@revtec.cr",
                            Provincia = "Puntarenas",
                            Canton = "Puntarenas",
                            Distrito = "Puntarenas",
                            HorarioAtencion = "Lunes a Viernes 7:00 AM - 4:00 PM",
                            Estado = "activa"
                        },
                        new Estacion
                        {
                            NombreEstacion = "REVTEC Limón",
                            Direccion = "Av. 2 entre calles 3 y 4, Puerto Limón",
                            Telefono = "2758-4444",
                            Email = "limon@revtec.cr",
                            Provincia = "Limón",
                            Canton = "Limón",
                            Distrito = "Limón",
                            HorarioAtencion = "Lunes a Viernes 7:00 AM - 4:00 PM",
                            Estado = "activa"
                        },
                        new Estacion
                        {
                            NombreEstacion = "REVTEC Guanacaste",
                            Direccion = "100m Norte del Hospital de Liberia",
                            Telefono = "2666-2222",
                            Email = "guanacaste@revtec.cr",
                            Provincia = "Guanacaste",
                            Canton = "Liberia",
                            Distrito = "Liberia",
                            HorarioAtencion = "Lunes a Viernes 7:00 AM - 4:30 PM",
                            Estado = "activa"
                        }
                    };

                    await context.Estaciones.AddRangeAsync(estaciones);
                    await context.SaveChangesAsync();
                    Console.WriteLine("✅ Estaciones creadas exitosamente");
                }

                // Seed Usuarios por defecto
                await SeedUsuarios(context);

                // Seed Vehículos de prueba (solo si hay usuarios)
                await SeedVehiculos(context);

                // Seed Citas de prueba
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
            // Seed Usuario Técnico por defecto
            if (!await context.Usuarios.AnyAsync(u => u.TipoUsuario == "tecnico"))
            {
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

                await context.Usuarios.AddRangeAsync(tecnicos);
                await context.SaveChangesAsync();
                Console.WriteLine("✅ Usuarios técnicos creados");
            }

            // Seed Usuarios Cliente de prueba - DATOS CONSISTENTES
            if (!await context.Usuarios.AnyAsync(u => u.TipoUsuario == "cliente"))
            {
                var clientes = new List<Usuario>
                {
                    new Usuario
                    {
                        Nombre = "María",
                        Apellidos = "González López",
                        Cedula = "987654321", // CEDULA CONSISTENTE CON TUS DATOS
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
                        Nombre = "Juan",
                        Apellidos = "Pérez Mora",
                        Cedula = "444555666",
                        Email = "juan.perez@email.cr",
                        Telefono = "8999-5555",
                        Direccion = "Alajuela, Costa Rica",
                        TipoUsuario = "cliente",
                        Password = "cliente123",
                        Estado = "activo",
                        FechaRegistro = DateTime.Now
                    },
                    new Usuario
                    {
                        Nombre = "Laura",
                        Apellidos = "Jiménez Castro",
                        Cedula = "777888999",
                        Email = "laura.jimenez@email.cr",
                        Telefono = "8444-3333",
                        Direccion = "Heredia, Costa Rica",
                        TipoUsuario = "cliente",
                        Password = "cliente123",
                        Estado = "activo",
                        FechaRegistro = DateTime.Now
                    }
                };

                await context.Usuarios.AddRangeAsync(clientes);
                await context.SaveChangesAsync();
                Console.WriteLine("✅ Usuarios clientes creados");
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
                        },
                        new Vehiculo
                        {
                            NumeroPlaca = "XYZ789",
                            IdPropietario = clientes.Count > 1 ? clientes[1].IdUsuario : clientes[0].IdUsuario,
                            Marca = "Nissan",
                            Modelo = "Sentra",
                            Año = 2019,
                            NumeroChasis = "3N1AB7AP5KY123456",
                            Color = "Azul",
                            TipoCombustible = "Gasolina",
                            Cilindrada = "1.6L",
                            FechaRegistro = DateTime.Now.AddDays(-25)
                        },
                        new Vehiculo
                        {
                            NumeroPlaca = "DEF456",
                            IdPropietario = clientes.Count > 2 ? clientes[2].IdUsuario : clientes[0].IdUsuario,
                            Marca = "Honda",
                            Modelo = "Civic",
                            Año = 2021,
                            NumeroChasis = "2HGFC2F59MH123456",
                            Color = "Negro",
                            TipoCombustible = "Gasolina",
                            Cilindrada = "2.0L",
                            FechaRegistro = DateTime.Now.AddDays(-20)
                        }
                    };

                    await context.Vehiculos.AddRangeAsync(vehiculos);
                    await context.SaveChangesAsync();
                    Console.WriteLine("✅ Vehículos de prueba creados");
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
                            FechaCita = DateTime.Today.AddDays(2),
                            HoraCita = new TimeSpan(9, 0, 0),
                            EstadoCita = "programada",
                            Observaciones = "Primera inspección del vehículo",
                            FechaCreacion = DateTime.Now.AddDays(-1)
                        },
                        new Cita
                        {
                            IdVehiculo = vehiculos.Count > 1 ? vehiculos[1].IdVehiculo : vehiculos[0].IdVehiculo,
                            IdEstacion = estaciones.Count > 1 ? estaciones[1].IdEstacion : estaciones[0].IdEstacion,
                            FechaCita = DateTime.Today.AddDays(3),
                            HoraCita = new TimeSpan(14, 30, 0),
                            EstadoCita = "programada",
                            Observaciones = "Revisión periódica",
                            FechaCreacion = DateTime.Now.AddHours(-12)
                        },
                        new Cita
                        {
                            IdVehiculo = vehiculos.Count > 2 ? vehiculos[2].IdVehiculo : vehiculos[0].IdVehiculo,
                            IdEstacion = estaciones[0].IdEstacion,
                            FechaCita = DateTime.Today.AddDays(-5),
                            HoraCita = new TimeSpan(10, 0, 0),
                            EstadoCita = "completada",
                            Observaciones = "Inspección completada satisfactoriamente",
                            FechaCreacion = DateTime.Now.AddDays(-6)
                        }
                    };

                    await context.Citas.AddRangeAsync(citas);
                    await context.SaveChangesAsync();
                    Console.WriteLine("✅ Citas de prueba creadas");

                    // Crear una inspección de ejemplo para la cita completada
                    await SeedInspeccionEjemplo(context, citas[2].IdCita);
                }
            }
        }

        private static async Task SeedInspeccionEjemplo(AppDbContext context, int idCita)
        {
            var tecnico = await context.Usuarios.FirstOrDefaultAsync(u => u.TipoUsuario == "tecnico");

            if (tecnico != null)
            {
                var inspeccion = new Inspeccion
                {
                    IdCita = idCita,
                    IdTecnico = tecnico.IdUsuario,
                    FechaInspeccion = DateTime.Now.AddDays(-5),
                    Resultado = "aprobado",
                    ObservacionesTecnicas = "Vehículo en excelentes condiciones. Todos los sistemas funcionando correctamente.",
                    FechaVencimiento = DateTime.Now.AddYears(1).AddDays(-5),
                    NumeroCertificado = $"CERT-{DateTime.Now:yyyyMMdd}-001"
                };

                context.Inspecciones.Add(inspeccion);
                await context.SaveChangesAsync();

                // Crear detalles de inspección
                var detalles = new List<DetalleInspeccion>
                {
                    new DetalleInspeccion
                    {
                        IdInspeccion = inspeccion.IdInspeccion,
                        CategoriaRevision = "Sistema de frenos",
                        ResultadoItem = "OK",
                        ObservacionesItem = "Frenos en perfectas condiciones"
                    },
                    new DetalleInspeccion
                    {
                        IdInspeccion = inspeccion.IdInspeccion,
                        CategoriaRevision = "Sistema de dirección",
                        ResultadoItem = "OK",
                        ObservacionesItem = "Dirección estable y precisa"
                    },
                    new DetalleInspeccion
                    {
                        IdInspeccion = inspeccion.IdInspeccion,
                        CategoriaRevision = "Neumáticos y llantas",
                        ResultadoItem = "OK",
                        ObservacionesItem = "Neumáticos con buen estado y presión adecuada"
                    },
                    new DetalleInspeccion
                    {
                        IdInspeccion = inspeccion.IdInspeccion,
                        CategoriaRevision = "Sistema de iluminación",
                        ResultadoItem = "OK",
                        ObservacionesItem = "Todas las luces funcionando correctamente"
                    }
                };

                context.DetalleInspecciones.AddRange(detalles);
                await context.SaveChangesAsync();

                // Crear certificado
                var certificado = new Certificado
                {
                    IdInspeccion = inspeccion.IdInspeccion,
                    NumeroCertificado = inspeccion.NumeroCertificado,
                    FechaEmision = inspeccion.FechaInspeccion,
                    FechaVencimiento = inspeccion.FechaVencimiento.Value,
                    RutaArchivoDigital = $"/certificados/{inspeccion.NumeroCertificado}.pdf",
                    EstadoCertificado = "valido"
                };

                context.Certificados.Add(certificado);
                await context.SaveChangesAsync();

                Console.WriteLine("✅ Inspección de ejemplo creada con certificado");
            }
        }
    }
}