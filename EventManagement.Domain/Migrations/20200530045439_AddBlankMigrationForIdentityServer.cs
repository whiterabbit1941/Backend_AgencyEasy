using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EventManagement.Domain.Migrations
{
    public partial class AddBlankMigrationForIdentityServer : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Added Blank Migration 
            // With this we have added the IDP entities to our Main Project.
            // Now on any changes to the db design needed to the Identity server should be done from the Main Project
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
