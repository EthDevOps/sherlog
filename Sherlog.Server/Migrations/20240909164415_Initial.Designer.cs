﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Sherlog.Server;

#nullable disable

namespace Sherlog.Server.Migrations
{
    [DbContext(typeof(LogbookContext))]
    [Migration("20240909164415_Initial")]
    partial class Initial
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.8");

            modelBuilder.Entity("Sherlog.Server.LogEntry", b =>
                {
                    b.Property<int>("LogEntryId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("TEXT");

                    b.Property<string>("LogType")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Message")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Project")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("RecipientGroups")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Tenant")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("LogEntryId");

                    b.ToTable("LogEntries");
                });
#pragma warning restore 612, 618
        }
    }
}
