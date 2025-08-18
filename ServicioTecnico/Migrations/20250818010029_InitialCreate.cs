using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServicioTecnico.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Estaciones",
                columns: table => new
                {
                    IdEstacion = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NombreEstacion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Direccion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Telefono = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Provincia = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Canton = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Distrito = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    HorarioAtencion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, defaultValue: "activa")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Estaciones", x => x.IdEstacion);
                    table.CheckConstraint("CK_Estacion_Estado", "[Estado] IN ('activa', 'inactiva')");
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    IdUsuario = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Apellidos = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Cedula = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Telefono = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    Direccion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    TipoUsuario = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Password = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, defaultValue: "activo")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.IdUsuario);
                    table.CheckConstraint("CK_Usuario_Estado", "[Estado] IN ('activo', 'inactivo')");
                    table.CheckConstraint("CK_Usuario_TipoUsuario", "[TipoUsuario] IN ('cliente', 'tecnico')");
                });

            migrationBuilder.CreateTable(
                name: "Vehiculos",
                columns: table => new
                {
                    IdVehiculo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NumeroPlaca = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    IdPropietario = table.Column<int>(type: "int", nullable: false),
                    Marca = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Modelo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Año = table.Column<int>(type: "int", nullable: false),
                    NumeroChasis = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Color = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    TipoCombustible = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Cilindrada = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vehiculos", x => x.IdVehiculo);
                    table.ForeignKey(
                        name: "FK_Vehiculos_Usuarios_IdPropietario",
                        column: x => x.IdPropietario,
                        principalTable: "Usuarios",
                        principalColumn: "IdUsuario",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Citas",
                columns: table => new
                {
                    IdCita = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdVehiculo = table.Column<int>(type: "int", nullable: false),
                    IdEstacion = table.Column<int>(type: "int", nullable: false),
                    FechaCita = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HoraCita = table.Column<TimeSpan>(type: "time", nullable: false),
                    EstadoCita = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false, defaultValue: "programada"),
                    Observaciones = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Citas", x => x.IdCita);
                    table.CheckConstraint("CK_Cita_EstadoCita", "[EstadoCita] IN ('programada', 'completada', 'cancelada')");
                    table.ForeignKey(
                        name: "FK_Citas_Estaciones_IdEstacion",
                        column: x => x.IdEstacion,
                        principalTable: "Estaciones",
                        principalColumn: "IdEstacion",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Citas_Vehiculos_IdVehiculo",
                        column: x => x.IdVehiculo,
                        principalTable: "Vehiculos",
                        principalColumn: "IdVehiculo",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Inspecciones",
                columns: table => new
                {
                    IdInspeccion = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdCita = table.Column<int>(type: "int", nullable: false),
                    IdTecnico = table.Column<int>(type: "int", nullable: false),
                    FechaInspeccion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Resultado = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ObservacionesTecnicas = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    FechaVencimiento = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NumeroCertificado = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inspecciones", x => x.IdInspeccion);
                    table.CheckConstraint("CK_Inspeccion_Resultado", "[Resultado] IN ('aprobado', 'rechazado')");
                    table.ForeignKey(
                        name: "FK_Inspecciones_Citas_IdCita",
                        column: x => x.IdCita,
                        principalTable: "Citas",
                        principalColumn: "IdCita",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Inspecciones_Usuarios_IdTecnico",
                        column: x => x.IdTecnico,
                        principalTable: "Usuarios",
                        principalColumn: "IdUsuario",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Certificados",
                columns: table => new
                {
                    IdCertificado = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdInspeccion = table.Column<int>(type: "int", nullable: false),
                    NumeroCertificado = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FechaEmision = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    FechaVencimiento = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RutaArchivoDigital = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    EstadoCertificado = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, defaultValue: "valido")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Certificados", x => x.IdCertificado);
                    table.CheckConstraint("CK_Certificado_EstadoCertificado", "[EstadoCertificado] IN ('valido', 'vencido', 'anulado')");
                    table.ForeignKey(
                        name: "FK_Certificados_Inspecciones_IdInspeccion",
                        column: x => x.IdInspeccion,
                        principalTable: "Inspecciones",
                        principalColumn: "IdInspeccion",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DetalleInspecciones",
                columns: table => new
                {
                    IdDetalle = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdInspeccion = table.Column<int>(type: "int", nullable: false),
                    CategoriaRevision = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ResultadoItem = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ObservacionesItem = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetalleInspecciones", x => x.IdDetalle);
                    table.CheckConstraint("CK_DetalleInspeccion_ResultadoItem", "[ResultadoItem] IN ('OK', 'FALLO')");
                    table.ForeignKey(
                        name: "FK_DetalleInspecciones_Inspecciones_IdInspeccion",
                        column: x => x.IdInspeccion,
                        principalTable: "Inspecciones",
                        principalColumn: "IdInspeccion",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Certificados_IdInspeccion",
                table: "Certificados",
                column: "IdInspeccion");

            migrationBuilder.CreateIndex(
                name: "IX_Certificados_NumeroCertificado",
                table: "Certificados",
                column: "NumeroCertificado",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Citas_FechaCita_HoraCita",
                table: "Citas",
                columns: new[] { "FechaCita", "HoraCita" });

            migrationBuilder.CreateIndex(
                name: "IX_Citas_IdEstacion",
                table: "Citas",
                column: "IdEstacion");

            migrationBuilder.CreateIndex(
                name: "IX_Citas_IdVehiculo",
                table: "Citas",
                column: "IdVehiculo");

            migrationBuilder.CreateIndex(
                name: "IX_DetalleInspecciones_IdInspeccion",
                table: "DetalleInspecciones",
                column: "IdInspeccion");

            migrationBuilder.CreateIndex(
                name: "IX_Inspecciones_FechaInspeccion",
                table: "Inspecciones",
                column: "FechaInspeccion");

            migrationBuilder.CreateIndex(
                name: "IX_Inspecciones_IdCita",
                table: "Inspecciones",
                column: "IdCita");

            migrationBuilder.CreateIndex(
                name: "IX_Inspecciones_IdTecnico",
                table: "Inspecciones",
                column: "IdTecnico");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Cedula",
                table: "Usuarios",
                column: "Cedula",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Email",
                table: "Usuarios",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vehiculos_IdPropietario",
                table: "Vehiculos",
                column: "IdPropietario");

            migrationBuilder.CreateIndex(
                name: "IX_Vehiculos_NumeroPlaca",
                table: "Vehiculos",
                column: "NumeroPlaca",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Certificados");

            migrationBuilder.DropTable(
                name: "DetalleInspecciones");

            migrationBuilder.DropTable(
                name: "Inspecciones");

            migrationBuilder.DropTable(
                name: "Citas");

            migrationBuilder.DropTable(
                name: "Estaciones");

            migrationBuilder.DropTable(
                name: "Vehiculos");

            migrationBuilder.DropTable(
                name: "Usuarios");
        }
    }
}
