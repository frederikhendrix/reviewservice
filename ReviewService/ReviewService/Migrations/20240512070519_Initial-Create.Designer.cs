﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using ReviewService.Data;

#nullable disable

namespace ReviewService.Migrations
{
    [DbContext(typeof(DataContext))]
    [Migration("20240512070519_Initial-Create")]
    partial class InitialCreate
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("ReviewService.Models.Review", b =>
                {
                    b.Property<Guid>("ReviewId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<bool>("IsVisible")
                        .HasColumnType("boolean");

                    b.Property<Guid>("MovieId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("ReviewedById")
                        .HasColumnType("uuid");

                    b.Property<int>("Score")
                        .HasColumnType("integer");

                    b.Property<string>("Text")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.HasKey("ReviewId");

                    b.ToTable("Reviews");
                });
#pragma warning restore 612, 618
        }
    }
}
