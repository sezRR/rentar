using Microsoft.EntityFrameworkCore;

namespace Rentar.Shared.Infrastructure.Messaging;

public static class MessagingModelBuilderExtensions
{
    /// <summary>
    /// Maps the module's outbox and inbox into its default schema, so the module stores the events it publishes and
    /// the ones it handled in the same database, and the same transactions, as its own data.
    /// </summary>
    public static ModelBuilder MapMessaging(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OutboxMessage>(outbox =>
        {
            outbox.ToTable("OutboxMessages");
            outbox.HasKey(message => message.Id);
            outbox.Property(message => message.Type).HasMaxLength(300);
            outbox.Property(message => message.Content).HasColumnType("jsonb");

            // The processor only ever looks for messages still to be delivered, by when they are due.
            outbox.HasIndex(message => message.NextAttemptOn).HasFilter("\"ProcessedOn\" IS NULL");
        });

        modelBuilder.Entity<InboxMessage>(inbox =>
        {
            inbox.ToTable("InboxMessages");
            inbox.HasKey(message => new { message.MessageId, message.Handler });
            inbox.Property(message => message.Handler).HasMaxLength(300);
        });

        return modelBuilder;
    }
}
