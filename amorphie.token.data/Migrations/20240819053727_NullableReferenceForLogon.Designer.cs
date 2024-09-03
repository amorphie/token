﻿// <auto-generated />
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using amorphie.token.data;

#nullable disable

namespace amorphie.token.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    [Migration("20240819053727_NullableReferenceForLogon")]
    partial class NullableReferenceForLogon
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Microsoft.AspNetCore.DataProtection.EntityFrameworkCore.DataProtectionKey", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("FriendlyName")
                        .HasColumnType("text");

                    b.Property<string>("Xml")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("DataProtectionKeys");
                });

            modelBuilder.Entity("amorphie.token.core.Models.Token.FailedLogon", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uuid");

                    b.Property<string>("ClientId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid>("LogonId")
                        .HasColumnType("uuid");

                    b.Property<string>("Reference")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("LogonId");

                    b.HasIndex("Reference");

                    b.ToTable("FailedLogon");
                });

            modelBuilder.Entity("amorphie.token.core.Models.Token.Logon", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("ClientId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Error")
                        .HasColumnType("text");

                    b.Property<long>("LastJobKey")
                        .HasColumnType("bigint");

                    b.Property<int>("LogonStatus")
                        .HasColumnType("integer");

                    b.Property<int>("LogonType")
                        .HasColumnType("integer");

                    b.Property<string>("Reference")
                        .HasColumnType("text");

                    b.Property<long>("WorkflowInstanceId")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.HasIndex("Reference");

                    b.HasIndex("WorkflowInstanceId");

                    b.ToTable("Logon");
                });

            modelBuilder.Entity("amorphie.token.core.Models.Token.TokenInfo", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("ClientId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid?>("ConsentId")
                        .HasColumnType("uuid");

                    b.Property<string>("DeviceId")
                        .HasColumnType("text");

                    b.Property<DateTime>("ExpiredAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<bool>("IsActive")
                        .HasColumnType("boolean");

                    b.Property<DateTime>("IssuedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Reference")
                        .HasColumnType("text");

                    b.Property<Guid?>("RelatedTokenId")
                        .HasColumnType("uuid");

                    b.Property<List<string>>("Scopes")
                        .IsRequired()
                        .HasColumnType("text[]");

                    b.Property<int>("TokenType")
                        .HasColumnType("integer");

                    b.Property<Guid?>("UserId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("ConsentId");

                    b.HasIndex("Reference");

                    b.HasIndex("UserId");

                    b.ToTable("Tokens");
                });

            modelBuilder.Entity("amorphie.token.core.Models.Token.FailedLogon", b =>
                {
                    b.HasOne("amorphie.token.core.Models.Token.Logon", "Logon")
                        .WithMany("FailedLogons")
                        .HasForeignKey("LogonId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Logon");
                });

            modelBuilder.Entity("amorphie.token.core.Models.Token.Logon", b =>
                {
                    b.Navigation("FailedLogons");
                });
#pragma warning restore 612, 618
        }
    }
}