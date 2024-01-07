﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using testingStuff.data;

#nullable disable

namespace testingStuff.Migrations
{
    [DbContext(typeof(DbDataContext))]
    partial class DbDataContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.14")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("testingStuff.models.Chat", b =>
                {
                    b.Property<Guid>("id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)");

                    b.Property<Guid>("user_id")
                        .HasColumnType("char(36)");

                    b.HasKey("id");

                    b.HasIndex("user_id");

                    b.ToTable("Chats");
                });

            modelBuilder.Entity("testingStuff.models.ChatSucessfullResponse", b =>
                {
                    b.Property<Guid>("id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)");

                    b.Property<Guid>("conversation_id")
                        .HasColumnType("char(36)");

                    b.Property<bool>("is_final")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("response")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("id");

                    b.HasIndex("conversation_id");

                    b.ToTable("AiResponses");
                });

            modelBuilder.Entity("testingStuff.models.User", b =>
                {
                    b.Property<Guid>("id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)");

                    b.Property<string>("UserName")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("passHash")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("salt")
                        .HasColumnType("longtext");

                    b.HasKey("id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("testingStuff.models.UserPrompt", b =>
                {
                    b.Property<Guid?>("id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)");

                    b.Property<Guid>("conversation_id")
                        .HasColumnType("char(36)");

                    b.Property<string>("prompt")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("id");

                    b.HasIndex("conversation_id");

                    b.ToTable("userPrompts");
                });

            modelBuilder.Entity("testingStuff.models.Chat", b =>
                {
                    b.HasOne("testingStuff.models.User", "user")
                        .WithMany("chats")
                        .HasForeignKey("user_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("user");
                });

            modelBuilder.Entity("testingStuff.models.ChatSucessfullResponse", b =>
                {
                    b.HasOne("testingStuff.models.Chat", "chat")
                        .WithMany("chatPrompts")
                        .HasForeignKey("conversation_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("chat");
                });

            modelBuilder.Entity("testingStuff.models.UserPrompt", b =>
                {
                    b.HasOne("testingStuff.models.Chat", "chat")
                        .WithMany("userPrompts")
                        .HasForeignKey("conversation_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("chat");
                });

            modelBuilder.Entity("testingStuff.models.Chat", b =>
                {
                    b.Navigation("chatPrompts");

                    b.Navigation("userPrompts");
                });

            modelBuilder.Entity("testingStuff.models.User", b =>
                {
                    b.Navigation("chats");
                });
#pragma warning restore 612, 618
        }
    }
}
