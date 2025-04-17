using Xunit;
using Moq;
using MediTrack.Infrastructure.Services;
using MediTrack.Application.Interfaces;
using MediTrack.Infrastructure.Data;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using MediTrack.Application.Dtos.Auth;
using MediTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Identity;

namespace MediTrack.Tests.Unit.Services
{
    public class AuthServiceTests
    {
        private readonly Mock&lt;ApplicationDbContext&gt; _mockContext;
        private readonly Mock&lt;IConfiguration&gt; _mockConfiguration;
        private readonly Mock&lt;DbSet&lt;User&gt;&gt; _mockUserSet;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            // Mock DbSet&lt;User&gt;
            var users = new List&lt;User&gt;().AsQueryable();
            _mockUserSet = new Mock&lt;DbSet&lt;User&gt;&gt;();
            _mockUserSet.As&lt;IQueryable&lt;User&gt;&gt;().Setup(m =&gt; m.Provider).Returns(users.Provider);
            _mockUserSet.As&lt;IQueryable&lt;User&gt;&gt;().Setup(m =&gt; m.Expression).Returns(users.Expression);
            _mockUserSet.As&lt;IQueryable&lt;User&gt;&gt;().Setup(m =&gt; m.ElementType).Returns(users.ElementType);
            _mockUserSet.As&lt;IQueryable&lt;User&gt;&gt;().Setup(m =&gt; m.GetEnumerator()).Returns(users.GetEnumerator());
            _mockUserSet.Setup(m =&gt; m.AddAsync(It.IsAny&lt;User&gt;(), It.IsAny&lt;CancellationToken&gt;()))
                .Callback&lt;User, CancellationToken&gt;((u, ct) =&gt; users.ToList().Add(u));

            // Mock ApplicationDbContext
            var options = new DbContextOptionsBuilder&lt;ApplicationDbContext&gt;()
                .UseInMemoryDatabase(databaseName: $"TestDb_{System.Guid.NewGuid()}")
                .Options;
            _mockContext = new Mock&lt;ApplicationDbContext&gt;(options);
            _mockContext.Setup(c =&gt; c.Users).Returns(_mockUserSet.Object);
            _mockContext.Setup(c =&gt; c.SaveChangesAsync(It.IsAny&lt;CancellationToken&gt;())).ReturnsAsync(1);

            // Mock IConfiguration for JWT settings
            var jwtSettingsStub = new Dictionary&lt;string, string&gt; {
                {"Jwt:Key", "TestSecretKeyForUnitTestingPurposeOnly1234567890"},
                {"Jwt:Issuer", "TestIssuer"},
                {"Jwt:Audience", "TestAudience"},
                {"Jwt:DurationInMinutes", "60"}
            };
            _mockConfiguration = new Mock&lt;IConfiguration&gt;();
            _mockConfiguration.Setup(c =&gt; c.GetSection("Jwt")).Returns(new ConfigurationSectionStub("Jwt", jwtSettingsStub));
            foreach (var kvp in jwtSettingsStub)
            {
                _mockConfiguration.Setup(c =&gt; c[kvp.Key]).Returns(kvp.Value);
            }

            _authService = new AuthService(_mockContext.Object, _mockConfiguration.Object);
        }

        private class ConfigurationSectionStub : IConfigurationSection
        {
            private readonly string _key;
            private readonly Dictionary&lt;string, string&gt; _values;

            public ConfigurationSectionStub(string key, Dictionary&lt;string, string&gt; values)
            {
                _key = key;
                _values = values;
            }

            public string this[string key]
            {
                get =&gt; _values.TryGetValue($"{_key}:{key}", out var value) ? value : null;
                set =&gt; throw new NotImplementedException();
            }

            public string Key =&gt; _key;
            public string Path =&gt; _key;
            public string Value { get; set; }
            public IEnumerable&lt;IConfigurationSection&gt; GetChildren() =&gt; Enumerable.Empty&lt;IConfigurationSection&gt;();
            public IChangeToken GetReloadToken() =&gt; null;
        }

