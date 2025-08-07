using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CusomMapOSM_Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDataDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "account_statuses",
                keyColumn: "status_id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000007"),
                column: "name",
                value: "PendingVerification");

            migrationBuilder.UpdateData(
                table: "annotation_types",
                keyColumn: "type_id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000013"),
                column: "type_name",
                value: "TextLabel");

            migrationBuilder.UpdateData(
                table: "layer_sources",
                keyColumn: "source_type_id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000026"),
                column: "name",
                value: "UserUploaded");

            migrationBuilder.UpdateData(
                table: "layer_sources",
                keyColumn: "source_type_id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000027"),
                column: "name",
                value: "ExternalAPI");

            migrationBuilder.UpdateData(
                table: "layer_sources",
                keyColumn: "source_type_id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000029"),
                column: "name",
                value: "WebMapService");

            migrationBuilder.UpdateData(
                table: "layer_types",
                keyColumn: "layer_type_id",
                keyValue: 3,
                column: "type_name",
                value: "POI");

            migrationBuilder.UpdateData(
                table: "layer_types",
                keyColumn: "layer_type_id",
                keyValue: 4,
                column: "type_name",
                value: "GEOJSON");

            migrationBuilder.UpdateData(
                table: "membership_statuses",
                keyColumn: "status_id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000033"),
                column: "name",
                value: "PendingPayment");

            migrationBuilder.UpdateData(
                table: "organization_location_statuses",
                keyColumn: "status_id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000037"),
                column: "name",
                value: "UnderConstruction");

            migrationBuilder.UpdateData(
                table: "organization_location_statuses",
                keyColumn: "status_id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000038"),
                column: "name",
                value: "TemporaryClosed");

            migrationBuilder.UpdateData(
                table: "payment_gateways",
                keyColumn: "gateway_id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000051"),
                column: "name",
                value: "BankTransfer");

            migrationBuilder.UpdateData(
                table: "ticket_statuses",
                keyColumn: "status_id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000044"),
                column: "name",
                value: "InProgress");

            migrationBuilder.UpdateData(
                table: "ticket_statuses",
                keyColumn: "status_id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000045"),
                column: "name",
                value: "WaitingForCustomer");

            migrationBuilder.UpdateData(
                table: "user_roles",
                keyColumn: "role_id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000002"),
                column: "name",
                value: "RegisteredUser");

            migrationBuilder.UpdateData(
                table: "user_roles",
                keyColumn: "role_id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000003"),
                column: "name",
                value: "Admin");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "account_statuses",
                keyColumn: "status_id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000007"),
                column: "name",
                value: "Pending Verification");

            migrationBuilder.UpdateData(
                table: "annotation_types",
                keyColumn: "type_id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000013"),
                column: "type_name",
                value: "Text Label");

            migrationBuilder.UpdateData(
                table: "layer_sources",
                keyColumn: "source_type_id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000026"),
                column: "name",
                value: "User Upload");

            migrationBuilder.UpdateData(
                table: "layer_sources",
                keyColumn: "source_type_id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000027"),
                column: "name",
                value: "External API");

            migrationBuilder.UpdateData(
                table: "layer_sources",
                keyColumn: "source_type_id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000029"),
                column: "name",
                value: "Web Service");

            migrationBuilder.UpdateData(
                table: "layer_types",
                keyColumn: "layer_type_id",
                keyValue: 3,
                column: "type_name",
                value: "POIs");

            migrationBuilder.UpdateData(
                table: "layer_types",
                keyColumn: "layer_type_id",
                keyValue: 4,
                column: "type_name",
                value: "GeoJSON");

            migrationBuilder.UpdateData(
                table: "membership_statuses",
                keyColumn: "status_id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000033"),
                column: "name",
                value: "Pending Payment");

            migrationBuilder.UpdateData(
                table: "organization_location_statuses",
                keyColumn: "status_id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000037"),
                column: "name",
                value: "Under Construction");

            migrationBuilder.UpdateData(
                table: "organization_location_statuses",
                keyColumn: "status_id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000038"),
                column: "name",
                value: "Temporary Closed");

            migrationBuilder.UpdateData(
                table: "payment_gateways",
                keyColumn: "gateway_id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000051"),
                column: "name",
                value: "Bank Transfer");

            migrationBuilder.UpdateData(
                table: "ticket_statuses",
                keyColumn: "status_id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000044"),
                column: "name",
                value: "In Progress");

            migrationBuilder.UpdateData(
                table: "ticket_statuses",
                keyColumn: "status_id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000045"),
                column: "name",
                value: "Waiting for Customer");

            migrationBuilder.UpdateData(
                table: "user_roles",
                keyColumn: "role_id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000002"),
                column: "name",
                value: "Registered User");

            migrationBuilder.UpdateData(
                table: "user_roles",
                keyColumn: "role_id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000003"),
                column: "name",
                value: "Administrator");
        }
    }
}
