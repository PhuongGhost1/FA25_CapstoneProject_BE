using CusomMapOSM_Domain.Entities.Bookmarks;
using CusomMapOSM_Domain.Entities.Comments;
using CusomMapOSM_Domain.Entities.Exports;
using CusomMapOSM_Domain.Entities.Faqs;
using CusomMapOSM_Domain.Entities.Layers;
using CusomMapOSM_Domain.Entities.Locations;
using CusomMapOSM_Domain.Entities.Maps;
using CusomMapOSM_Domain.Entities.Memberships;
using CusomMapOSM_Domain.Entities.Notifications;
using CusomMapOSM_Domain.Entities.Organizations;
using CusomMapOSM_Domain.Entities.Workspaces;
using CusomMapOSM_Domain.Entities.Tickets;
using CusomMapOSM_Domain.Entities.Transactions;
using CusomMapOSM_Domain.Entities.Users;
using CusomMapOSM_Domain.Entities.Segments;
using CusomMapOSM_Domain.Entities.Timeline;
using CusomMapOSM_Domain.Entities.Zones;
using CusomMapOSM_Domain.Entities.Animations;
using CusomMapOSM_Domain.Entities.QuestionBanks;
using CusomMapOSM_Domain.Entities.Sessions;
using CusomMapOSM_Domain.Entities.Groups;
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
    public DbSet<Bookmark> Bookmarks { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<Export> Exports { get; set; }
    public DbSet<Faq> Faqs { get; set; }
    public DbSet<Layer> Layers { get; set; }
    public DbSet<Map> Maps { get; set; }
    public DbSet<MapHistory> MapHistories { get; set; }
    public DbSet<MapFeature> MapFeatures { get; set; }
    public DbSet<MapImage> MapImages { get; set; }
    public DbSet<MapReport> MapReports { get; set; }
    public DbSet<SessionQuestionBank> SessionQuestionBanks { get; set; }
    public DbSet<Segment> MapSegments { get; set; }
    public DbSet<SegmentLayer> SegmentLayers { get; set; }
    public DbSet<SegmentZone> SegmentZones { get; set; }
    public DbSet<Location> MapLocations { get; set; }
    public DbSet<AnimatedLayer> AnimatedLayers { get; set; }
    public DbSet<AnimatedLayerPreset> AnimatedLayerPresets { get; set; }
    public DbSet<TimelineTransition> TimelineTransitions { get; set; }
    public DbSet<RouteAnimation> RouteAnimations { get; set; }
    public DbSet<Zone> Zones { get; set; }
    public DbSet<MapZone> MapZones { get; set; }
    public DbSet<Membership> Memberships { get; set; }
    public DbSet<Plan> Plans { get; set; }
    public DbSet<MembershipUsage> MembershipUsages { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<Organization> Organizations { get; set; }
    public DbSet<OrganizationInvitation> OrganizationInvitations { get; set; }
    public DbSet<OrganizationMember> OrganizationMembers { get; set; }
    public DbSet<Workspace> Workspaces { get; set; }
    public DbSet<SupportTicket> SupportTickets { get; set; }
    public DbSet<SupportTicketMessage> SupportTicketMessages { get; set; }
    public DbSet<Transactions> Transactions { get; set; }
    public DbSet<PaymentGateway> PaymentGateways { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<QuestionBank> QuestionBanks { get; set; }
    public DbSet<QuestionOption> QuestionOptions { get; set; }
    public DbSet<Question> Questions { get; set; }
    public DbSet<Session> Sessions { get; set; }
    public DbSet<SessionParticipant> SessionParticipants { get; set; }
    public DbSet<StudentResponse> StudentResponses { get; set; }
    public DbSet<SessionQuestion> SessionQuestions { get; set; }
    public DbSet<SessionGroup> SessionGroups { get; set; }
    public DbSet<SessionGroupMember> SessionGroupMembers { get; set; }
    public DbSet<GroupSubmission> GroupSubmissions { get; set; }
    #endregion

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CustomMapOSMDbContext).Assembly);
    }
}
