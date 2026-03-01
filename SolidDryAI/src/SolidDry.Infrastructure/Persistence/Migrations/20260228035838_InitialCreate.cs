using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SolidDry.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CodeSubmissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    SourceType = table.Column<string>(type: "TEXT", nullable: false),
                    SourceContent = table.Column<string>(type: "TEXT", nullable: false),
                    SubmittedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    SubmittedBy = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CodeSubmissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VendorProfiles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", nullable: false),
                    Specialty = table.Column<string>(type: "TEXT", nullable: false),
                    PrecisionScore = table.Column<decimal>(type: "TEXT", precision: 5, scale: 4, nullable: false),
                    ReworkRate = table.Column<decimal>(type: "TEXT", precision: 5, scale: 4, nullable: false),
                    SlaComplianceRate = table.Column<decimal>(type: "TEXT", precision: 5, scale: 4, nullable: false),
                    TasksReviewed = table.Column<int>(type: "INTEGER", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VendorProfiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AuditEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CodeSubmissionId = table.Column<Guid>(type: "TEXT", nullable: true),
                    FindingId = table.Column<Guid>(type: "TEXT", nullable: true),
                    EventType = table.Column<string>(type: "TEXT", nullable: false),
                    ActorId = table.Column<string>(type: "TEXT", nullable: false),
                    Details = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditEvents_CodeSubmissions_CodeSubmissionId",
                        column: x => x.CodeSubmissionId,
                        principalTable: "CodeSubmissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Findings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CodeSubmissionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Principle = table.Column<int>(type: "INTEGER", nullable: false),
                    RuleKey = table.Column<string>(type: "TEXT", nullable: false),
                    Summary = table.Column<string>(type: "TEXT", nullable: false),
                    Evidence = table.Column<string>(type: "TEXT", nullable: false),
                    Severity = table.Column<int>(type: "INTEGER", nullable: false),
                    Confidence = table.Column<decimal>(type: "TEXT", precision: 5, scale: 4, nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Findings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Findings_CodeSubmissions_CodeSubmissionId",
                        column: x => x.CodeSubmissionId,
                        principalTable: "CodeSubmissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QaAdjudications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    FindingId = table.Column<Guid>(type: "TEXT", nullable: false),
                    QaReviewerId = table.Column<string>(type: "TEXT", nullable: false),
                    FinalLabel = table.Column<int>(type: "INTEGER", nullable: false),
                    DecisionNote = table.Column<string>(type: "TEXT", nullable: false),
                    CalibrationTag = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QaAdjudications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QaAdjudications_Findings_FindingId",
                        column: x => x.FindingId,
                        principalTable: "Findings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReviewDecisions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    FindingId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ReviewerId = table.Column<string>(type: "TEXT", nullable: false),
                    VendorId = table.Column<string>(type: "TEXT", nullable: true),
                    Label = table.Column<int>(type: "INTEGER", nullable: false),
                    Rationale = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReviewDecisions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReviewDecisions_Findings_FindingId",
                        column: x => x.FindingId,
                        principalTable: "Findings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditEvents_CodeSubmissionId",
                table: "AuditEvents",
                column: "CodeSubmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_Findings_CodeSubmissionId",
                table: "Findings",
                column: "CodeSubmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_QaAdjudications_FindingId",
                table: "QaAdjudications",
                column: "FindingId");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewDecisions_FindingId",
                table: "ReviewDecisions",
                column: "FindingId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditEvents");

            migrationBuilder.DropTable(
                name: "QaAdjudications");

            migrationBuilder.DropTable(
                name: "ReviewDecisions");

            migrationBuilder.DropTable(
                name: "VendorProfiles");

            migrationBuilder.DropTable(
                name: "Findings");

            migrationBuilder.DropTable(
                name: "CodeSubmissions");
        }
    }
}
