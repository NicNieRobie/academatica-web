using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SmartMath.Api.Common.Models;
using SmartMath.Api.Common.Models.Tokens;
using System;

namespace SmartMath.Api.Common.Data
{
    public class SmartMathDbContext: IdentityDbContext<User, SmartMathRole, Guid>
    {
        public SmartMathDbContext(DbContextOptions<SmartMathDbContext> options) : base(options) { }

        public DbSet<Tier> Tiers { get; set; }
        public DbSet<Topic> Topics { get; set; }
        public DbSet<Class> Classes { get; set; }

        public DbSet<Achievement> Achievements { get; set; }
        public DbSet<LeaderboardEntry> Leaderboard { get; set; }
        public DbSet<RefreshToken> UserRefreshTokens { get; set; }
        public DbSet<StatsEntry> UserStats { get; set; }

        public DbSet<UserAchievement> UserAchievements { get; set; }
        public DbSet<UserClass> UserClasses { get; set; }
        public DbSet<UserTopic> UserTopics { get; set; }
        public DbSet<UserTier> UserTiers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder
                .Entity<LeaderboardEntry>().HasKey(u => u.UserId);

            modelBuilder
                .Entity<UserAchievement>().HasKey(u => new { u.UserId, u.AchievementId });

            modelBuilder
                .Entity<UserClass>().HasKey(u => new { u.UserId, u.ClassId });

            modelBuilder
                .Entity<UserTopic>().HasKey(u => new { u.UserId, u.TopicId });

            modelBuilder
                .Entity<UserTier>().HasKey(u => new { u.UserId, u.TierId });
        }
    }
}
