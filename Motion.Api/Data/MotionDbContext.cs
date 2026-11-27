using Microsoft.EntityFrameworkCore;

namespace Motion.Api.Data;

   public class MotionDbContext : DbContext
    {
        public MotionDbContext(DbContextOptions<MotionDbContext> options) : base(options)
        {
        }

        public DbSet<Night> Nights => Set<Night>();
        public DbSet<Member> Members => Set<Member>();
        public DbSet<Poll> Polls => Set<Poll>();
        public DbSet<Proposal> Proposals => Set<Proposal>();
        public DbSet<Vote> Votes => Set<Vote>();
        public DbSet<NightEvent> Events => Set<NightEvent>();
    
        protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Night>(e =>
    {
        e.HasIndex(n => n.Code).IsUnique();
        e.Property(n => n.Title).HasMaxLength(40);
        e.Property(n => n.CurrentSpot).HasMaxLength(60);
        e.Property(n => n.Note).HasMaxLength(120);
        e.Property(n => n.Status).HasConversion<string>();
    });

    modelBuilder.Entity<Member>(e =>
    {
        e.HasIndex(m => new { m.NightId, m.GuestId }).IsUnique();
        e.Property(m => m.Alias).HasMaxLength(20);
        e.Property(m => m.Role).HasConversion<string>();
        e.Property(m => m.Status).HasConversion<string>();
    });

    modelBuilder.Entity<Poll>()
        .Property(p => p.Status).HasConversion<string>();

    modelBuilder.Entity<Proposal>()
        .Property(p => p.Name).HasMaxLength(60);

    modelBuilder.Entity<Vote>(e =>
    {
        e.HasKey(v => new { v.PollId, v.MemberId });
        e.HasIndex(v => new { v.PollId, v.ProposalId });
    });
}
    }