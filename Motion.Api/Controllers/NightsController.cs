using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Motion.Api.Data;
using Motion.Api.Services;

namespace Motion.Api.Controllers;
public record CreateNightRequest(string Title, string? Vibe, string? Spot, string? MeetTime,
                                 string? Note, string Alias, string? Icon);
public record JoinNightRequest(string Alias, string? Icon);

[ApiController]
[Route("api/nights")]
public class NightsController(MotionDbContext db, GuestTokenService tokens) : ControllerBase
{
    private const string CodeChars = "abcdefghjkmnpqrstuvwxyz23456789";

    // POST /api/nights : start a night, become its host
    [HttpPost]
    public async Task<IActionResult> Create(CreateNightRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Title) || string.IsNullOrWhiteSpace(req.Alias))
            return BadRequest(new { error = "Title and alias are required." });

        var guestId = Guid.NewGuid();
        var night = new Night
        {
            Code = await NewCode(),
            Title = Clip(req.Title, 40)!,
            Vibe = Clip(req.Vibe, 30),
            CurrentSpot = Clip(req.Spot, 60),
            MeetTime = Clip(req.MeetTime, 10),
            Note = Clip(req.Note, 120),
            HostGuestId = guestId
        };
        var member = new Member
        {
            NightId = night.Id,
            GuestId = guestId,
            Alias = Clip(req.Alias, 20)!,
            Icon = Clip(req.Icon, 8) ?? "⚡",
            Role = MemberRole.Host
        };

        db.Nights.Add(night);
        db.Members.Add(member);
        db.Events.Add(new NightEvent { NightId = night.Id, MemberId = member.Id, Kind = "night_created" });
        await db.SaveChangesAsync();

        return Ok(new { nightId = night.Id, code = night.Code, memberId = member.Id,
                        token = tokens.Create(guestId) });
    }

    // POST /api/nights/{code}/join : a friend joins with an alias
    [HttpPost("{code}/join")]
    public async Task<IActionResult> Join(string code, JoinNightRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Alias))
            return BadRequest(new { error = "Alias is required." });

        var c = code.Trim().ToLowerInvariant();
        var night = await db.Nights.FirstOrDefaultAsync(n =>
            n.Code == c && n.Status == NightStatus.Live && n.ExpiresAt > DateTime.UtcNow);
        if (night is null)
            return NotFound(new { error = "Night not found or already ended." });

        var guestId = Guid.NewGuid();
        var member = new Member
        {
            NightId = night.Id,
            GuestId = guestId,
            Alias = Clip(req.Alias, 20)!,
            Icon = Clip(req.Icon, 8) ?? "⚡"
        };

        db.Members.Add(member);
        db.Events.Add(new NightEvent { NightId = night.Id, MemberId = member.Id, Kind = "member_joined" });
        night.LastActivityAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        return Ok(new { nightId = night.Id, memberId = member.Id, token = tokens.Create(guestId) });
    }

    // GET /api/nights/{code} : members only (proves the token works)
    [Authorize]
    [HttpGet("{code}")]
    public async Task<IActionResult> Get(string code)
    {
        var guestId = CurrentGuestId();
        if (guestId is null) return Unauthorized();

        var c = code.Trim().ToLowerInvariant();
        var night = await db.Nights.Include(n => n.Members).FirstOrDefaultAsync(n => n.Code == c);
        if (night is null || !night.Members.Any(m => m.GuestId == guestId))
            return NotFound(new { error = "Night not found." });

        return Ok(new
        {
            night.Id, night.Code, night.Title, night.Vibe,
            spot = night.CurrentSpot, night.MeetTime, night.Note,
            status = night.Status.ToString(), night.ExpiresAt,
            members = night.Members.Select(m => new
            {
                m.Id, m.Alias, m.Icon,
                role = m.Role.ToString(), status = m.Status.ToString()
            })
        });
    }

    private Guid? CurrentGuestId()
    {
        var sub = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(sub, out var id) ? id : null;
    }

    private async Task<string> NewCode()
    {
        while (true)
        {
            var code = new string(Enumerable.Range(0, 6)
                .Select(_ => CodeChars[Random.Shared.Next(CodeChars.Length)]).ToArray());
            if (!await db.Nights.AnyAsync(n => n.Code == code)) return code;
        }
    }

    private static string? Clip(string? s, int max)
    {
        s = s?.Trim();
        if (string.IsNullOrEmpty(s)) return null;
        return s.Length <= max ? s : s[..max];
    }
}