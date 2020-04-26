using System;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using AuthenticationSampleWebApp.DTOs;
using AuthenticationSampleWebApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace AuthenticationSampleWebApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EnrollmentsController : ControllerBase
    {
        
        private IConfiguration Configuration;
        private StudentDbService _service;

        public EnrollmentsController( IConfiguration configuration, StudentDbService service)
        {
            
            Configuration = configuration;
            _service = service;
        }
        [HttpPost]
        [Authorize(Roles = "Employee")]
        public IActionResult EnrollStudent(EnrollStudentReq request)
        {
            
                return Ok(_service.EnrollStudent(request));

        }

        [HttpPost("promotions")]
        [Authorize(Roles = "Employee")]
        public IActionResult PromoteStudent(PromoteStudentReq request)
            
        {
            var sem = request.Semester + 1;
            var res = _service.PromoteStudents(sem, request.Studies);
            
            return Ok(res);
          
        }
        [HttpGet("login")]
        public IActionResult Login(LoginRequestDto req)
        {
            if (!_service.CheckPassword(req))
                return null;

            var claims = _service.GetClaims(req.Login);
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: "Gakko",
                audience: "Students",
                claims: claims,
                expires: DateTime.Now.AddMinutes(10),
                signingCredentials: creds
                );
            var refreshToken = Guid.NewGuid();
            _service.SetRefreshToken(refreshToken.ToString(), req.Login);
            return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token), refreshToken });
        }

        [HttpPost("refresh-token/{Reftoken}")]
        public IActionResult RefreshToken(string reftoken)
        {
            var user = _service.CheckRefreshToken(reftoken);
            if (user == null)
                return null;

            var claims = _service.GetClaims(user);
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var newToken = new JwtSecurityToken(
                issuer: "Gakko",
                audience: "Students",
                claims: claims,
                expires: DateTime.Now.AddMinutes(5),
                signingCredentials: creds
                );
            var refreshToken = Guid.NewGuid();
            _service.SetRefreshToken(refreshToken.ToString(), user);
            return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(newToken), refreshToken });



        }

       
    }


}
