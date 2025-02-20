using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Selu383.SP25.P02.Api.Data;
using Selu383.SP25.P02.Api.Features.Theaters;
using System.Linq;

namespace Selu383.SP25.P02.Api.Controllers
{
    [Route("api/theaters")]
    [ApiController]
    public class TheatersController : ControllerBase
    {
        private readonly DbSet<Theater> theaters;
        private readonly DataContext dataContext;

        public TheatersController(DataContext dataContext)
        {
            this.dataContext = dataContext;
            theaters = dataContext.Set<Theater>();
        }

        [HttpGet]
        public IQueryable<TheaterDto> GetAllTheaters()
        {
            return GetTheaterDtos(theaters);
        }

        [HttpGet("{id}")]
        public ActionResult<TheaterDto> GetTheaterById(int id)
        {
            var result = GetTheaterDtos(theaters.Where(x => x.Id == id)).FirstOrDefault();
            if (result == null)
            {  
                return NotFound();
            }
            return Ok(result);
        }

        [HttpPost]
        [Authorize] // Must be logged in
        public ActionResult<TheaterDto> CreateTheater(TheaterDto dto)
        {
            // 401 if not logged in
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            // 403 if not admin
            if (!User.IsInRole("Admin"))
            {
                return Forbid();
            }

            // 400 if invalid
            if (IsInvalid(dto))
            {
                return BadRequest();
            }

            var theater = new Theater
            {
                Name = dto.Name,
                Address = dto.Address,
                SeatCount = dto.SeatCount
            };
            theaters.Add(theater);
            dataContext.SaveChanges();

            dto.Id = theater.Id;
            return CreatedAtAction(nameof(GetTheaterById), new { id = dto.Id }, dto);
        }

        [HttpPut("{id}")]
        [Authorize]
        public ActionResult<TheaterDto> UpdateTheater(int id, TheaterDto dto)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            var theater = theaters.FirstOrDefault(x => x.Id == id);
            if (theater == null)
            {
                return NotFound();
            }

            if (!User.IsInRole("Admin"))
            {
                return Forbid();
            }

            if (IsInvalid(dto))
            {
                return BadRequest();
            }

            theater.Name = dto.Name;
            theater.Address = dto.Address;
            theater.SeatCount = dto.SeatCount;
            dataContext.SaveChanges();

            dto.Id = theater.Id;
            return Ok(dto);
        }

        [HttpDelete("{id}")]
        [Authorize]
        public ActionResult DeleteTheater(int id)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            var theater = theaters.FirstOrDefault(x => x.Id == id);
            if (theater == null)
            {
                return NotFound();
            }

            if (!User.IsInRole("Admin"))
            {
                return Forbid();
            }

            theaters.Remove(theater);
            dataContext.SaveChanges();
            return Ok();
        }

        private static bool IsInvalid(TheaterDto dto)
        {
            return string.IsNullOrWhiteSpace(dto.Name)
                || dto.Name.Length > 120
                || string.IsNullOrWhiteSpace(dto.Address)
                || dto.SeatCount <= 0;
        }

        private static IQueryable<TheaterDto> GetTheaterDtos(IQueryable<Theater> theaters)
        {
            return theaters.Select(x => new TheaterDto
            {
                Id = x.Id,
                Name = x.Name,
                Address = x.Address,
                SeatCount = x.SeatCount
            });
        }
    }
}
