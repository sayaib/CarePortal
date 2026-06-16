using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarePortal.Infrastructure.Persistence.Migrations;

public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "invoices",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                reference = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_invoices", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "patients",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                date_of_birth = table.Column<DateOnly>(type: "date", nullable: false),
                created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_patients", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "invoice_line_items",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                invoice_id = table.Column<Guid>(type: "uuid", nullable: false),
                description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                due_date = table.Column<DateOnly>(type: "date", nullable: false),
                amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_invoice_line_items", x => x.id);
                table.ForeignKey(
                    name: "fk_invoice_line_items_invoices_invoice_id",
                    column: x => x.invoice_id,
                    principalTable: "invoices",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "ledger_entries",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                invoice_id = table.Column<Guid>(type: "uuid", nullable: false),
                line_item_id = table.Column<Guid>(type: "uuid", nullable: true),
                type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_ledger_entries", x => x.id);
                table.ForeignKey(
                    name: "fk_ledger_entries_invoice_line_items_line_item_id",
                    column: x => x.line_item_id,
                    principalTable: "invoice_line_items",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_ledger_entries_invoices_invoice_id",
                    column: x => x.invoice_id,
                    principalTable: "invoices",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "ix_invoice_line_items_invoice_id",
            table: "invoice_line_items",
            column: "invoice_id");

        migrationBuilder.CreateIndex(
            name: "ix_invoices_reference",
            table: "invoices",
            column: "reference",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_ledger_entries_invoice_id",
            table: "ledger_entries",
            column: "invoice_id");

        migrationBuilder.CreateIndex(
            name: "ix_ledger_entries_line_item_id",
            table: "ledger_entries",
            column: "line_item_id");

        migrationBuilder.CreateIndex(
            name: "ix_patients_email",
            table: "patients",
            column: "email",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "ledger_entries");
        migrationBuilder.DropTable(name: "patients");
        migrationBuilder.DropTable(name: "invoice_line_items");
        migrationBuilder.DropTable(name: "invoices");
    }
}
