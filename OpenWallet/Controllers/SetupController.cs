using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenWallet.Database;
using OpenWallet.Database.Models;
using OpenWallet.Shared.DTOs;

namespace OpenWallet.Controllers;

[ApiController]
[Route("api/setup")]
public class SetupController(AppDbContext db, UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager) : ControllerBase
{
    /// <summary>Returns whether initial setup has been completed.</summary>
    [HttpGet("status")]
    public async Task<ActionResult<SetupStatusDto>> GetStatus() =>
        Ok(new SetupStatusDto { IsSetupDone = await userManager.Users.AnyAsync() });

    /// <summary>Creates the initial user and first account. Only works if no user exists yet.</summary>
    [HttpPost]
    public async Task<IActionResult> CompleteSetup(SetupDto dto)
    {
        if (await userManager.Users.AnyAsync())
            return Conflict(new { error = "Setup already completed" });

        IdentityUser user = new() { UserName = dto.Username };
        IdentityResult result = await userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            return BadRequest(new { error = result.Errors.FirstOrDefault()?.Description });

        db.Accounts.Add(new Account
        {
            Name = dto.AccountName,
            Currency = dto.AccountCurrency,
            InitialAmount = dto.InitialBalance,
            Color = dto.AccountColor
        });

        SeedCategories();
        await db.SaveChangesAsync();

        await signInManager.SignInAsync(user, isPersistent: true);
        return Ok();
    }

    private void SeedCategories()
    {
        db.Categories.AddRange(
            Parent("Food & Drinks", "bi-egg-fried", "#f97316",
                Sub("Restaurants",     "bi-shop",       "#f97316"),
                Sub("Drinks, Cafes",   "bi-cup-hot",    "#f97316"),
                Sub("Groceries",       "bi-cart3",      "#f97316")),

            Parent("Shopping", "bi-bag", "#8b5cf6",
                Sub("Home",              "bi-house",                  "#8b5cf6"),
                Sub("Clothes & Shoes",   "bi-handbag",                "#8b5cf6"),
                Sub("Kids",              "bi-balloon-heart",          "#8b5cf6"),
                Sub("Health & Beauty",   "bi-stars",                  "#8b5cf6"),
                Sub("Electronics",       "bi-cpu",                    "#8b5cf6"),
                Sub("Store Subscriptions","bi-bag-check",             "#8b5cf6")),

            Parent("Housing", "bi-house-door", "#06b6d4",
                Sub("Rent",         "bi-building",         "#06b6d4"),
                Sub("Energy",       "bi-lightning-charge", "#06b6d4"),
                Sub("Maintenance",  "bi-tools",            "#06b6d4")),

            Parent("Transportation", "bi-signpost-split", "#3b82f6",
                Sub("Taxi",             "bi-taxi-front",  "#3b82f6"),
                Sub("Airplanes",        "bi-airplane",    "#3b82f6"),
                Sub("Public Transport", "bi-bus-front",   "#3b82f6")),

            Parent("Vehicle", "bi-car-front", "#64748b",
                Sub("Fuel",              "bi-fuel-pump",    "#64748b"),
                Sub("Cleaning",          "bi-droplet",      "#64748b"),
                Sub("Vehicle Insurance", "bi-shield-check", "#64748b")),

            Parent("Life", "bi-heart-pulse", "#ec4899",
                Sub("Health Care",        "bi-hospital",  "#ec4899"),
                Sub("Trips & Activities", "bi-compass",   "#ec4899"),
                Sub("Hotels",             "bi-building",  "#ec4899"),
                Sub("Charity & Gifts",    "bi-gift",      "#ec4899"),
                Sub("Books",              "bi-book",      "#ec4899"),
                Sub("TV, Streaming",      "bi-tv",        "#ec4899")),

            Parent("Communication, PC", "bi-phone", "#6366f1",
                Sub("Phone",    "bi-phone",        "#6366f1"),
                Sub("Internet", "bi-wifi",         "#6366f1"),
                Sub("Software", "bi-code-square",  "#6366f1"),
                Sub("Games",    "bi-controller",   "#6366f1")),

            Parent("Income", "bi-cash-coin", "#22c55e",
                Sub("Wages", "bi-wallet2", "#22c55e")),

            Parent("Financial Expenses", "bi-bank", "#ef4444",
                Sub("Gov Fees",   "bi-building-fill-gear", "#ef4444"),
                Sub("Fines",      "bi-exclamation-triangle","#ef4444"),
                Sub("School Fees","bi-mortarboard",          "#ef4444"),
                Sub("Zakat",      "bi-moon-stars",           "#ef4444")),

            Parent("Investments", "bi-graph-up-arrow", "#f59e0b",
                Sub("Business", "bi-briefcase",         "#f59e0b"),
                Sub("Crypto",   "bi-currency-bitcoin",  "#f59e0b"),
                Sub("Stocks",   "bi-graph-up",          "#f59e0b")),

            Parent("Others", "bi-three-dots-vertical", "#6b7280",
                Sub("Missing",               "bi-question-circle", "#6b7280"),
                Sub("Service to Someone",    "bi-person-check",    "#6b7280"),
                Sub("Wife Allowance",        "bi-heart",           "#6b7280"))
        );
    }

    private static Category Parent(string name, string icon, string color, params Category[] children)
    {
        Category cat = new() { Name = name, Icon = icon, Color = color };
        foreach (Category child in children)
            cat.SubCategories.Add(child);
        return cat;
    }

    private static Category Sub(string name, string icon, string color) =>
        new() { Name = name, Icon = icon, Color = color };
}
