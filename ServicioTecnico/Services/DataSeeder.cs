using Microsoft.EntityFrameworkCore;
using ServicioTecnico.Data;
using ServicioTecnico.Models;
using System;

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
                            NombreEstacion = "RTV San José Centro",
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
                            NombreEstacion = "RTV Cartago",
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
                            NombreEstacion = "RTV Alajuela",
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
                            NombreEstacion = "RTV Heredia",
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
                            NombreEstacion = "RTV Puntarenas",
                            Direccion = "Avenida 3, Calle 1-3, Puntarenas Centro",
                            Telefono = "2661-3333",
                            Email = "puntarenas@revtec.cr",
                            Provincia = "Puntarenas",
                            Canton = "Puntarenas",
                            Distrito = "Puntarenas",
                            HorarioAtencion = "Lunes a Viernes 7:00 AM - 4:00 PM",
                            Estado = "activa"
                        }
                    };

                    await context.Estaciones.AddRangeAsync(estaciones);
                    await context.SaveChangesAsync();
                    Console.WriteLine("✅ Estaciones creadas");
                }

                // Seed Usuario Técnico por defecto
                if (!await context.Usuarios.AnyAsync(u => u.TipoUsuario == "tecnico"))
                {
                    var tecnico = new Usuario
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
                    };

                    await context.Usuarios.AddAsync(tecnico);
                    await context.SaveChangesAsync();
                    Console.WriteLine("✅ Usuario técnico creado");
                }

                // Seed Usuario Cliente de prueba
                if (!await context.Usuarios.AnyAsync(u => u.TipoUsuario == "cliente"))
                {
                    var cliente = new Usuario
                    {
                        Nombre = "María",
                        Apellidos = "González López",
                        Cedula = "987654321",
                        Email = "cliente@test.cr",
                        Telefono = "8888-1234",
                        Direccion = "Cartago, Costa Rica",
                        TipoUsuario = "cliente",
                        Password = "cliente123",
                        Estado = "activo",
                        FechaRegistro = DateTime.Now
                    };

                    await context.Usuarios.AddAsync(cliente);
                    await context.SaveChangesAsync();
                    Console.WriteLine("✅ Usuario cliente creado");
                }

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
    }
}