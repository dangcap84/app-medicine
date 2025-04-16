﻿// <auto-generated />
using System;
using MediTrack.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MediTrack.Infrastructure.Data.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20250416075653_AddSpecificDaysFrequencyType")]
    partial class AddSpecificDaysFrequencyType
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("MediTrack.Domain.Entities.Medicine", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Dosage")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid>("MedicineUnitId")
                        .HasColumnType("uuid");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Notes")
                        .HasColumnType("text");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("MedicineUnitId");

                    b.HasIndex("UserId");

                    b.ToTable("Medicines");
                });

            modelBuilder.Entity("MediTrack.Domain.Entities.MedicineUnit", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Description")
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("MedicineUnits");
                });

            modelBuilder.Entity("MediTrack.Domain.Entities.Notification", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<bool>("IsRead")
                        .HasColumnType("boolean");

                    b.Property<string>("Message")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid>("ScheduleTimeId")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("ScheduledTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime?>("SentTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("ScheduleTimeId");

                    b.HasIndex("UserId");

                    b.ToTable("Notifications");
                });

            modelBuilder.Entity("MediTrack.Domain.Entities.Schedule", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("DaysOfWeek")
                        .HasColumnType("text");

                    b.Property<DateTime?>("EndDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("FrequencyType")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid>("MedicineId")
                        .HasColumnType("uuid");

                    b.Property<string>("Notes")
                        .HasColumnType("text");

                    b.Property<DateTime>("StartDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("MedicineId");

                    b.HasIndex("UserId");

                    b.ToTable("Schedules");
                });

            modelBuilder.Entity("MediTrack.Domain.Entities.ScheduleTime", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<int>("Quantity")
                        .HasColumnType("integer");

                    b.Property<Guid>("ScheduleId")
                        .HasColumnType("uuid");

                    b.Property<TimeOnly>("TimeOfDay")
                        .HasColumnType("time without time zone");

                    b.HasKey("Id");

                    b.HasIndex("ScheduleId");

                    b.ToTable("ScheduleTimes");
                });

            modelBuilder.Entity("MediTrack.Domain.Entities.User", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("PasswordHash")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("Email")
                        .IsUnique();

                    b.ToTable("Users");
                });

            modelBuilder.Entity("MediTrack.Domain.Entities.UserProfile", b =>
                {
                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.Property<string>("AvatarUrl")
                        .HasColumnType("text");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime>("DateOfBirth")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Gender")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("UserId");

                    b.ToTable("UserProfiles");
                });

            modelBuilder.Entity("MediTrack.Domain.Entities.Medicine", b =>
                {
                    b.HasOne("MediTrack.Domain.Entities.MedicineUnit", "MedicineUnit")
                        .WithMany("Medicines")
                        .HasForeignKey("MedicineUnitId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("MediTrack.Domain.Entities.User", "User")
                        .WithMany("Medicines")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("MedicineUnit");

                    b.Navigation("User");
                });

            modelBuilder.Entity("MediTrack.Domain.Entities.Notification", b =>
                {
                    b.HasOne("MediTrack.Domain.Entities.ScheduleTime", "ScheduleTime")
                        .WithMany("Notifications")
                        .HasForeignKey("ScheduleTimeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("MediTrack.Domain.Entities.User", "User")
                        .WithMany("Notifications")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ScheduleTime");

                    b.Navigation("User");
                });

            modelBuilder.Entity("MediTrack.Domain.Entities.Schedule", b =>
                {
                    b.HasOne("MediTrack.Domain.Entities.Medicine", "Medicine")
                        .WithMany("Schedules")
                        .HasForeignKey("MedicineId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("MediTrack.Domain.Entities.User", "User")
                        .WithMany("Schedules")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Medicine");

                    b.Navigation("User");
                });

            modelBuilder.Entity("MediTrack.Domain.Entities.ScheduleTime", b =>
                {
                    b.HasOne("MediTrack.Domain.Entities.Schedule", "Schedule")
                        .WithMany("ScheduleTimes")
                        .HasForeignKey("ScheduleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Schedule");
                });

            modelBuilder.Entity("MediTrack.Domain.Entities.UserProfile", b =>
                {
                    b.HasOne("MediTrack.Domain.Entities.User", "User")
                        .WithOne("UserProfile")
                        .HasForeignKey("MediTrack.Domain.Entities.UserProfile", "UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("MediTrack.Domain.Entities.Medicine", b =>
                {
                    b.Navigation("Schedules");
                });

            modelBuilder.Entity("MediTrack.Domain.Entities.MedicineUnit", b =>
                {
                    b.Navigation("Medicines");
                });

            modelBuilder.Entity("MediTrack.Domain.Entities.Schedule", b =>
                {
                    b.Navigation("ScheduleTimes");
                });

            modelBuilder.Entity("MediTrack.Domain.Entities.ScheduleTime", b =>
                {
                    b.Navigation("Notifications");
                });

            modelBuilder.Entity("MediTrack.Domain.Entities.User", b =>
                {
                    b.Navigation("Medicines");

                    b.Navigation("Notifications");

                    b.Navigation("Schedules");

                    b.Navigation("UserProfile");
                });
#pragma warning restore 612, 618
        }
    }
}
