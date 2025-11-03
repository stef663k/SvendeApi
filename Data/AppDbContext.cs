using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SvendeApi.Models;

namespace SvendeApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
    
    // DbSets for alle vores tabeller i databasen
    public DbSet<UserModel> Users { get; set; }
    public DbSet<RoleModel> Roles { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<PostModel> Posts { get; set; }
    public DbSet<CommentModel> Comments { get; set; }
    public DbSet<FollowerModel> Followers { get; set; }
    public DbSet<LikeModel> Likes { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Konfiguration af UserModel tabellen
        modelBuilder.Entity<UserModel>(entity =>
        {
            entity.HasKey(e => e.UserId);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Password).IsRequired().HasMaxLength(255);
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.HasIndex(e => e.Email).IsUnique(); // Sikrer at email er unik
        });

        // Konfiguration af RoleModel tabellen
        modelBuilder.Entity<RoleModel>(entity =>
        {
            entity.HasKey(e => e.RoleId);
            entity.Property(e => e.RoleName).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.RoleName).IsUnique(); // Sikrer at rolle navn er unik
        });

        // Many-to-many relationship mellem Users og Roles
        modelBuilder.Entity<UserRole>(entity =>
        {
            // Composite key: En sammensat nøgle der bruger både UserId og RoleId som primær nøgle.
            // Dette betyder at kombinationen af UserId + RoleId skal være unik, men hver enkelt værdi kan forekomme flere gange.
            entity.HasKey(e => new { e.UserId, e.RoleId });

            entity.HasOne(e => e.User)
                .WithMany(e => e.UserRoles)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Role)
                .WithMany(e => e.UserRoles)
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Konfiguration af PostModel tabellen
        modelBuilder.Entity<PostModel>(entity =>
        {
            entity.HasKey(e => e.PostId);
            entity.Property(e => e.Content).IsRequired();
            //Denne del her function fortæller at createdAt og updatedAt skal have default værdien GETUTCDATE() i sql server.
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.Author)
                .WithMany(e => e.Posts)
                .HasForeignKey(e => e.AuthorId)
                .OnDelete(DeleteBehavior.Restrict); // Forhindrer cascade delete problemer

            entity.HasOne(e => e.ParentPost)
                .WithMany()
                .HasForeignKey(e => e.ParentPostId)
                .OnDelete(DeleteBehavior.Restrict); // Self-referencing for reposts
        });

        // Konfiguration af CommentModel tabellen
        modelBuilder.Entity<CommentModel>(entity =>
        {
            entity.HasKey(e => e.CommentId);
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.Post)
                .WithMany()
                .HasForeignKey(e => e.PostId)
                .OnDelete(DeleteBehavior.Cascade); // Slet kommentarer når post slettes

            entity.HasOne(e => e.Author)
                .WithMany(e => e.Comments)
                .HasForeignKey(e => e.AuthorId)
                .OnDelete(DeleteBehavior.Restrict); // Forhindrer cascade delete problemer

            entity.HasOne(e => e.ParentComment)
                .WithMany()
                .HasForeignKey(e => e.ParentCommentId)
                .OnDelete(DeleteBehavior.Restrict); // Self-referencing for nested comments
        });

        // Self-referencing many-to-many for user following system
        modelBuilder.Entity<FollowerModel>(entity =>
        {
            // Composite key: En sammensat nøgle der bruger både FollowerUserId og FolloweeUserId som primær nøgle.
            // Dette sikrer at en bruger kun kan følge en anden bruger én gang, men kan følge mange forskellige brugere.
            // Eksempel: User 1 kan følge User 2 og User 3, men kan ikke følge User 2 to gange.
            // Kombinationen af FollowerUserId + FolloweeUserId skal være unik i tabellen.
            entity.HasKey(e => new { e.FollowerUserId, e.FolloweeUserId });
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.FollowerUser)
                .WithMany(e => e.Followees)
                .HasForeignKey(e => e.FollowerUserId)
                .OnDelete(DeleteBehavior.Restrict); // Forhindrer cascade delete problemer

            entity.HasOne(e => e.FolloweeUser)
                .WithMany(e => e.Followers)
                .HasForeignKey(e => e.FolloweeUserId)
                .OnDelete(DeleteBehavior.Restrict); // Forhindrer cascade delete problemer
        });

        // Konfiguration af LikeModel tabellen
        modelBuilder.Entity<LikeModel>(entity =>
        {
            entity.HasKey(e => e.LikeId);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.Post)
                .WithMany()
                .HasForeignKey(e => e.PostId)
                .OnDelete(DeleteBehavior.Cascade); // Slet likes når post slettes

            entity.HasOne(e => e.User)
                .WithMany(e => e.Likes)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict); // Forhindrer cascade delete problemer

            entity.HasIndex(e => new { e.UserId, e.PostId }).IsUnique(); // Sikrer at en bruger kun kan like en post én gang
        });

        // Konfiguration af RefreshToken tabellen for JWT authentication
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.RefreshTokenId);
            entity.Property(e => e.Token).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.CreatedByIp).HasMaxLength(45); // IPv4/IPv6 adresse
            entity.Property(e => e.RevokedByIp).HasMaxLength(45);
            entity.Property(e => e.ReplacedByToken).HasMaxLength(255);
            entity.Property(e => e.RevocationReason).HasMaxLength(255);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade); // Slet tokens når bruger slettes

            entity.HasIndex(e => e.Token).IsUnique(); // Sikrer at hver token er unik
        });
        
    }
}