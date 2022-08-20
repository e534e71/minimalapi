using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

var builder = WebApplication.CreateBuilder(args);

// Connect to PostgreSQL Database start
var connectionString = builder.Configuration.GetConnectionString("AppCon");

builder.Services.AddDbContext<UserLoginDb>(options => options.UseNpgsql(connectionString))
                .AddDbContext<InternshipBookDb>(options => options.UseNpgsql(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();
// Connect to PostgreSQL Database end

builder.Services.AddHealthChecks(); // Healt Check For Docker
var app = builder.Build();
app.MapHealthChecks("/health");
app.MapGet("/", () => "/users/{id:int} tamamlandý");

//____________________________ UserLogin Crud Actions Start ______________________\\
app.MapPost("/users/", async (UserLogin n, UserLoginDb db) =>
{
    db.UserLogin.Add(n);
    await db.SaveChangesAsync();
    return Results.Created($"/users/{n.Id}", n);
});

app.MapGet("/users", async (UserLoginDb db) => await db.UserLogin.ToListAsync());

app.MapGet("/users/{Id:int}", async (int Id, UserLoginDb db) =>
{
    return await db.UserLogin.FindAsync(Id)
    is UserLogin n ? Results.Ok(n) : Results.NotFound();
});

app.MapPut("/users/{Id:int}", async (int Id, UserLogin n, UserLoginDb db) =>
{
    var userlogin = await db.UserLogin.FindAsync(Id);
    if (userlogin is null) return Results.NotFound();
    userlogin.Username = n.Username;
    userlogin.Password = n.Password;
    await db.SaveChangesAsync();
    return Results.Ok(userlogin);
});

app.MapDelete("/users/{Id:int}", async (int Id, UserLoginDb db) =>
{

    var user = await db.UserLogin.FindAsync(Id);
    if (user is not null)
    {
        db.UserLogin.Remove(user);
        await db.SaveChangesAsync();
    }
    return Results.NoContent();
});

//__________________________   UserLogin Crud Actions End    _____________________\\


//_______________________   InternshipBook Crud Actions Start    _________________\\

app.MapPost("/users/books/", async (InternshipBook s, InternshipBookDb Sdb) =>
{
    Sdb.InternshipBook.Add(s);
    await Sdb.SaveChangesAsync();
    return Results.Created($"/users/books/{s.BookId}", s);
});

app.MapGet("/users/books", async (InternshipBookDb db) => await db.InternshipBook.ToListAsync());

app.MapGet("/users/books/{BookId:int}", async (int BookId, InternshipBookDb db) => 
{
    return await db.InternshipBook.FindAsync(BookId)
    is InternshipBook n ? Results.Ok(n) : Results.NotFound();
});

app.MapPut("/users/books/{Id:int}", async (int Id, InternshipBook n, InternshipBookDb db) =>
{
    var book = await db.InternshipBook.FindAsync(Id);
    if (book is null) return Results.NotFound();
    book.Note = n.Note;
    book.Subject = n.Subject;
    book.Date = n.Subject;
    book.ImageName = n.ImageName;
    await db.SaveChangesAsync();
    return Results.Ok(book);
});

app.MapDelete("/users/books/{Id:int}", async (int Id, InternshipBookDb db) =>
{

    var user = await db.InternshipBook.FindAsync(Id);
    if (user is not null)
    {
        db.InternshipBook.Remove(user);
        await db.SaveChangesAsync();
    }
    return Results.NoContent();
});


app.Run();

// model UserLogin

record UserLogin()
{
    [Key]
    [Column(Order = 1)]
    public int Id { get; set; }
    public string Username { get; set; } = default!;
    public string Password { get; set; } = default!;
}

// model InternshipBook

record InternshipBook()
{
    [Key]
    public int BookId { get; set; }

    [ForeignKey("UserLogin")]
    [Column(Order = 1)]
    public int Id{ get; set; }
    public string Date { get; set; } = string.Empty;
    public string Subject { get; set; } = default!;
    public string Note { get; set; } = default!;
    public string ImageName { get; set; } = default!;
}

// Ctor UserLoginDb

class UserLoginDb : DbContext {
    public UserLoginDb(DbContextOptions<UserLoginDb> options) : base(options) { }
    public DbSet<UserLogin> UserLogin => Set<UserLogin>();
}

// Ctor InternshipBookDb

class InternshipBookDb : DbContext {
    public InternshipBookDb(DbContextOptions<InternshipBookDb> options) : base(options) { }
    public DbSet<InternshipBook> InternshipBook => Set<InternshipBook>();
}
//