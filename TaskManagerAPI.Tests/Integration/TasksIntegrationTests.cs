using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using TaskManagerAPI.Data;
using TaskManagerAPI.Models.DTOs;

namespace TaskManagerAPI.Tests.Integration
{
    public class TasksIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly WebApplicationFactory<Program> _app;

        public TasksIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _app = factory.WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
                builder.ConfigureAppConfiguration((context, config) =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["Jwt:SecretKey"] = "test-secret-key-min-32-characters-long!!",
                        ["Jwt:Issuer"] = "TaskManagerAPI",
                        ["Jwt:Audience"] = "TaskManagerClient",
                        ["Jwt:ExpireMinutes"] = "15",
                        ["Jwt:RefreshTokenExpireDays"] = "7"
                    });
                });
                builder.ConfigureServices(services =>
                {
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                    if (descriptor != null) services.Remove(descriptor);

                    // Sabit DB adı kullan
                    services.AddDbContext<AppDbContext>(options =>
                        options.UseInMemoryDatabase("IntegrationTestDb"));
                });
            });

            _client = _app.CreateClient();
        }

        private void ResetDatabase()
        {
            using var scope = _app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Users.RemoveRange(db.Users);
            db.Tasks.RemoveRange(db.Tasks);
            db.SaveChanges();
        }

        [Fact]
        public async Task GetTasks_WithoutToken_ShouldReturn401()
        {
            var response = await _client.GetAsync("/api/tasks");
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Register_WithValidData_ShouldReturn200()
        {
            ResetDatabase();
            var dto = new { username = "testuser", password = "test123" };
            var response = await _client.PostAsJsonAsync("/api/auth/register", dto);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Register_WithDuplicateUsername_ShouldReturn400()
        {
            ResetDatabase();
            var dto = new { username = "duplicateuser", password = "test123" };
            await _client.PostAsJsonAsync("/api/auth/register", dto);
            var response = await _client.PostAsJsonAsync("/api/auth/register", dto);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Login_WithValidCredentials_ShouldReturnTokens()
        {
            ResetDatabase();
            var dto = new { username = "loginuser", password = "test123" };
            await _client.PostAsJsonAsync("/api/auth/register", dto);
            var response = await _client.PostAsJsonAsync("/api/auth/login", dto);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<TokenDto>();
            result!.AccessToken.Should().NotBeNullOrEmpty();
            result.RefreshToken.Should().NotBeNullOrEmpty();
        }
    }
}