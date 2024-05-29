using Amazon.S3;
using BaseCrud.Database;
using BaseCrud.Interfaces;
using BaseCrud.Services;
using Microsoft.EntityFrameworkCore;

internal class Program
{


    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        string connectionString = builder.Configuration.GetConnectionString("Default")
                ?? throw new InvalidOperationException("Không tìm thấy connection string \"Default\" trong appsettings.json");

        builder.Services.AddDbContext<ApplicationDbContext>(options =>
                        options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));



        string endpoint = "http://localhost:9000";
        string accessKey = "5GsafdR5a3y2Y6ReWfeI";
        string secretKey = "xjJ2IV5KqgN6SQLhOKGgNSCQkbKyj47O9MvH9gcs";

        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddSingleton<IFileService, FileService>();
        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}