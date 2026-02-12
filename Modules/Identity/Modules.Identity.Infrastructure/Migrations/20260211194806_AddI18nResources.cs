using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Modules.Identity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddI18nResources : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_I18nResources_Key_Namespace",
                table: "I18nResources");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "I18nResources");

            migrationBuilder.DropColumn(
                name: "Namespace",
                table: "I18nResources");

            migrationBuilder.DropColumn(
                name: "ValueEN",
                table: "I18nResources");

            migrationBuilder.RenameColumn(
                name: "ValueTR",
                table: "I18nResources",
                newName: "Value");

            migrationBuilder.AlterColumn<string>(
                name: "UpdatedBy",
                table: "I18nResources",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "I18nResources",
                type: "datetime2",
                nullable: true,
                defaultValueSql: "SYSUTCDATETIME()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Key",
                table: "I18nResources",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "DeletedBy",
                table: "I18nResources",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "I18nResources",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "I18nResources",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "I18nResources",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()",
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<string>(
                name: "Lang",
                table: "I18nResources",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "I18nResources",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "VersionNo",
                table: "I18nResources",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_I18nResources_Lang",
                table: "I18nResources",
                column: "Lang");

            migrationBuilder.CreateIndex(
                name: "IX_I18nResources_TenantId",
                table: "I18nResources",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_I18nResources_TenantId_Key_Lang",
                table: "I18nResources",
                columns: new[] { "TenantId", "Key", "Lang" },
                unique: true,
                filter: "[TenantId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_I18nResources_Lang",
                table: "I18nResources");

            migrationBuilder.DropIndex(
                name: "IX_I18nResources_TenantId",
                table: "I18nResources");

            migrationBuilder.DropIndex(
                name: "IX_I18nResources_TenantId_Key_Lang",
                table: "I18nResources");

            migrationBuilder.DropColumn(
                name: "Lang",
                table: "I18nResources");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "I18nResources");

            migrationBuilder.DropColumn(
                name: "VersionNo",
                table: "I18nResources");

            migrationBuilder.RenameColumn(
                name: "Value",
                table: "I18nResources",
                newName: "ValueTR");

            migrationBuilder.AlterColumn<string>(
                name: "UpdatedBy",
                table: "I18nResources",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "I18nResources",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldDefaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AlterColumn<string>(
                name: "Key",
                table: "I18nResources",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256);

            migrationBuilder.AlterColumn<string>(
                name: "DeletedBy",
                table: "I18nResources",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "I18nResources",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "I18nResources",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "I18nResources",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldDefaultValueSql: "NEWID()");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "I18nResources",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Namespace",
                table: "I18nResources",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ValueEN",
                table: "I18nResources",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_I18nResources_Key_Namespace",
                table: "I18nResources",
                columns: new[] { "Key", "Namespace" },
                unique: true);
        }
    }
}
