using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Orders.Api.Entities;
using Polly;
// using Polly;

namespace Orders.Api.Persistence
{
    public class OrderContext : DbContext
    {
        public DbSet<Order> Order { get; set; }

        public OrderContext(DbContextOptions options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            var converter = new EnumToStringConverter<Status>();
            builder
                .Entity<Order>()
                .Property(p => p.Status)
                .HasConversion(converter);
        }

        public void MigrateDb()
        {
            Policy
                .Handle<Exception>()
                .WaitAndRetry(10, r => TimeSpan.FromSeconds(10))
                .Execute(() => Database.Migrate());
        }
    }
}