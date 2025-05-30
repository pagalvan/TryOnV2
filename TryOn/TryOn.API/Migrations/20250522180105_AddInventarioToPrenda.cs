using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TryOn.API.Migrations
{
    /// <inheritdoc />
    public partial class AddInventarioToPrenda : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CategoriaDTO",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoriaDTO", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "categorias",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    descripcion = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("categorias_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "usuarios",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    apellido = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    password = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    telefono = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    direccion = table.Column<string>(type: "text", nullable: true),
                    es_admin = table.Column<bool>(type: "boolean", nullable: true, defaultValue: false),
                    fecha_registro = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("usuarios_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "prendas",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    codigo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    descripcion = table.Column<string>(type: "text", nullable: true),
                    precio_venta = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    costo = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    categoria_id = table.Column<int>(type: "integer", nullable: true),
                    imagen_url = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    fecha_creacion = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("prendas_pkey", x => x.id);
                    table.ForeignKey(
                        name: "prendas_categoria_id_fkey",
                        column: x => x.categoria_id,
                        principalTable: "categorias",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "pedidos",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    usuario_id = table.Column<int>(type: "integer", nullable: true),
                    fecha_pedido = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    total = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    direccion_envio = table.Column<string>(type: "text", nullable: true),
                    metodo_pago = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pedidos_pkey", x => x.id);
                    table.ForeignKey(
                        name: "pedidos_usuario_id_fkey",
                        column: x => x.usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "preferencias_usuario",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    usuario_id = table.Column<int>(type: "integer", nullable: true),
                    categoria_id = table.Column<int>(type: "integer", nullable: true),
                    talla_preferida = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    color_preferido = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("preferencias_usuario_pkey", x => x.id);
                    table.ForeignKey(
                        name: "preferencias_usuario_categoria_id_fkey",
                        column: x => x.categoria_id,
                        principalTable: "categorias",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "preferencias_usuario_usuario_id_fkey",
                        column: x => x.usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "inventario",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    prenda_id = table.Column<int>(type: "integer", nullable: true),
                    talla = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    color = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    cantidad = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ubicacion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("inventario_pkey", x => x.id);
                    table.ForeignKey(
                        name: "inventario_prenda_id_fkey",
                        column: x => x.prenda_id,
                        principalTable: "prendas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "promociones",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    titulo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    descripcion = table.Column<string>(type: "text", nullable: true),
                    porcentaje_descuento = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    fecha_inicio = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    fecha_fin = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    prenda_id = table.Column<int>(type: "integer", nullable: true),
                    categoria_id = table.Column<int>(type: "integer", nullable: true),
                    codigo_promocion = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    activa = table.Column<bool>(type: "boolean", nullable: true, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("promociones_pkey", x => x.id);
                    table.ForeignKey(
                        name: "promociones_categoria_id_fkey",
                        column: x => x.categoria_id,
                        principalTable: "categorias",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "promociones_prenda_id_fkey",
                        column: x => x.prenda_id,
                        principalTable: "prendas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ventas",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    pedido_id = table.Column<int>(type: "integer", nullable: true),
                    fecha_venta = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    total = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    metodo_pago = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    usuario_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("ventas_pkey", x => x.id);
                    table.ForeignKey(
                        name: "ventas_pedido_id_fkey",
                        column: x => x.pedido_id,
                        principalTable: "pedidos",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "ventas_usuario_id_fkey",
                        column: x => x.usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "detalles_pedido",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    pedido_id = table.Column<int>(type: "integer", nullable: true),
                    inventario_id = table.Column<int>(type: "integer", nullable: true),
                    cantidad = table.Column<int>(type: "integer", nullable: false),
                    precio_unitario = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    subtotal = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("detalles_pedido_pkey", x => x.id);
                    table.ForeignKey(
                        name: "detalles_pedido_inventario_id_fkey",
                        column: x => x.inventario_id,
                        principalTable: "inventario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "detalles_pedido_pedido_id_fkey",
                        column: x => x.pedido_id,
                        principalTable: "pedidos",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "idx_detalles_inventario",
                table: "detalles_pedido",
                column: "inventario_id");

            migrationBuilder.CreateIndex(
                name: "idx_detalles_pedido",
                table: "detalles_pedido",
                column: "pedido_id");

            migrationBuilder.CreateIndex(
                name: "idx_inventario_bajo_stock",
                table: "inventario",
                column: "cantidad",
                filter: "(cantidad <= 5)");

            migrationBuilder.CreateIndex(
                name: "idx_inventario_prenda",
                table: "inventario",
                column: "prenda_id");

            migrationBuilder.CreateIndex(
                name: "idx_inventario_talla_color",
                table: "inventario",
                columns: new[] { "prenda_id", "talla", "color" });

            migrationBuilder.CreateIndex(
                name: "inventario_prenda_id_talla_color_key",
                table: "inventario",
                columns: new[] { "prenda_id", "talla", "color" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_pedidos_estado",
                table: "pedidos",
                column: "estado");

            migrationBuilder.CreateIndex(
                name: "idx_pedidos_fecha",
                table: "pedidos",
                column: "fecha_pedido");

            migrationBuilder.CreateIndex(
                name: "idx_pedidos_usuario",
                table: "pedidos",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "idx_preferencias_usuario",
                table: "preferencias_usuario",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "IX_preferencias_usuario_categoria_id",
                table: "preferencias_usuario",
                column: "categoria_id");

            migrationBuilder.CreateIndex(
                name: "idx_prendas_categoria",
                table: "prendas",
                column: "categoria_id");

            migrationBuilder.CreateIndex(
                name: "idx_prendas_codigo",
                table: "prendas",
                column: "codigo");

            migrationBuilder.CreateIndex(
                name: "prendas_codigo_key",
                table: "prendas",
                column: "codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_promociones_activa",
                table: "promociones",
                column: "activa",
                filter: "(activa = true)");

            migrationBuilder.CreateIndex(
                name: "idx_promociones_codigo",
                table: "promociones",
                column: "codigo_promocion");

            migrationBuilder.CreateIndex(
                name: "idx_promociones_fecha",
                table: "promociones",
                columns: new[] { "fecha_inicio", "fecha_fin" });

            migrationBuilder.CreateIndex(
                name: "IX_promociones_categoria_id",
                table: "promociones",
                column: "categoria_id");

            migrationBuilder.CreateIndex(
                name: "IX_promociones_prenda_id",
                table: "promociones",
                column: "prenda_id");

            migrationBuilder.CreateIndex(
                name: "idx_usuarios_email",
                table: "usuarios",
                column: "email");

            migrationBuilder.CreateIndex(
                name: "usuarios_email_key",
                table: "usuarios",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_ventas_fecha",
                table: "ventas",
                column: "fecha_venta");

            migrationBuilder.CreateIndex(
                name: "idx_ventas_pedido",
                table: "ventas",
                column: "pedido_id");

            migrationBuilder.CreateIndex(
                name: "idx_ventas_usuario",
                table: "ventas",
                column: "usuario_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CategoriaDTO");

            migrationBuilder.DropTable(
                name: "detalles_pedido");

            migrationBuilder.DropTable(
                name: "preferencias_usuario");

            migrationBuilder.DropTable(
                name: "promociones");

            migrationBuilder.DropTable(
                name: "ventas");

            migrationBuilder.DropTable(
                name: "inventario");

            migrationBuilder.DropTable(
                name: "pedidos");

            migrationBuilder.DropTable(
                name: "prendas");

            migrationBuilder.DropTable(
                name: "usuarios");

            migrationBuilder.DropTable(
                name: "categorias");
        }
    }
}
