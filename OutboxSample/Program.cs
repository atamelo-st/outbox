using Autofac;
using Autofac.Extensions.DependencyInjection;
using OutboxSample.Application;
using OutboxSample.Model;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());

builder.Host.ConfigureContainer<ContainerBuilder>(RegisterDependencies);

builder.Services.AddControllers();

var app = builder.Build();

app.UseAuthorization();

app.MapControllers();

app.Run();


static void RegisterDependencies(ContainerBuilder containerBuilder)
{

}

