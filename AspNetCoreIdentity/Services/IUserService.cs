using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AspNetCoreIdentity.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace AspNetCoreIdentity.API.Services
{
    public interface IUserService
    {
        Task<UserManagerResponce> RegisterUserAsync(RegisterViewModel registerViewModel);
        Task<UserManagerResponce> LoginUserAsync(LoginViewModel loginInfo);
    }


    public class UserService : IUserService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IConfiguration _configuration;

        public UserService(UserManager<IdentityUser> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        public async Task<UserManagerResponce> LoginUserAsync(LoginViewModel loginInfo)
        {
            var user = await _userManager.FindByEmailAsync(loginInfo.Email);
            if (user == null)
            {
                return new UserManagerResponce
                {
                    Message = "There is no user with that email !",
                    IsSuccess = false,
                };
            }
            var result = await _userManager.CheckPasswordAsync(user, loginInfo.Password);
            if(!result)
            {
                return new UserManagerResponce
                {
                    Message = "Invalid Password",
                    IsSuccess = false,
                };
            }

            var claims = new[]
            {
                new Claim("Email", loginInfo.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Authsettings:Key"]));

            var token = new JwtSecurityToken(
                issuer: _configuration["AuthSettings:Issuer"],
                audience: _configuration["AuthSettings:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(30),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

            string tokenAsString = new JwtSecurityTokenHandler().WriteToken(token);

            return new UserManagerResponce
            {
                Message = tokenAsString,
                Errors = new List<string>(),
                ExpireDate = token.ValidTo

            };
        }

        public async Task<UserManagerResponce> RegisterUserAsync(RegisterViewModel model)
        {
            if(model == null)
                throw new NullReferenceException(" Register model is null");

            if (model.Password != model.ConfirmPassword)
                return new UserManagerResponce
                {
                    Message = "Confirm password doesn't match",
                    IsSuccess = false,
                };

            var identityUser = new IdentityUser
            {
                Email = model.Email,
                UserName = model.Email
            };

            var result = await _userManager.CreateAsync(identityUser, model.Password);

            if (result.Succeeded)
            {
                return new UserManagerResponce
                {
                    Message = "User created successfully",
                    IsSuccess = true
                };
            }

            return new UserManagerResponce()
            {
                Message = "User did not create",
                IsSuccess = false,
                Errors = result.Errors.Select(e => e.Description)
            };
        }
    }
}
