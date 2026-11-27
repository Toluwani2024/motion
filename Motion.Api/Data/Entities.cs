namespace Motion.Api.Data;

public enum NightStatus { Live, Ended }
public enum MemberRole { Guest, Host }
public enum MemberStatus { Coming, Maybe, StayingPut, HomeSafe }
public enum PollStatus { Open, Closed }

public class Night
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Code { get; set; }
    public required string Title { get; set; }
    public string? Vibe { get; set; }
    public string? Note { get; set; }
    public string? CurrentSpot { get; set; }
    public string? MeetTime { get; set; }
    public Guid HostGuestId { get; set; }
    public NightStatus Status { get; set; } = NightStatus.Live;
    public bool LocationSharing { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastActivityAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddHours(12);

    public List<Member> Members { get; set; } = [];
    public List<Poll> Polls { get; set; } = [];
    public List<NightEvent> Events { get; set; } = [];
}

public class Member
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid NightId { get; set; }
    public Night? Night { get; set; }
    public Guid GuestId { get; set; }
    public required string Alias { get; set; }
    public string Icon { get; set; } = "⚡";
    public MemberRole Role { get; set; } = MemberRole.Guest;
    public MemberStatus Status { get; set; } = MemberStatus.Coming;
    public DateTime? HomeAt { get; set; }
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
}

public class Poll
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid NightId { get; set; }
    public Night? Night { get; set; }
    public PollStatus Status { get; set; } = PollStatus.Open;
    public DateTime ClosesAt { get; set; }
    public Guid? WinnerProposalId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<Proposal> Proposals { get; set; } = [];
    public List<Vote> Votes { get; set; } = [];
}

public class Proposal
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PollId { get; set; }
    public Poll? Poll { get; set; }
    public Guid NightId { get; set; }
    public required string Name { get; set; }
    public string? Area { get; set; }
    public string? Note { get; set; }
    public Guid ProposedByMemberId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Vote
{
    public Guid PollId { get; set; }
    public Poll? Poll { get; set; }
    public Guid MemberId { get; set; }
    public Guid ProposalId { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class NightEvent
{
    public long Id { get; set; }
    public Guid NightId { get; set; }
    public Night? Night { get; set; }
    public Guid? MemberId { get; set; }
    public required string Kind { get; set; }
    public string? PayloadJson { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}