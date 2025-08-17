using Microsoft.EntityFrameworkCore;
using WhatSharp.Shared.Models;

namespace WhatSharp.Srv.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Chat> Chats { get; set; }

        public DbSet<ChatReadState> ChatReadStates => Set<ChatReadState>();
 
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Chat>()
                .HasMany(c => c.Messages)
                .WithOne(m => m.Chat)
                .HasForeignKey(m => m.ChatId);


            modelBuilder.Entity<Chat>()
                .HasMany(c => c.Participants)
                .WithMany(u => u.Chats) // ou .WithMany() si pas de nav inverse dans User
                .UsingEntity(j => j.ToTable("ChatParticipants"));

            modelBuilder.Entity<ChatReadState>()
                .HasKey(x => new { x.ChatId, x.UserId });
        }
    }
}
