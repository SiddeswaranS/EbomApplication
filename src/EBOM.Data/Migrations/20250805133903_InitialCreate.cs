using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EBOM.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EntityDataType",
                columns: table => new
                {
                    DataTypeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DataTypeName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DataTypeDescription = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    DataTypeFormat = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntityDataType", x => x.DataTypeID);
                });

            migrationBuilder.CreateTable(
                name: "UserMaster",
                columns: table => new
                {
                    UserID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UserEmail = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserMaster", x => x.UserID);
                });

            migrationBuilder.CreateTable(
                name: "Entity",
                columns: table => new
                {
                    EntityID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntityName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EntityDisplayName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EntityDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    EntityType = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    DataTypeID = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Entity", x => x.EntityID);
                    table.ForeignKey(
                        name: "FK_Entity_EntityDataType_DataTypeID",
                        column: x => x.DataTypeID,
                        principalTable: "EntityDataType",
                        principalColumn: "DataTypeID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EntityTemplateRevision",
                columns: table => new
                {
                    TemplateRevisionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntityID = table.Column<int>(type: "int", nullable: false),
                    TemplateRevisionNumber = table.Column<int>(type: "int", nullable: false),
                    TemplateRevisionDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntityTemplateRevision", x => x.TemplateRevisionID);
                    table.ForeignKey(
                        name: "FK_EntityTemplateRevision_Entity_EntityID",
                        column: x => x.EntityID,
                        principalTable: "Entity",
                        principalColumn: "EntityID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EntityValue",
                columns: table => new
                {
                    EntityValueId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntityId = table.Column<int>(type: "int", nullable: false),
                    EntityObjValue = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EntityObjValueHash = table.Column<byte[]>(type: "varbinary(900)", nullable: true, computedColumnSql: "CAST(HASHBYTES('SHA2_256', EntityObjValue) AS VARBINARY(32))", stored: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntityValue", x => x.EntityValueId);
                    table.ForeignKey(
                        name: "FK_EntityValue_Entity_EntityId",
                        column: x => x.EntityId,
                        principalTable: "Entity",
                        principalColumn: "EntityID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MirrorEntity",
                columns: table => new
                {
                    EntityId = table.Column<int>(type: "int", nullable: false),
                    MirrorEntityId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MirrorEntity", x => new { x.EntityId, x.MirrorEntityId });
                    table.ForeignKey(
                        name: "FK_MirrorEntity_Entity_EntityId",
                        column: x => x.EntityId,
                        principalTable: "Entity",
                        principalColumn: "EntityID");
                    table.ForeignKey(
                        name: "FK_MirrorEntity_Entity_MirrorEntityId",
                        column: x => x.MirrorEntityId,
                        principalTable: "Entity",
                        principalColumn: "EntityID");
                });

            migrationBuilder.CreateTable(
                name: "EntityDataRevision",
                columns: table => new
                {
                    DataRevisionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateRevisionId = table.Column<int>(type: "int", nullable: false),
                    EntityId = table.Column<int>(type: "int", nullable: false),
                    DataRevisionNumber = table.Column<int>(type: "int", nullable: false),
                    DataRevisionDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntityDataRevision", x => x.DataRevisionId);
                    table.ForeignKey(
                        name: "FK_EntityDataRevision_EntityTemplateRevision_TemplateRevisionId",
                        column: x => x.TemplateRevisionId,
                        principalTable: "EntityTemplateRevision",
                        principalColumn: "TemplateRevisionID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EntityDataRevision_Entity_EntityId",
                        column: x => x.EntityId,
                        principalTable: "Entity",
                        principalColumn: "EntityID");
                });

            migrationBuilder.CreateTable(
                name: "EntityDependencyDefinition",
                columns: table => new
                {
                    EntityDependencyID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateRevisionID = table.Column<int>(type: "int", nullable: false),
                    EntityID = table.Column<int>(type: "int", nullable: false),
                    DependentEntityID = table.Column<int>(type: "int", nullable: false),
                    EntityOrder = table.Column<int>(type: "int", nullable: false),
                    IsValueType = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntityDependencyDefinition", x => x.EntityDependencyID);
                    table.ForeignKey(
                        name: "FK_EntityDependencyDefinition_EntityTemplateRevision_TemplateRevisionID",
                        column: x => x.TemplateRevisionID,
                        principalTable: "EntityTemplateRevision",
                        principalColumn: "TemplateRevisionID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EntityDependencyDefinition_Entity_DependentEntityID",
                        column: x => x.DependentEntityID,
                        principalTable: "Entity",
                        principalColumn: "EntityID");
                    table.ForeignKey(
                        name: "FK_EntityDependencyDefinition_Entity_EntityID",
                        column: x => x.EntityID,
                        principalTable: "Entity",
                        principalColumn: "EntityID");
                });

            migrationBuilder.InsertData(
                table: "EntityDataType",
                columns: new[] { "DataTypeID", "CreatedAt", "CreatedBy", "DataTypeDescription", "DataTypeFormat", "DataTypeName", "IsActive", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 8, 5, 13, 39, 3, 763, DateTimeKind.Utc).AddTicks(8011), 1, "Text values", null, "String", true, null, null },
                    { 2, new DateTime(2025, 8, 5, 13, 39, 3, 763, DateTimeKind.Utc).AddTicks(8018), 1, "Whole numbers", null, "Integer", true, null, null },
                    { 3, new DateTime(2025, 8, 5, 13, 39, 3, 763, DateTimeKind.Utc).AddTicks(8019), 1, "Date and time values", null, "Date", true, null, null },
                    { 4, new DateTime(2025, 8, 5, 13, 39, 3, 763, DateTimeKind.Utc).AddTicks(8020), 1, "True/False values", null, "Boolean", true, null, null },
                    { 5, new DateTime(2025, 8, 5, 13, 39, 3, 763, DateTimeKind.Utc).AddTicks(8020), 1, "Range values (e.g., 10:100)", null, "Range", true, null, null },
                    { 6, new DateTime(2025, 8, 5, 13, 39, 3, 763, DateTimeKind.Utc).AddTicks(8021), 1, "Range with step (e.g., 10:100:10)", null, "RangeSet", true, null, null }
                });

            migrationBuilder.InsertData(
                table: "UserMaster",
                columns: new[] { "UserID", "CreatedAt", "CreatedBy", "IsActive", "UserEmail", "UserName" },
                values: new object[] { 1, new DateTime(2025, 8, 5, 13, 39, 3, 763, DateTimeKind.Utc).AddTicks(8112), 1, true, null, "System" });

            migrationBuilder.CreateIndex(
                name: "IX_Entity_DataTypeID",
                table: "Entity",
                column: "DataTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_Entity_EntityName",
                table: "Entity",
                column: "EntityName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EntityDataRevision_EntityId",
                table: "EntityDataRevision",
                column: "EntityId");

            migrationBuilder.CreateIndex(
                name: "IX_EntityDataRevision_TemplateRevisionId",
                table: "EntityDataRevision",
                column: "TemplateRevisionId");

            migrationBuilder.CreateIndex(
                name: "IX_EntityDataType_DataTypeName",
                table: "EntityDataType",
                column: "DataTypeName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EntityDependencyDefinition_DependentEntityID",
                table: "EntityDependencyDefinition",
                column: "DependentEntityID");

            migrationBuilder.CreateIndex(
                name: "IX_EntityDependencyDefinition_EntityID",
                table: "EntityDependencyDefinition",
                column: "EntityID");

            migrationBuilder.CreateIndex(
                name: "IX_EntityDependencyDefinition_TemplateRevisionID",
                table: "EntityDependencyDefinition",
                column: "TemplateRevisionID");

            migrationBuilder.CreateIndex(
                name: "IX_EntityTemplateRevision_EntityID",
                table: "EntityTemplateRevision",
                column: "EntityID");

            migrationBuilder.CreateIndex(
                name: "IX_EntityValue_EntityId",
                table: "EntityValue",
                column: "EntityId");

            migrationBuilder.CreateIndex(
                name: "IX_EntityValue_Hash",
                table: "EntityValue",
                column: "EntityObjValueHash");

            migrationBuilder.CreateIndex(
                name: "IX_MirrorEntity_MirrorEntityId",
                table: "MirrorEntity",
                column: "MirrorEntityId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EntityDataRevision");

            migrationBuilder.DropTable(
                name: "EntityDependencyDefinition");

            migrationBuilder.DropTable(
                name: "EntityValue");

            migrationBuilder.DropTable(
                name: "MirrorEntity");

            migrationBuilder.DropTable(
                name: "UserMaster");

            migrationBuilder.DropTable(
                name: "EntityTemplateRevision");

            migrationBuilder.DropTable(
                name: "Entity");

            migrationBuilder.DropTable(
                name: "EntityDataType");
        }
    }
}
