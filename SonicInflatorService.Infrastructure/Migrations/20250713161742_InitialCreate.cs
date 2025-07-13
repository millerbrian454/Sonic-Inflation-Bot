using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SonicInflatorService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Configurations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Key = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Section = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Configurations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DiscordConfigurations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Token = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    PrimaryChannelId = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                    GuildId = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                    MimicUserId = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                    SirenEmojiId = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                    SirenEmojiName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    InflatedImagePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DeflatedImagePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SonichuImagePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CurseYeHaMeHaImagePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RandomIntervalMinutesMaxValue = table.Column<int>(type: "int", nullable: false),
                    RandomIntervalMinutesMinValue = table.Column<int>(type: "int", nullable: false),
                    ResponseCooldownIntervalSeconds = table.Column<int>(type: "int", nullable: false),
                    RandomChannelPercentageChance = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscordConfigurations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OpenAIConfigurations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApiKey = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    BaseUri = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpenAIConfigurations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DiscordChannels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChannelId = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                    DiscordConfigurationId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscordChannels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DiscordChannels_DiscordConfigurations_DiscordConfigurationId",
                        column: x => x.DiscordConfigurationId,
                        principalTable: "DiscordConfigurations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DiscordContextChannels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChannelId = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                    DiscordConfigurationId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscordContextChannels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DiscordContextChannels_DiscordConfigurations_DiscordConfigurationId",
                        column: x => x.DiscordConfigurationId,
                        principalTable: "DiscordConfigurations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DiscordProfessionalWranglers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                    DiscordConfigurationId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscordProfessionalWranglers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DiscordProfessionalWranglers_DiscordConfigurations_DiscordConfigurationId",
                        column: x => x.DiscordConfigurationId,
                        principalTable: "DiscordConfigurations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OpenAIModels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ModelName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    OpenAIConfigurationId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpenAIModels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OpenAIModels_OpenAIConfigurations_OpenAIConfigurationId",
                        column: x => x.OpenAIConfigurationId,
                        principalTable: "OpenAIConfigurations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Configurations_Key_Section",
                table: "Configurations",
                columns: new[] { "Key", "Section" },
                unique: true,
                filter: "[Section] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_DiscordChannels_DiscordConfigurationId",
                table: "DiscordChannels",
                column: "DiscordConfigurationId");

            migrationBuilder.CreateIndex(
                name: "IX_DiscordContextChannels_DiscordConfigurationId",
                table: "DiscordContextChannels",
                column: "DiscordConfigurationId");

            migrationBuilder.CreateIndex(
                name: "IX_DiscordProfessionalWranglers_DiscordConfigurationId",
                table: "DiscordProfessionalWranglers",
                column: "DiscordConfigurationId");

            migrationBuilder.CreateIndex(
                name: "IX_OpenAIModels_OpenAIConfigurationId",
                table: "OpenAIModels",
                column: "OpenAIConfigurationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Configurations");

            migrationBuilder.DropTable(
                name: "DiscordChannels");

            migrationBuilder.DropTable(
                name: "DiscordContextChannels");

            migrationBuilder.DropTable(
                name: "DiscordProfessionalWranglers");

            migrationBuilder.DropTable(
                name: "OpenAIModels");

            migrationBuilder.DropTable(
                name: "DiscordConfigurations");

            migrationBuilder.DropTable(
                name: "OpenAIConfigurations");
        }
    }
}
