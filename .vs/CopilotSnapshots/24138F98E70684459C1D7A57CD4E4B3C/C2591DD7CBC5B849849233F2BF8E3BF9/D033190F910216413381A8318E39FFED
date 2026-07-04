using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ai_healthcare_monitoring_and_assistance_system.Migrations
{
    /// <inheritdoc />
    public partial class Final : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Check if table exists before attempting to alter it
            migrationBuilder.Sql(@"
                IF OBJECT_ID(N'[AnomalyHistory]', N'U') IS NOT NULL
                BEGIN
                    IF EXISTS(SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[AnomalyHistory]') AND name = 'MetricType')
                    BEGIN
                        IF EXISTS(SELECT 1 FROM sys.indexes WHERE name = 'IX_AnomalyHistory_DeviceId_MetricType' AND object_id = OBJECT_ID('[AnomalyHistory]'))
                        BEGIN
                            DROP INDEX [IX_AnomalyHistory_DeviceId_MetricType] ON [AnomalyHistory];
                        END

                        ALTER TABLE [AnomalyHistory] ALTER COLUMN [MetricType] nvarchar(450) NOT NULL;

                        CREATE INDEX [IX_AnomalyHistory_DeviceId_MetricType] ON [AnomalyHistory] ([DeviceId], [MetricType]);
                    END
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "MetricType",
                table: "AnomalyHistory",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
