using Microsoft.EntityFrameworkCore;
using ChatApp.Domain.Entities;
using ChatApp.Domain.Common;
using MediatR;

namespace ChatApp.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
   
    private readonly IPublisher _publisher;
    public DbSet<User> Users => Set<User>();
    public DbSet<ChatRoom> ChatRooms => Set<ChatRoom>();
    public DbSet<Message> Messages => Set<Message>();
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IPublisher publisher) : base(options)
    {
         _publisher = publisher;
    }
    public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        var domainEntities = ChangeTracker.Entries<BaseEntity>()
            .Where(x => x.Entity.DomainEvents.Any())
            .Select(x => x.Entity)
            .ToList();

        var domainEvents = domainEntities
            .SelectMany(x => x.DomainEvents)
            .ToList();

        domainEntities.ForEach(x => x.ClearDomainEvents());

        var result = await base.SaveChangesAsync(ct);

        foreach (var domainEvent in domainEvents)
        {
            await _publisher.Publish(domainEvent, ct);
        }

        return result;
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
                {
                    property.SetColumnType("timestamp with time zone");
                }
            }
        }

        base.OnModelCreating(modelBuilder);
    }
}