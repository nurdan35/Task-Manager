﻿using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
 using Microsoft.EntityFrameworkCore;
 using TaskManagement.Models;
using TaskManagement.ViewModels;
 
 namespace TaskManagement.Data
 {
     public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
     {
         public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
             : base(options)
         {
         }
         public DbSet<Board> Boards { get; set; }
         public DbSet<TaskItem> TaskItems { get; set; }
         public DbSet<Notification> Notifications { get; set; }
         
         public DbSet<Friendship> Friendships { get; set; }
         
         public DbSet<BoardShare> BoardShares { get; set; }
         
         protected override void OnModelCreating(ModelBuilder modelBuilder)
         {
             base.OnModelCreating(modelBuilder);
 
             // ApplicationUser ve Board arasında bire-çok ilişki
             modelBuilder.Entity<Board>()
                 .HasOne(b => b.User)
                 .WithMany(u => u.Boards)
                 .HasForeignKey(b => b.UserId)
                 .OnDelete(DeleteBehavior.Cascade);
             
             // ApplicationUser ve TaskItem arasında bire-çok ilişki
             modelBuilder.Entity<TaskItem>()
                 .HasOne(t => t.User)
                 .WithMany(u => u.TaskItems)
                 .HasForeignKey(t => t.UserId)
                 .OnDelete(DeleteBehavior.Cascade);

             // Board ve TaskItem arasında bire-çok ilişki
             modelBuilder.Entity<TaskItem>()
                 .HasOne(t => t.Board)
                 .WithMany(b => b.Tasks)
                 .HasForeignKey(t => t.BoardId)
                 .OnDelete(DeleteBehavior.Cascade);
 
             // ApplicationUser ve Board arasında iş birliği ilişkisi
             modelBuilder.Entity<Board>()
                 .HasMany(b => b.Collaborators)
                 .WithMany("CollaboratingBoards"); // İki yönlü çoklu ilişki için CollaboratingBoards özelliği ApplicationUser sınıfında tanımlanabilir.
             

             modelBuilder.Entity<BoardShare>()
                 .HasOne(bs => bs.SharedWithUser)
                 .WithMany(u => u.SharedBoards)
                 .HasForeignKey(bs => bs.SharedWithUserId);
             
             modelBuilder.Entity<Board>()
                 .HasMany(b => b.BoardShares)
                 .WithOne(bs => bs.Board)
                 .HasForeignKey(bs => bs.BoardId)
                 .OnDelete(DeleteBehavior.Cascade);
         }
        
     }
 }