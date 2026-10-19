using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenWallet.Managers;
using OpenWallet.Shared.DTOs;

namespace OpenWallet.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class StatsController(StatsManager manager) : ControllerBase
{
    /// <summary>Returns dashboard data including accounts, recent records, and chart data.</summary>
    [HttpGet("dashboard")]
    public async Task<ActionResult<DashboardDto>> GetDashboard(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to) =>
        Ok(await manager.GetDashboardAsync(from, to));

    /// <summary>Returns expenses grouped by category for a date range.</summary>
    [HttpGet("expenses-by-category")]
    public async Task<ActionResult<List<CategoryExpenseDto>>> GetExpensesByCategory(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to) =>
        Ok(await manager.GetExpensesByCategoryAsync(from, to));

    /// <summary>Returns expenses grouped by tag for a date range.</summary>
    [HttpGet("expenses-by-tag")]
    public async Task<ActionResult<List<TagExpenseDto>>> GetExpensesByTag(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to) =>
        Ok(await manager.GetExpensesByTagAsync(from, to));

    /// <summary>Returns balance trend data for a date range.</summary>
    [HttpGet("balance-trend")]
    public async Task<ActionResult<List<BalanceTrendDto>>> GetBalanceTrend(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        DateTime now = DateTime.UtcNow;
        DateTime resolvedFrom = from.HasValue ? from.Value : now.AddDays(-30);
        DateTime resolvedTo   = to.HasValue   ? to.Value   : now;
        return Ok(await manager.GetBalanceTrendAsync(resolvedFrom, resolvedTo));
    }
}
