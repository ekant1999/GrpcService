using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TodoGrpc.Data;
using TodoGrpc.Models;
using ToDoGrpc;

namespace TodoGrpc.Services
{
    public class TodoService : ToDoIt.ToDoItBase
    {
        private readonly ILogger<TodoService> _logger;
        private readonly AppDbContext _dbContext;

        public TodoService(ILogger<TodoService> logger, AppDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        public override async Task<CreateToDoResponse> CreateToDo(CreateToDoRequest request, ServerCallContext context)
        {
            ValidateRequest(request.Title, request.Description);

            var todoItem = new TodoItem
            {
                Title = request.Title,
                Description = request.Description,
            };

            await _dbContext.TodoItems.AddAsync(todoItem, context.CancellationToken);
            await _dbContext.SaveChangesAsync(context.CancellationToken);

            _logger.LogInformation("Created new Todo with ID: {Id}", todoItem.Id);

            return new CreateToDoResponse { Id = todoItem.Id };
        }

        public override async Task<ReadTodoResponse> ReadTodo(ReadTodoRequest request, ServerCallContext context)
        {
            ValidateRequest(request.Id);

            var todoItem = await FindTodoByIdAsync(request.Id, context);

            return new ReadTodoResponse
            {
                Id = todoItem.Id,
                Title = todoItem.Title,
                Description = todoItem.Description,
                ToDoStatus = todoItem.TodoStatus
            };
        }

        public override async Task<GetAllResponse> ListTodo(GetAllRequest request, ServerCallContext context)
        {
            var todoItems = await _dbContext.TodoItems.ToListAsync(context.CancellationToken);

            var response = new GetAllResponse();
            foreach (var item in todoItems)
            {
                response.ToDo.Add(new ReadTodoResponse
                {
                    Id = item.Id,
                    Title = item.Title,
                    Description = item.Description,
                    ToDoStatus = item.TodoStatus
                });
            }

            return response;
        }


        public override async Task<UpdateTodoResponse> UpdateTodo(UpdateTodoRequest request, ServerCallContext context)
        {

            ValidateRequest(request.Id);
            ValidateRequest(request.Title, request.Description);

            var todoItem = await FindTodoByIdAsync(request.Id, context);

            todoItem.Title = request.Title;
            todoItem.Description = request.Description;
            todoItem.TodoStatus = request.ToDoStatus;

            await _dbContext.SaveChangesAsync(context.CancellationToken);

            _logger.LogInformation("Updated Todo with ID: {Id}", todoItem.Id);

            return new UpdateTodoResponse { Id = todoItem.Id };
        }

        public override async Task<DeleteTodoResponse> DeleteTodo(DeleteTodoRequest request, ServerCallContext context)
        {

            ValidateRequest(request.Id);

            var todoItem = await FindTodoByIdAsync(request.Id, context);

            _dbContext.TodoItems.Remove(todoItem);
            await _dbContext.SaveChangesAsync(context.CancellationToken);

            _logger.LogInformation("Deleted Todo with ID: {Id}", todoItem.Id);

            return new DeleteTodoResponse { Id = todoItem.Id };
        }


        private static void ValidateRequest(string title, string description)
        {
            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(description))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Title and description cannot be empty."));
            }
        }

        private static void ValidateRequest(int id)
        {
            if (id <= 0)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "ID must be greater than zero."));
            }
        }

        private async Task<TodoItem> FindTodoByIdAsync(int id, ServerCallContext context)
        {
            var todoItem = await _dbContext.TodoItems.FindAsync(new object[] { id }, context.CancellationToken);

            if (todoItem == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"Todo item with ID {id} not found."));
            }

            return todoItem;
        }
    }
}
