using AspNetCoreIdentity.Shared;
using Microsoft.AspNetCore.Identity;

namespace AspNetCoreIdentity.API.Services
{
    public interface IUserService
    {
        Task<UserManagerResponce> RegisterUserAsync(RegisterViewModel registerViewModel);

    }


    public class UserService : IUserService
    {
        private readonly UserManager<IdentityUser> _userManager;

        public UserService(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
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
