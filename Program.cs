using GrpcService.Services;
using Microsoft.EntityFrameworkCore;
using TodoGrpc.Data;
using TodoGrpc.Services;
using Microsoft.AspNetCore.Grpc.JsonTranscoding;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddGrpc()
                .AddJsonTranscoding();

builder.Services.AddGrpcReflection();

var app = builder.Build();

app.MapGrpcService<GreeterService>();
app.MapGrpcService<TodoService>();

if (app.Environment.IsDevelopment())
{
    app.MapGrpcReflectionService();
}

// ?? Fallback HTTP endpoint
app.MapGet("/", () =>
    "Use a gRPC client or REST client (with JSON transcoding) to communicate with this server. Docs: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
