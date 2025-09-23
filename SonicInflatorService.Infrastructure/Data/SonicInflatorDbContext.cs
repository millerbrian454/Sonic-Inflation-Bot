using Microsoft.EntityFrameworkCore;
using SonicInflatorService.Core.Entities;

namespace SonicInflatorService.Infrastructure.Data
{
    public class SonicInflatorDbContext : DbContext
    {
        public SonicInflatorDbContext(DbContextOptions<SonicInflatorDbContext> options) : base(options) { }

        public DbSet<ConfigurationEntity> Configurations { get; set; }
        public DbSet<OpenAIConfigurationEntity> OpenAIConfigurations { get; set; }
        public DbSet<OpenAIModelEntity> OpenAIModels { get; set; }
        public DbSet<DiscordConfigurationEntity> DiscordConfigurations { get; set; }
        public DbSet<DiscordChannelEntity> DiscordChannels { get; set; }
        public DbSet<DiscordContextChannelEntity> DiscordContextChannels { get; set; }
        public DbSet<DiscordProfessionalWranglerEntity> DiscordProfessionalWranglers { get; set; }
        public DbSet<DiscordNaughtyWordEntity> DiscordNaughtyWords { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuration entity
            modelBuilder.Entity<ConfigurationEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Key).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Section).HasMaxLength(255);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.HasIndex(e => new { e.Key, e.Section }).IsUnique();
            });

            // OpenAI Configuration
            modelBuilder.Entity<OpenAIConfigurationEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ApiKey).IsRequired().HasMaxLength(500);
                entity.Property(e => e.BaseUri).IsRequired().HasMaxLength(500);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
            });

            // OpenAI Models
            modelBuilder.Entity<OpenAIModelEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ModelName).IsRequired().HasMaxLength(255);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.HasOne(e => e.OpenAIConfiguration)
                      .WithMany(e => e.Models)
                      .HasForeignKey(e => e.OpenAIConfigurationId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Discord Configuration
            modelBuilder.Entity<DiscordConfigurationEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Token).IsRequired().HasMaxLength(500);
                entity.Property(e => e.SirenEmojiName).HasMaxLength(100);
                entity.Property(e => e.InflatedImagePath).HasMaxLength(500);
                entity.Property(e => e.DeflatedImagePath).HasMaxLength(500);
                entity.Property(e => e.SonichuImagePath).HasMaxLength(500);
                entity.Property(e => e.CurseYeHaMeHaImagePath).HasMaxLength(500);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
            });

            // Discord Channels
            modelBuilder.Entity<DiscordChannelEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.HasOne(e => e.DiscordConfiguration)
                      .WithMany(e => e.ChannelIds)
                      .HasForeignKey(e => e.DiscordConfigurationId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Discord Context Channels
            modelBuilder.Entity<DiscordContextChannelEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.HasOne(e => e.DiscordConfiguration)
                      .WithMany(e => e.ContextChannelIds)
                      .HasForeignKey(e => e.DiscordConfigurationId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Discord Professional Wranglers
            modelBuilder.Entity<DiscordProfessionalWranglerEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.HasOne(e => e.DiscordConfiguration)
                      .WithMany(e => e.ProfessionalSonicWranglerUserIds)
                      .HasForeignKey(e => e.DiscordConfigurationId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Discord Naughty Words
            modelBuilder.Entity<DiscordNaughtyWordEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.HasOne(e => e.DiscordConfiguration)
                      .WithMany(e => e.NaughtyWords)
                      .HasForeignKey(e => e.DiscordConfigurationId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
