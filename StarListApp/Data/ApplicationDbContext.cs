using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StarListApp.Models;
using System;

namespace StarListApp.Data;

public class ApplicationDbContext : IdentityDbContext<StarListUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Setlist> Setlists { get; set; }
    public DbSet<Song> Songs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Allow this to cascade (if you want deleting a setlist to delete its songs)
        modelBuilder.Entity<Song>()
            .HasOne(s => s.Setlist)
            .WithMany(sl => sl.Songs)
            .HasForeignKey(s => s.SetlistId)
            .OnDelete(DeleteBehavior.Cascade);

        // Prevent cascade on User → Song
        modelBuilder.Entity<Song>()
            .HasOne(s => s.User)
            .WithMany(u => u.Songs)
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Restrict); // or DeleteBehavior.NoAction

        // Also restrict on User → Setlist to be safe
        modelBuilder.Entity<Setlist>()
            .HasOne(s => s.User)
            .WithMany(u => u.Setlists)
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}