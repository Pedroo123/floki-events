using FlokiEvents.Consumer;
using FlokiEvents.Infrastructure;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddFlokiInfrastructure(builder.Configuration);

//add worker
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
