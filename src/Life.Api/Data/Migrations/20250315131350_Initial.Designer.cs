﻿// <auto-generated />
using System;
using Life.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Life.Api.Data.Migrations
{
    [DbContext(typeof(GameDbContext))]
    [Migration("20250315131350_Initial")]
    partial class Initial
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "7.0.20");

            modelBuilder.Entity("Life.Domain.Game", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<bool[,]>("RawBoard")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnName("Board");

                    b.HasKey("Id");

                    b.ToTable("Games");
                });
#pragma warning restore 612, 618
        }
    }
}
