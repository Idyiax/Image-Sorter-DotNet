﻿// <auto-generated />
using System;
using Image_Sorter_DotNet.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Image_Sorter_DotNet.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20250311191213_Changed-Tag-Colour-Format")]
    partial class ChangedTagColourFormat
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("Image_Sorter_DotNet.Models.CollectionConnections", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("AltIndex")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("CollectionId")
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("datetime2");

                    b.Property<int>("ImageId")
                        .HasColumnType("int");

                    b.Property<string>("Index")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("CollectionId");

                    b.HasIndex("ImageId");

                    b.ToTable("CollectionConnections");
                });

            modelBuilder.Entity("Image_Sorter_DotNet.Models.Collections", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("CollectionName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.ToTable("Collections");
                });

            modelBuilder.Entity("Image_Sorter_DotNet.Models.Images", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("FileName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FilePathName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Images");
                });

            modelBuilder.Entity("Image_Sorter_DotNet.Models.TagConnections", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("datetime2");

                    b.Property<int>("ImageId")
                        .HasColumnType("int");

                    b.Property<int>("TagId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("ImageId");

                    b.HasIndex("TagId");

                    b.ToTable("TagConnections");
                });

            modelBuilder.Entity("Image_Sorter_DotNet.Models.TagRelations", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("ChildTagId")
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("datetime2");

                    b.Property<int>("ParentTagId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("ChildTagId");

                    b.HasIndex("ParentTagId");

                    b.ToTable("TagRelations");
                });

            modelBuilder.Entity("Image_Sorter_DotNet.Models.Tags", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("ColourHex")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("TagName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Tags");
                });

            modelBuilder.Entity("Image_Sorter_DotNet.Models.CollectionConnections", b =>
                {
                    b.HasOne("Image_Sorter_DotNet.Models.Collections", "Collection")
                        .WithMany("CollectionConnection")
                        .HasForeignKey("CollectionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Image_Sorter_DotNet.Models.Images", "Image")
                        .WithMany("CollectionConnection")
                        .HasForeignKey("ImageId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Collection");

                    b.Navigation("Image");
                });

            modelBuilder.Entity("Image_Sorter_DotNet.Models.TagConnections", b =>
                {
                    b.HasOne("Image_Sorter_DotNet.Models.Images", "Image")
                        .WithMany()
                        .HasForeignKey("ImageId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Image_Sorter_DotNet.Models.Tags", "Tag")
                        .WithMany("TagConnections")
                        .HasForeignKey("TagId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Image");

                    b.Navigation("Tag");
                });

            modelBuilder.Entity("Image_Sorter_DotNet.Models.TagRelations", b =>
                {
                    b.HasOne("Image_Sorter_DotNet.Models.Tags", "ChildTag")
                        .WithMany("ChildTagRelations")
                        .HasForeignKey("ChildTagId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("Image_Sorter_DotNet.Models.Tags", "ParentTag")
                        .WithMany("ParentTagRelations")
                        .HasForeignKey("ParentTagId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("ChildTag");

                    b.Navigation("ParentTag");
                });

            modelBuilder.Entity("Image_Sorter_DotNet.Models.Collections", b =>
                {
                    b.Navigation("CollectionConnection");
                });

            modelBuilder.Entity("Image_Sorter_DotNet.Models.Images", b =>
                {
                    b.Navigation("CollectionConnection");
                });

            modelBuilder.Entity("Image_Sorter_DotNet.Models.Tags", b =>
                {
                    b.Navigation("ChildTagRelations");

                    b.Navigation("ParentTagRelations");

                    b.Navigation("TagConnections");
                });
#pragma warning restore 612, 618
        }
    }
}
