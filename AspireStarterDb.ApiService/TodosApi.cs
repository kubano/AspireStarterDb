﻿using Microsoft.EntityFrameworkCore;
using AspireStarterDb.ApiDbModel;
using AspireStarterDb.ApiService;
using Microsoft.AspNetCore.Mvc;

namespace Microsoft.Extensions.Hosting;

public static class TodosApi
{
    public static IEndpointRouteBuilder MapTodosApi(this IEndpointRouteBuilder app)
    {
        var prefix = "/todos";
        var todos = app.MapGroup(prefix);

        todos.MapGet("/", (TodosDbContext db) => db.Todos.ToListAsync());

        todos.MapGet("/{id:int}", (int id, TodosDbContext db) => db.Todos.FirstOrDefaultAsync(t => t.Id == id));

        todos.MapPost("/", async (Todo todo, TodosDbContext db) =>
        {
            if (todo.Id != 0)
            {
                return Results.Problem("Id must not be specified when creating a new todo.", statusCode: StatusCodes.Status400BadRequest);
            }

            if (!ApiValidator.IsValid(todo, out var validationErrors))
            {
                return Results.ValidationProblem(validationErrors);
            }

            db.Todos.Add(todo);
            await db.SaveChangesAsync();

            return Results.Created($"{prefix}/{todo.Id}", todo);
        });

        todos.MapPut("/{id:int}", async (int id, Todo todo, TodosDbContext db) =>
        {
            if (id != todo.Id)
            {
                return Results.Problem($"Id in the path ({id}) does not match the Id in the body ({todo.Id}).",
                    statusCode: StatusCodes.Status400BadRequest);
            }

            if (!ApiValidator.IsValid(todo, out var validationErrors))
            {
                return Results.ValidationProblem(validationErrors);
            }

            var affected = await db.Todos
                .Where(model => model.Id == id)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(t => t.Title, todo.Title)
                    .SetProperty(t => t.IsComplete, todo.IsComplete)
                );

            return affected == 1 ? Results.NoContent() : Results.NotFound();
        });

        todos.MapDelete("/{id:int}", async (int id, TodosDbContext db) =>
        {
            var affected = await db.Todos
                .Where(todo => todo.Id == id)
                .ExecuteDeleteAsync();

            return affected == 1 ? Results.NoContent() : Results.NotFound();
        });

        return app;
    }
}
