using System.Text.Json.Serialization;

namespace TopacioServer.Layout
{
    public record Todo(int Id, string? Title, DateOnly? DueBy = null, bool IsComplete = false);

    public class TodoSample
    {
        public Todo[] sampleTodos = new Todo[] {
        new(1, "Walk the dog"),
        new(2, "Do the dishes", DateOnly.FromDateTime(DateTime.Now)),
        new(3, "Do the laundry", DateOnly.FromDateTime(DateTime.Now.AddDays(1))),
        new(4, "Clean the bathroom"),
        new(5, "Clean the car", DateOnly.FromDateTime(DateTime.Now.AddDays(2)))
        };

        public TodoSample(WebApplication app)
        {
            RouteGroupBuilder? todosApi = app.MapGroup("/todos");
            todosApi.MapGet("/", () => this.sampleTodos);
            todosApi.MapGet("/{id}", (int id) =>
                this.sampleTodos.FirstOrDefault(a => a.Id == id) is { } todo
                    ? Results.Ok(todo)
                    : Results.NotFound());
        }        
    }

    [JsonSerializable(typeof(Todo[]))]
    internal partial class AppJsonSerializerContext : JsonSerializerContext
    {

    }

}
