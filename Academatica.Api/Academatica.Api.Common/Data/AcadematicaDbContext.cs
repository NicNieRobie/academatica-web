using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Academatica.Api.Common.Models;
using System;

namespace Academatica.Api.Common.Data
{
    public class AcadematicaDbContext: IdentityDbContext<User, AcadematicaRole, Guid>
    {
        public AcadematicaDbContext(DbContextOptions<AcadematicaDbContext> options) : base(options) { }

        public DbSet<Tier> Tiers { get; set; }
        public DbSet<Topic> Topics { get; set; }
        public DbSet<Class> Classes { get; set; }
        public DbSet<Problem> Problems { get; set; }

        public DbSet<Achievement> Achievements { get; set; }
        public DbSet<StatsEntry> UserStats { get; set; }

        public DbSet<UserAchievement> UserAchievements { get; set; }
        public DbSet<UserClass> UserClasses { get; set; }
        public DbSet<UserTopic> UserTopic { get; set; }
        public DbSet<UserTier> UserTier { get; set; }
        public DbSet<UserTopicMistake> UserTopicMistakes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder
                .Entity<UserAchievement>().HasKey(u => new { u.UserId, u.AchievementId });

            modelBuilder
                .Entity<UserClass>().HasKey(u => new { u.UserId, u.ClassId });

            modelBuilder
                .Entity<UserTopic>().HasKey(u => new { u.UserId, u.TopicId });

            modelBuilder
                .Entity<UserTier>().HasKey(u => new { u.UserId, u.TierId });

            modelBuilder
                .Entity<UserTopicMistake>().HasKey(u => new { u.UserId, u.TopicId });
        }
    }
}
