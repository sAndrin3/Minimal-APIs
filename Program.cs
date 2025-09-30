using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using MinimalApi.Context;
using MinimalApi.Entities;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// builder.Services.AddOutputCache();
builder.Services.AddStackExchangeRedisOutputCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
});

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer("name=defaultConnection"));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Minimal API");

        // Option 1:
        // if (app.Environment.IsDevelopment())
        // {
        //     options.EnablePersistAuthorization();
        // }

        // Option 2:
#if DEBUG
        options.EnablePersistAuthorization();
#endif
    });
}

app.UseHttpsRedirection();

app.UseOutputCache();

app.MapGet("/configuration",
    (IConfiguration configuration) => configuration.GetValue<string>("secret") ?? "No secret was found");

app.MapGet("/people", async (AppDbContext context) =>
{
    var people = await context.People.ToListAsync();
    return TypedResults.Ok(people);
}).CacheOutput(c => c.Expire(TimeSpan.FromSeconds(60)).Tag("people-get"));

app.MapGet("/people/{id:int}", async Task<Results<Ok<Person>, NotFound>> (int id, AppDbContext context) =>
{
    var person = await context.People.FirstOrDefaultAsync(p => p.Id == id);

    if (person == null)
    {
        return TypedResults.NotFound();
    }

    return TypedResults.Ok(person);
}).WithName("GetPerson");

app.MapPost("/people", async (
    Person person,
    AppDbContext context,
    IOutputCacheStore outputCacheStore
) =>
{
    context.Add(person);
    await context.SaveChangesAsync();
    await outputCacheStore.EvictByTagAsync("people-get", default);
    return TypedResults.CreatedAtRoute(person, "GetPerson", new { id = person.Id });
});

app.MapPut("/people/{id:int}", async Task<Results<BadRequest<string>, NotFound, NoContent>>
(int id, Person person,
    AppDbContext context,
    IOutputCacheStore outputCacheStore) =>
{
    if (id != person.Id)
    {
        return TypedResults.BadRequest("The id does not match");
    }

    var exists = await context.People.AnyAsync(p => p.Id == id);

    if (!exists)
    {
        return TypedResults.NotFound();
    }

    context.Update(person);
    await context.SaveChangesAsync();
    await outputCacheStore.EvictByTagAsync("people-get", default);
    return TypedResults.NoContent();
});

app.MapDelete("/people/{id:int}", async Task<Results<NotFound, NoContent>> (
    int id,
    AppDbContext context,
    IOutputCacheStore outputCacheStore) =>
{
    var deletedRecords = await context.People.Where(p => p.Id == id).ExecuteDeleteAsync();

    if (deletedRecords == 0)
    {
        return TypedResults.NotFound();
    }

    await outputCacheStore.EvictByTagAsync("people-get", default);

    return TypedResults.NoContent();
});

app.Run();