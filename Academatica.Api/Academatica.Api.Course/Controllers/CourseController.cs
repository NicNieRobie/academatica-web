using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Academatica.Api.Common.Data;
using Academatica.Api.Common.Models;
using Academatica.Api.Course.DTOs;
using Academatica.Api.Course.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Academatica.Api.Course.Controllers
{
    [ApiController]
    [Route("api/course")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class CourseController : ControllerBase
    {
        private readonly AcadematicaDbContext _academaticaDbContext;

        public CourseController(AcadematicaDbContext academaticaDbContext)
        {
            _academaticaDbContext = academaticaDbContext;
        }

        [HttpGet]
        [Route("upcoming")]
        public IActionResult GetUpcomingLessons()
        {
            var userId = User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("User ID was null.");
            }

            var user = _academaticaDbContext.Users.Where(x => x.Id.ToString() == userId).FirstOrDefault();

            if (user == null)
            {
                return NotFound("Invalid user ID.");
            }

            var algebraClasses = _academaticaDbContext.Classes
                .Where(x => x.IsAlgebraClass)
                .ToList()
                .OrderBy(x => x.Id, new ClassIdComparer());
            var geometryClasses = _academaticaDbContext.Classes
                .Where(x => !x.IsAlgebraClass)
                .ToList()
                .OrderBy(x => x.Id, new ClassIdComparer());

            List<Class> upcomingClasses = new List<Class>();

            foreach (var alClass in algebraClasses)
            {
                var isFinished = _academaticaDbContext.UserClasses
                    .Where(x => x.UserId.ToString() == userId && x.ClassId == alClass.Id)
                    .FirstOrDefault() != null;

                if (!isFinished)
                {
                    upcomingClasses.Add(alClass);
                    break;
                }
            }

            foreach (var geomClass in geometryClasses)
            {
                var isFinished = _academaticaDbContext.UserClasses
                    .Where(x => x.UserId.ToString() == userId && x.ClassId == geomClass.Id)
                    .FirstOrDefault() != null;

                if (!isFinished)
                {
                    upcomingClasses.Add(geomClass);
                    break;
                }
            }

            return Ok(new GetUpcomingClassesRequestDto
            {
                UpcomingClasses = upcomingClasses
            });
        }

        //[HttpGet]
        //[Route("practice/recommended")]
        //public IActionResult GetRecommendedPracticeTopic()
        //{

        //}
    }
}