        [Fact]
        public async Task RegisterAsync_ShouldAddUser_WhenUsernameIsUnique()
        {
            // Arrange
            var registerDto = new RegisterRequestDto { Username = "newuser", Password = "password123" };

            // Act
            var result = await _authService.RegisterAsync(registerDto);

            // Assert
            result.Should().NotBeNull();
            result.Token.Should().NotBeNullOrEmpty();
            _mockUserSet.Verify(m =&gt; m.AddAsync(It.Is&lt;User&gt;(u =&gt; u.Username == registerDto.Username), It.IsAny&lt;CancellationToken&gt;()), Times.Once);
            _mockContext.Verify(m =&gt; m.SaveChangesAsync(It.IsAny&lt;CancellationToken&gt;()), Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_ShouldThrowException_WhenUsernameExists()
        {
            // Arrange
            var existingUser = new User { Username = "existinguser", PasswordHash = BCrypt.Net.BCrypt.HashPassword("password") };
            var users = new List&lt;User&gt; { existingUser }.AsQueryable();
            _mockUserSet.As&lt;IQueryable&lt;User&gt;&gt;().Setup(m =&gt; m.Provider).Returns(users.Provider);
            _mockUserSet.As&lt;IQueryable&lt;User&gt;&gt;().Setup(m =&gt; m.Expression).Returns(users.Expression);
            _mockUserSet.As&lt;IQueryable&lt;User&gt;&gt;().Setup(m =&gt; m.ElementType).Returns(users.ElementType);
            _mockUserSet.As&lt;IQueryable&lt;User&gt;&gt;().Setup(m =&gt; m.GetEnumerator()).Returns(() =&gt; users.GetEnumerator());

            var registerDto = new RegisterRequestDto { Username = "existinguser", Password = "password123" };

            // Act & Assert
            await Assert.ThrowsAsync&lt;ArgumentException&gt;(() =&gt; _authService.RegisterAsync(registerDto));
            _mockUserSet.Verify(m =&gt; m.AddAsync(It.IsAny&lt;User&gt;(), It.IsAny&lt;CancellationToken&gt;()), Times.Never);
            _mockContext.Verify(m =&gt; m.SaveChangesAsync(It.IsAny&lt;CancellationToken&gt;()), Times.Never);
        }

        [Fact]
        public async Task RegisterAsync_ShouldThrowArgumentNullException_WhenRequestIsNull()
        {
            // Arrange
            RegisterRequestDto registerDto = null;

            // Act & Assert
            await Assert.ThrowsAsync&lt;ArgumentNullException&gt;(() =&gt; _authService.RegisterAsync(registerDto));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task RegisterAsync_ShouldThrowArgumentException_WhenUsernameIsInvalid(string invalidUsername)
        {
            // Arrange
            var registerDto = new RegisterRequestDto { Username = invalidUsername, Password = "password123" };

            // Act & Assert
            var exception = await Assert.ThrowsAsync&lt;ArgumentException&gt;(() =&gt; _authService.RegisterAsync(registerDto));
            exception.ParamName.Should().Be("Username");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task RegisterAsync_ShouldThrowArgumentException_WhenPasswordIsInvalid(string invalidPassword)
        {
            // Arrange
            var registerDto = new RegisterRequestDto { Username = "newuser", Password = invalidPassword };

            // Act & Assert
            var exception = await Assert.ThrowsAsync&lt;ArgumentException&gt;(() =&gt; _authService.RegisterAsync(registerDto));
            exception.ParamName.Should().Be("Password");
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnToken_WhenCredentialsAreValid()
        {
            // Arrange
            var username = "testuser";
            var password = "password123";
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
            var user = new User { Id = 1, Username = username, PasswordHash = hashedPassword };

            var users = new List&lt;User&gt; { user }.AsQueryable();
            _mockUserSet.As&lt;IQueryable&lt;User&gt;&gt;().Setup(m =&gt; m.Provider).Returns(users.Provider);
            _mockUserSet.As&lt;IQueryable&lt;User&gt;&gt;().Setup(m =&gt; m.Expression).Returns(users.Expression);
            _mockUserSet.As&lt;IQueryable&lt;User&gt;&gt;().Setup(m =&gt; m.ElementType).Returns(users.ElementType);
            _mockUserSet.As&lt;IQueryable&lt;User&gt;&gt;().Setup(m =&gt; m.GetEnumerator()).Returns(() =&gt; users.GetEnumerator());

            var loginDto = new LoginRequestDto { Username = username, Password = password };

            // Act
            var result = await _authService.LoginAsync(loginDto);

            // Assert
            result.Should().NotBeNull();
            result.Token.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task LoginAsync_ShouldThrowArgumentNullException_WhenRequestIsNull()
        {
            // Arrange
            LoginRequestDto loginDto = null;

            // Act & Assert
            await Assert.ThrowsAsync&lt;ArgumentNullException&gt;(() =&gt; _authService.LoginAsync(loginDto));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task LoginAsync_ShouldThrowArgumentException_WhenUsernameIsInvalid(string invalidUsername)
        {
            // Arrange
            var loginDto = new LoginRequestDto { Username = invalidUsername, Password = "password123" };

            // Act & Assert
            var exception = await Assert.ThrowsAsync&lt;ArgumentException&gt;(() =&gt; _authService.LoginAsync(loginDto));
            exception.ParamName.Should().Be("Username");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task LoginAsync_ShouldThrowArgumentException_WhenPasswordIsInvalid(string invalidPassword)
        {
            // Arrange
            var loginDto = new LoginRequestDto { Username = "testuser", Password = invalidPassword };

            // Act & Assert
            var exception = await Assert.ThrowsAsync&lt;ArgumentException&gt;(() =&gt; _authService.LoginAsync(loginDto));
            exception.ParamName.Should().Be("Password");
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnNull_WhenUserNotFound()
        {
            // Arrange
            var users = new List&lt;User&gt;().AsQueryable();
            _mockUserSet.As&lt;IQueryable&lt;User&gt;&gt;().Setup(m =&gt; m.Provider).Returns(users.Provider);
            _mockUserSet.As&lt;IQueryable&lt;User&gt;&gt;().Setup(m =&gt; m.Expression).Returns(users.Expression);
            _mockUserSet.As&lt;IQueryable&lt;User&gt;&gt;().Setup(m =&gt; m.ElementType).Returns(users.ElementType);
            _mockUserSet.As&lt;IQueryable&lt;User&gt;&gt;().Setup(m =&gt; m.GetEnumerator()).Returns(() =&gt; users.GetEnumerator());

            var loginDto = new LoginRequestDto { Username = "nonexistentuser", Password = "password123" };

            // Act
            var result = await _authService.LoginAsync(loginDto);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnNull_WhenPasswordIsIncorrect()
        {
            // Arrange
            var username = "testuser";
            var correctPassword = "password123";
            var incorrectPassword = "wrongpassword";
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(correctPassword);
            var user = new User { Id = 1, Username = username, PasswordHash = hashedPassword };

            var users = new List&lt;User&gt; { user }.AsQueryable();
            _mockUserSet.As&lt;IQueryable&lt;User&gt;&gt;().Setup(m =&gt; m.Provider).Returns(users.Provider);
            _mockUserSet.As&lt;IQueryable&lt;User&gt;&gt;().Setup(m =&gt; m.Expression).Returns(users.Expression);
            _mockUserSet.As&lt;IQueryable&lt;User&gt;&gt;().Setup(m =&gt; m.ElementType).Returns(users.ElementType);
            _mockUserSet.As&lt;IQueryable&lt;User&gt;&gt;().Setup(m =&gt; m.GetEnumerator()).Returns(() =&gt; users.GetEnumerator());

            var loginDto = new LoginRequestDto { Username = username, Password = incorrectPassword };

            // Act
            var result = await _authService.LoginAsync(loginDto);

            // Assert
            result.Should().BeNull();
        }
    }
}
