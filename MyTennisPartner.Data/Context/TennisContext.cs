using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using MyTennisPartner.Data.Models;
using MyTennisPartner.Models.Enums;

namespace MyTennisPartner.Data.Context
{
    public class TennisContext : DbContext
    {
        // normal constructor for app, unit tests
        public TennisContext(DbContextOptions<TennisContext> options) : base(options) { }

        // constructor for LinqPad purposes, takes a conn string instead of dbOptions
        public TennisContext(string connectionString) : base(new DbContextOptionsBuilder()
                .UseSqlServer(connectionString).Options)
        { }

        public DbSet<Address> Addresses { get; set; }
        public DbSet<Contact> Contacts { get; set; }
        public DbSet<LeagueMember> LeagueMembers { get; set; }
        public DbSet<League> Leagues { get; set; }
        public DbSet<Line> Lines { get; set; }
        public DbSet<Match> Matches { get; set; }
        public DbSet<Member> Members { get; set; }
        public DbSet<Session> Sessions { get; set; }
        public DbSet<PlayerPreference> PlayerPreferences { get; set; }
        public DbSet<Venue> Venues { get; set; }
        public DbSet<MemberImage> MemberImages { get; set; }
        public DbSet<MemberRole> MemberRoles { get; set; }
        public DbSet<Player> Players { get; set; }
        public DbSet<ReservationSystem> ReservationSystems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            if (modelBuilder is null) return;

            // configure join tables for league<-->Member
            modelBuilder.Entity<LeagueMember>()
               .HasOne(lp => lp.League)
               .WithMany(l => l.LeagueMembers)
               .HasForeignKey(lp => lp.LeagueId);

            modelBuilder.Entity<LeagueMember>()
               .HasOne(lp => lp.Member)
               .WithMany(p => p.LeagueMembers)
               .HasForeignKey(lp => lp.MemberId);

            // set default values
            modelBuilder.Entity<League>()
                .Property(b => b.DefaultNumberOfLines)
                .HasDefaultValue(1);

            // set default value
            modelBuilder.Entity<Member>()
                .Property(m => m.MemberRoleFlags)
                .HasDefaultValue(MemberRoleFlags.Player);

            // set default value
            modelBuilder.Entity<Member>()
                .Property(m => m.EmailNotificationFlags)
                .HasDefaultValue(EmailNotificationFlags.AllNotifications);

            // set default value
            modelBuilder.Entity<Member>()
                .Property(m => m.TextNotificationFlags)
                .HasDefaultValue(TextNotificationFlags.DefaultNotifications);

            // disable cascade delete for lines, so we don't get circular actions
            modelBuilder.Entity<Line>()
                .HasOne(l => l.Match)
                .WithMany(m => m.Lines)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Player>()
                .HasOne(p => p.LeagueMember)
                .WithMany(lm => lm.Players)
                .HasForeignKey(p => p.LeagueMemberId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Player>()
                .HasOne(p => p.Match)
                .WithMany(m => m.Players)
                .HasForeignKey(p => p.MatchId)
                .OnDelete(DeleteBehavior.Restrict);                
        }
    }
}
