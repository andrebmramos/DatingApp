using DatingApp.API.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DatingApp.API.Data
{
    public class DataContext : DbContext
    {
        public DbSet<Value> Values { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Photo> Photos { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<Message> Messages { get; set; }


        public DataContext(DbContextOptions<DataContext> options) : base(options) { }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            // Chave
            builder.Entity<Like>()
                .HasKey(key => new {key.LikerId, key.LikeeId });

            // 1 Nota <=> N Detalhes 
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
