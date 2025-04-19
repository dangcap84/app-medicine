using Microsoft.AspNetCore.Mvc;
using MediTrack.Infrastructure.Data;
using MediTrack.Domain.Entities;

namespace MediTrack.Api.Controllers
{
    [ApiController]
    [Route("api/medicine-units")]
    public class MedicineUnitsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MedicineUnitsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public ActionResult<IEnumerable<MedicineUnitDto>> GetAll()
        {
            var units = _context.MedicineUnits
                .Select(u => new MedicineUnitDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    Description = u.Description
                })
                .ToList();

            return Ok(units);
        }
    }

    public class MedicineUnitDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
    }
}
