using CusomMapOSM_Domain.Entities.Advertisements;
using CusomMapOSM_Domain.Entities.Bookmarks;
using CusomMapOSM_Domain.Entities.Collaborations;
using CusomMapOSM_Domain.Entities.Comments;
using CusomMapOSM_Domain.Entities.Exports;
using CusomMapOSM_Domain.Entities.Faqs;
using CusomMapOSM_Domain.Entities.Layers;
using CusomMapOSM_Domain.Entities.Locations;
using CusomMapOSM_Domain.Entities.Maps;
using CusomMapOSM_Domain.Entities.Memberships;
using CusomMapOSM_Domain.Entities.Notifications;
using CusomMapOSM_Domain.Entities.Organizations;
using CusomMapOSM_Domain.Entities.Tickets;
using CusomMapOSM_Domain.Entities.Transactions;
using CusomMapOSM_Domain.Entities.Users;
using CusomMapOSM_Domain.Entities.Segments;
using CusomMapOSM_Domain.Entities.Timeline;
using CusomMapOSM_Domain.Entities.Zones;
using CusomMapOSM_Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace CusomMapOSM_Infrastructure.Databases;

public class CustomMapOSMDbContext : DbContext
{
    public CustomMapOSMDbContext(DbContextOptions<CustomMapOSMDbContext> options) : base(options) { }

    public CustomMapOSMDbContext()
    {
    }

    // DbSet properties for your entities here
    #region DbSet Properties
    public DbSet<Advertisement> Advertisements { get; set; }
    public DbSet<Bookmark> Bookmarks { get; set; }
    public DbSet<DataSourceBookmark> DataSourceBookmarks { get; set; }
    public DbSet<Collaboration> Collaborations { get; set; }
    public DbSet<CollaborationPermission> CollaborationPermissions { get; set; }
    public DbSet<CollaborationTargetType> CollaborationTargetTypes { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<Export> Exports { get; set; }
    public DbSet<ExportType> ExportTypes { get; set; }
    public DbSet<Faq> Faqs { get; set; }
    public DbSet<Layer> Layers { get; set; }
    public DbSet<Map> Maps { get; set; }
    public DbSet<MapHistory> MapHistories { get; set; }
    public DbSet<MapFeature> MapFeatures { get; set; }
    public DbSet<MapImage> MapImages { get; set; }
    public DbSet<Segment> MapSegments { get; set; }
    public DbSet<SegmentZone> MapSegmentZones { get; set; }
    public DbSet<Location> MapLocations { get; set; }
    public DbSet<SegmentLayer> MapSegmentLayers { get; set; }
    public DbSet<SegmentTransition> SegmentTransitions { get; set; }
    public DbSet<LayerAnimationPreset> LayerAnimationPresets { get; set; }
    public DbSet<TimelineStep> TimelineSteps { get; set; }
    public DbSet<TimelineStepLayer> TimelineStepLayers { get; set; }
    public DbSet<Zone> AdministrativeZones { get; set; }
    public DbSet<ZoneSelection> MapZoneSelections { get; set; }
    public DbSet<ZoneStatistic> ZoneStatistics { get; set; }
    public DbSet<ZoneInsight> ZoneInsights { get; set; }
    public DbSet<Membership> Memberships { get; set; }
    public DbSet<Plan> Plans { get; set; }
    public DbSet<MembershipUsage> MembershipUsages { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<Organization> Organizations { get; set; }
    public DbSet<OrganizationInvitation> OrganizationInvitations { get; set; }

    public DbSet<OrganizationMember> OrganizationMembers { get; set; }
    public DbSet<OrganizationMemberType> OrganizationMemberTypes { get; set; }
    public DbSet<SupportTicket> SupportTickets { get; set; }
    public DbSet<SupportTicketMessage> SupportTicketMessages { get; set; }
    public DbSet<Transactions> Transactions { get; set; }
    public DbSet<PaymentGateway> PaymentGateways { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    
    #endregion

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CustomMapOSMDbContext).Assembly);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        optionsBuilder.EnableSensitiveDataLogging();
        optionsBuilder.EnableDetailedErrors();
    }
}
