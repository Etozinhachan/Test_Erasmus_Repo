using Microsoft.EntityFrameworkCore;
using testingStuff.models;

namespace testingStuff.data;

public class DbDataContext : DbContext
{
    public DbDataContext(DbContextOptions<DbDataContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Chat> Chats { get; set; } = null!;
    public DbSet<ChatSucessfullResponse> AiResponses { get; set; } = null!;
    public DbSet<UserPrompt> userPrompts { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasMany(e => e.chats)
            .WithOne(e => e.user)
            .HasForeignKey(e => e.user_id)
            .HasPrincipalKey(e => e.id);

        modelBuilder.Entity<Chat>()
            .HasMany(e => e.chatPrompts)
            .WithOne(e => e.chat)
            .HasForeignKey(e => e.conversation_id)
            .HasPrincipalKey(e => e.id);
        
        modelBuilder.Entity<Chat>()
            .HasMany(e => e.userPrompts)
            .WithOne(e => e.chat)
            .HasForeignKey(e => e.conversation_id)
            .HasPrincipalKey(e => e.id);

        
    }
}