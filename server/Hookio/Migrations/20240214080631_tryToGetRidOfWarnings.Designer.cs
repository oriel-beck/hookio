﻿// <auto-generated />
using System;
using Hookio.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Hookio.Migrations
{
    [DbContext(typeof(HookioContext))]
    [Migration("20240214080631_tryToGetRidOfWarnings")]
    partial class tryToGetRidOfWarnings
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.1")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Hookio.Database.Entities.Embed", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<bool>("AddTimestamp")
                        .HasColumnType("boolean");

                    b.Property<string>("Author")
                        .HasColumnType("text");

                    b.Property<string>("AuthorIcon")
                        .HasColumnType("text");

                    b.Property<string>("Color")
                        .HasColumnType("text");

                    b.Property<string>("Description")
                        .HasColumnType("text");

                    b.Property<string>("Footer")
                        .HasColumnType("text");

                    b.Property<string>("FooterIcon")
                        .HasColumnType("text");

                    b.Property<string>("Image")
                        .HasColumnType("text");

                    b.Property<int>("MessageId")
                        .HasColumnType("integer");

                    b.Property<string>("Thumbnail")
                        .HasColumnType("text");

                    b.Property<string>("Title")
                        .HasColumnType("text");

                    b.Property<string>("Url")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("MessageId");

                    b.ToTable("Embeds");
                });

            modelBuilder.Entity("Hookio.Database.Entities.EmbedField", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("EmbedId")
                        .HasColumnType("integer");

                    b.Property<bool>("Inline")
                        .HasColumnType("boolean");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Value")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("EmbedId");

                    b.ToTable("EmbedFields");
                });

            modelBuilder.Entity("Hookio.Database.Entities.Message", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Content")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("SubscriptionId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("SubscriptionId")
                        .IsUnique();

                    b.ToTable("Messages");
                });

            modelBuilder.Entity("Hookio.Database.Entities.Subscription", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("ChannelId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<decimal>("GuildId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<int>("MessageId")
                        .HasColumnType("integer");

                    b.Property<int>("SubscriptionType")
                        .HasColumnType("integer");

                    b.Property<string>("WebhookUrl")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("GuildId");

                    b.HasIndex("SubscriptionType");

                    b.ToTable("Subscriptions");
                });

            modelBuilder.Entity("Hookio.Database.Entities.User", b =>
                {
                    b.Property<decimal>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("numeric(20,0)");

                    b.Property<string>("AccessToken")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTimeOffset>("ExpireAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("Premium")
                        .HasColumnType("integer");

                    b.Property<DateTimeOffset?>("PremiumExpires")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("RefreshToken")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("Hookio.Database.Entities.Embed", b =>
                {
                    b.HasOne("Hookio.Database.Entities.Message", "Message")
                        .WithMany("Embeds")
                        .HasForeignKey("MessageId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Message");
                });

            modelBuilder.Entity("Hookio.Database.Entities.EmbedField", b =>
                {
                    b.HasOne("Hookio.Database.Entities.Embed", "Embed")
                        .WithMany("Fields")
                        .HasForeignKey("EmbedId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Embed");
                });

            modelBuilder.Entity("Hookio.Database.Entities.Message", b =>
                {
                    b.HasOne("Hookio.Database.Entities.Subscription", "Subscription")
                        .WithOne("Message")
                        .HasForeignKey("Hookio.Database.Entities.Message", "SubscriptionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Subscription");
                });

            modelBuilder.Entity("Hookio.Database.Entities.Embed", b =>
                {
                    b.Navigation("Fields");
                });

            modelBuilder.Entity("Hookio.Database.Entities.Message", b =>
                {
                    b.Navigation("Embeds");
                });

            modelBuilder.Entity("Hookio.Database.Entities.Subscription", b =>
                {
                    b.Navigation("Message");
                });
#pragma warning restore 612, 618
        }
    }
}
