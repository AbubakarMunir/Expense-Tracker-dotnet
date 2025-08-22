using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDb>(opt =>
    opt.UseSqlite("Data Source=expenses.db"));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDb>();
    db.Database.EnsureCreated();
}

app.MapGet("/expenses", async (AppDb db) =>
    await db.Expenses.OrderByDescending(e => e.Date).ToListAsync());

app.MapPost("/expenses", async (Expense e, AppDb db) =>
{
    db.Expenses.Add(e);
    await db.SaveChangesAsync();
    return Results.Created($"/expenses/{e.Id}", e);
});

app.MapGet("/health", () => "ok");

app.Run();

public class Expense
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public string Category { get; set; } = "";
    public double Amount { get; set; }
    public string Notes { get; set; } = "";
}

public class AppDb : DbContext
{
    public AppDb(DbContextOptions<AppDb> options) : base(options) { }
    public DbSet<Expense> Expenses => Set<Expense>();
    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<Expense>().Property(e => e.Id).ValueGeneratedOnAdd();
    }
}
