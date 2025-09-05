using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mtu.Rentals.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:uuid-ossp", ",,");

            migrationBuilder.CreateTable(
                name: "couriers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    cnpj = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: false),
                    birth_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    cnh_number = table.Column<string>(type: "text", nullable: false),
                    cnh_type = table.Column<int>(type: "integer", nullable: false),
                    cnh_image_path = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_couriers", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "motorcycle_notifications",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    motorcycle_id = table.Column<Guid>(type: "uuid", nullable: false),
                    year = table.Column<int>(type: "integer", nullable: false),
                    received_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_motorcycle_notifications", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "motorcycles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    year = table.Column<int>(type: "integer", nullable: false),
                    model = table.Column<string>(type: "text", nullable: false),
                    plate = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    plate_normalized = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_motorcycles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rentals",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    motorcycle_id = table.Column<Guid>(type: "uuid", nullable: false),
                    courier_id = table.Column<Guid>(type: "uuid", nullable: false),
                    start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    expected_end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    plan = table.Column<int>(type: "integer", nullable: false),
                    daily_rate = table.Column<decimal>(type: "numeric", nullable: false),
                    return_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_rentals", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_couriers_cnh_number",
                table: "couriers",
                column: "cnh_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_couriers_cnpj",
                table: "couriers",
                column: "cnpj",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_motorcycle_notifications_motorcycle_id",
                table: "motorcycle_notifications",
                column: "motorcycle_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_motorcycles_plate_normalized",
                table: "motorcycles",
                column: "plate_normalized",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "couriers");

            migrationBuilder.DropTable(
                name: "motorcycle_notifications");

            migrationBuilder.DropTable(
                name: "motorcycles");

            migrationBuilder.DropTable(
                name: "rentals");
        }
    }
}
