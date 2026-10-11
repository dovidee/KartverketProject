using KartverketProject.Controllers;
using KartverketProject.Dtos;
using KartverketProject.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Security.Claims;
using Xunit;
using Assert = Xunit.Assert;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

public class ControllerTests
{
    // sjekk at dataform lagrer hindre
    [Fact]
    public async Task ObstacleController_PostDataForm_SavesObstacle()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("TestDb_Save")
            .Options;

        var context = new ApplicationDbContext(options);
        var store = new Mock<IUserStore<User>>();
        var mockUserManager = new Mock<UserManager<User>>(store.Object, null, null, null, null, null, null, null, null);
        mockUserManager.Setup(u => u.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("test-user-id");

        var controller = new ObstacleController(context, mockUserManager.Object);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, "test-user-id")
                }, "mock"))
            }
        };

        var obstacleData = new Obstacle
        {
            ObstacleName = "john",
            ObstacleHeight = 5.5,
            ObstacleDescription = "test",
            ObstacleSubmittedDate = new DateTime(2025, 10, 15, 14, 30, 0),
            ObstacleJSON = "1"
        };

        // Act
        var result = await controller.DataForm(obstacleData);

        // Assert
        var savedObstacle = await context.Obstacle.FirstOrDefaultAsync(o => o.ObstacleName == "john");
        Assert.NotNull(result);
        Assert.NotNull(savedObstacle);
        Assert.Equal("john", savedObstacle.ObstacleName);
    }

    // sjekk at login funker
    [Fact]
    public async Task AccountController_Login_SuccessfulRedirect()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("TestDb_User")
            .Options;
        var context = new ApplicationDbContext(options);

        var userStore = new Mock<IUserStore<User>>();
        var mockUserManager = new Mock<UserManager<User>>(userStore.Object, null, null, null, null, null, null, null, null);
        var mockSignInManager = new Mock<SignInManager<User>>(
            mockUserManager.Object,
            new Mock<IHttpContextAccessor>().Object,
            new Mock<IUserClaimsPrincipalFactory<User>>().Object,
            null, null, null, null
        );

        var testUser = new User { Id = "1", UserName = "alpha", Email = "alpha@example.com" };
        mockUserManager.Setup(x => x.FindByNameAsync("alpha")).ReturnsAsync(testUser);
        mockSignInManager.Setup(x => x.PasswordSignInAsync("alpha", "alpha123", false, true))
            .ReturnsAsync(SignInResult.Success);

        var controller = new AccountController(mockUserManager.Object, mockSignInManager.Object, context);

        var loginModel = new LoginRequest
        {
            UserName = "alpha",
            Password = "alpha123"
        };

        // Act
        var result = await controller.Login(loginModel);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("DataForm", redirectResult.ActionName);
        Assert.Equal("Obstacle", redirectResult.ControllerName);
    }

    // sjekk at register funker
    [Fact]
    public async Task AccountController_Register_SuccessfulRedirect()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("TestDb_Register")
            .Options;
        var context = new ApplicationDbContext(options);

        var userStore = new Mock<IUserStore<User>>();
        var mockUserManager = new Mock<UserManager<User>>(userStore.Object, null, null, null, null, null, null, null, null);
        var mockSignInManager = new Mock<SignInManager<User>>(
            mockUserManager.Object,
            new Mock<IHttpContextAccessor>().Object,
            new Mock<IUserClaimsPrincipalFactory<User>>().Object,
            null, null, null, null
        );

        mockUserManager.Setup(u => u.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        mockUserManager.Setup(u => u.AddToRoleAsync(It.IsAny<User>(), "user"))
            .ReturnsAsync(IdentityResult.Success);
        mockSignInManager.Setup(s => s.SignInAsync(It.IsAny<User>(), false, null))
            .Returns(Task.CompletedTask);

        var controller = new AccountController(mockUserManager.Object, mockSignInManager.Object, context);

        var registerModel = new RegisterRequest
        {
            UserName = "beta",
            Email = "beta@example.com",
            Password = "Pass123!"
        };

        // Act
        var result = await controller.Register(registerModel);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("DataForm", redirectResult.ActionName);
        Assert.Equal("Obstacle", redirectResult.ControllerName);
    }
}
