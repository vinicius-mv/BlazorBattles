﻿using BlazorBattles.Server.Data;
using BlazorBattles.Shared;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BlazorBattles.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _authRepo;

        public AuthController(IAuthRepository authRepo)
        {
            _authRepo = authRepo;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegister request)
        {
            var response = await _authRepo.RegisterAsync(
                new User
                {
                    Username = request.Username,
                    Email = request.Email,
                    Bananas = request.Bananas,
                    DateOfBirth = request.DateOfBirth,
                    IsConfirmed = request.IsConfirmed,
                }, 
                request.Password,
                request.StartUnitId);

            if (!response.IsSuccess)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLogin request)
        {
            var response = await _authRepo.LoginAsync(request.Email, request.Password);

            if (!response.IsSuccess)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }
    }
}
