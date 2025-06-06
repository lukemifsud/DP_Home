
//using BookingMicroservice.Services;
using Customer.Data;
using Customer.Services;
using Microsoft.EntityFrameworkCore;

namespace Customer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", "firestore-key4.json");

            builder.Services.AddSingleton<CustomerService>();

            //builder.Services.AddSingleton<BookingService>();
            builder.Services.AddControllers();
            builder.Services.AddHostedService<Customer.Services.NotificationSubscriber>();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            //builder.Services.AddDbContext<AppDbContext>(options =>
            // options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            //builder.Services.AddDbContext<AppDbContext>(options =>
                //options.UseInMemoryDatabase("CustomerDB"));

            //builder.Services.AddScoped<CustomerService>();

            var app = builder.Build();
            /*
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            */

            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
