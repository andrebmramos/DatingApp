using DatingApp.API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DatingApp.API.Data
{
    public class DataContext : IdentityDbContext<User, Role, int, 
        IdentityUserClaim<int>, UserRole, IdentityUserLogin<int>,
        IdentityRoleClaim<int>, IdentityUserToken<int>>
    {
        // public DbSet<User> Users { get; set; } // Identity assume
        public DbSet<Value> Values { get; set; }
        public DbSet<Photo> Photos { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<Message> Messages { get; set; }


        public DataContext(DbContextOptions<DataContext> options) : base(options) { }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            // aula 199 (Seção Identity)
            base.OnModelCreating(builder);

            builder.Entity<UserRole>(ur =>
            {
                ur.HasKey(ur => new { ur.UserId, ur.RoleId });

                ur.HasOne(ur => ur.Role)
                    .WithMany(ur => ur.UserRoles)
                    .HasForeignKey(ur => ur.RoleId)
                    .IsRequired();

                ur.HasOne(ur => ur.User)
                    .WithMany(ur => ur.UserRoles)
                    .HasForeignKey(ur => ur.UserId)
                    .IsRequired();
            });
            // fim da aula 199 (Seção Identity)

            // Chave
            builder.Entity<Like>()
                .HasKey(key => new {key.LikerId, key.LikeeId });

            // Likes N para N
            builder.Entity<Like>()
                .HasOne(like => like.Likee)
                .WithMany(user => user.Likers)
                .HasForeignKey(user => user.LikeeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Like>()
                .HasOne(like => like.Liker)
                .WithMany(user => user.Likees)
                .HasForeignKey(user => user.LikerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Messages N para N
            builder.Entity<Message>()
                .HasOne(m => m.Sender)
                .WithMany(user => user.MessagesSent)
                //.HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Message>()
                .HasOne(m => m.Recipient)
                .WithMany(user => user.MessagesReceived)
                //.HasForeignKey(m => m.RecipientId)
                .OnDelete(DeleteBehavior.Restrict);

        }

    }

}
