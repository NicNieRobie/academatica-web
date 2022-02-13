using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Academatica.Api.Common.Data;
using Academatica.Api.Common.Models;
using Academatica.Api.Course.DTOs;
using Academatica.Api.Course.Services;
using Academatica.Api.Course.Services.Grpc;
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
        private readonly IPracticeAchievementsDataClient _practiceAchievementsDataClient;

        public CourseController(
            AcadematicaDbContext academaticaDbContext,
            IPracticeAchievementsDataClient practiceAchievementsDataClient)
        {
            _academaticaDbContext = academaticaDbContext;
            _practiceAchievementsDataClient = practiceAchievementsDataClient;
        }

        [HttpGet]
        [Route("classes/upcoming")]
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

            return Ok(new GetUpcomingClassesResponseDto
            {
                UpcomingClasses = upcomingClasses
            });
        }

        [HttpGet]
        [Route("practice/recommended")]
        public IActionResult GetRecommendedPracticeTopic()
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

            var userMistakes = _academaticaDbContext.UserTopicMistakes.Where(x => x.UserId.ToString() == userId).AsEnumerable().OrderByDescending(x => x.MistakeCount);

            if (userMistakes.Count() == 0)
            {
                var userCompletedTopics = _academaticaDbContext.UserTopic.Where(x => x.UserId.ToString() == userId).AsEnumerable();

                if (userCompletedTopics.Count() == 0)
                {
                    return Ok(new GetRecommendedPracticeTopicResponseDto()
                    {
                        RecommendedTopic = null
                    });
                }

                var randomTopicId = userCompletedTopics.OrderBy(x => Guid.NewGuid()).Select(x => x.TopicId).Take(1).SingleOrDefault();
                var topic = _academaticaDbContext.Topics.Where(x => x.Id == randomTopicId).FirstOrDefault();

                if (topic == null)
                {
                    return Ok(new GetRecommendedPracticeTopicResponseDto()
                    {
                        RecommendedTopic = null
                    });
                }

                return Ok(new GetRecommendedPracticeTopicResponseDto()
                {
                    RecommendedTopic = topic == null ? null : new TopicDto()
                    {
                        Description = topic.Description,
                        Id = topic.Id,
                        ImageUrl = topic.ImageUrl,
                        IsAlgebraTopic = topic.IsAlgebraTopic,
                        Name = topic.Name,
                        MaxProblemCount = (uint)_academaticaDbContext.Problems.Where(x => x.TopicId == topic.Id).Count()
                    }
                });
            }

            var topicId = userMistakes.Select(x => x.TopicId).First();
            var recommendedTopic = _academaticaDbContext.Topics.Where(x => x.Id == topicId).FirstOrDefault();

            if (recommendedTopic == null)
            {
                return Ok(new GetRecommendedPracticeTopicResponseDto()
                {
                    RecommendedTopic = null
                });
            }

            return Ok(new GetRecommendedPracticeTopicResponseDto()
            {
                RecommendedTopic = recommendedTopic == null ? null : new TopicDto()
                {
                    Description = recommendedTopic.Description,
                    Id = recommendedTopic.Id,
                    ImageUrl = recommendedTopic.ImageUrl,
                    IsAlgebraTopic = recommendedTopic.IsAlgebraTopic,
                    Name = recommendedTopic.Name,
                    MaxProblemCount = (uint)_academaticaDbContext.Problems.Where(x => x.TopicId == recommendedTopic.Id).Count()
                }
            });
        }

        [HttpGet]
        [Route("topics/completed")]
        public IActionResult GetCompletedTopics()
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

            var completedTopics = _academaticaDbContext.UserTopic.Where(x => x.UserId.ToString() == userId).Join(_academaticaDbContext.Topics, 
                ut => ut.TopicId,
                t => t.Id,
                (ut, t) => t).AsEnumerable();

            return Ok(completedTopics);
        }

        [HttpGet]
        [Route("classes/{id}")]
        public IActionResult GetCompletedTopics(string id)
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

            var classFound = _academaticaDbContext.Classes.Where(x => x.Id == id).FirstOrDefault();

            if (classFound == null)
            {
                return NotFound($"No class with ID {id} was found.");
            }

            var userClassStats = _academaticaDbContext.UserClasses.Where(x => x.UserId == user.Id);
            var classIsComplete = userClassStats.Where(x => x.ClassId == classFound.Id).Any();

            var classFoundIdTokens = classFound.Id.Split("-");
            var classFoundTopicId = classFoundIdTokens[0] + "-" + classFoundIdTokens[1];
            var classTopic = _academaticaDbContext.Topics.Where(x => x.Id == classFoundTopicId).FirstOrDefault();

            var topicName = classTopic == null ? null : classTopic.Name;

            var classNum = int.Parse(classFoundIdTokens[2]);

            var sameTypeClasses = _academaticaDbContext.Classes
                .Where(x => x.IsAlgebraClass == classFound.IsAlgebraClass).ToList();

            var completedSameTypeClasses = sameTypeClasses.Join(_academaticaDbContext.UserClasses,
                c => c.Id,
                uc => uc.ClassId,
                (c, uc) => c).AsEnumerable();

            var isUnlocked = false;
            var classIndex = sameTypeClasses.IndexOf(classFound);
            
            if (classIndex == 0)
            {
                isUnlocked = true;
            } else
            {
                isUnlocked = completedSameTypeClasses.Contains(sameTypeClasses.ElementAt(classIndex - 1));
            }

            return Ok(new GetClassForUserResponseDto()
            {
                Id = classFound.Id,
                Name = classFound.Name,
                Description = classFound.Description,
                IsComplete = classIsComplete,
                ImageUrl = classFound.ImageUrl,
                ProblemNum = classFound.ProblemNum,
                TheoryUrl = classFound.TheoryUrl,
                TopicName = topicName,
                IsUnlocked = isUnlocked
            });
        }

        [HttpGet]
        [Route("activity")]
        public IActionResult GetUserActivity()
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

            var activityMatrix = new Dictionary<DateTime, int>();
            var day = DateTime.Today;

            for (int diff = 0; diff < 365; diff++)
            {
                var completedClasses = _academaticaDbContext.UserClasses.Where(x => x.UserId.ToString() == userId && x.CompletedAt.Date == day.AddDays(-1 * diff).Date).Count();
                activityMatrix.Add(day.AddDays(-1 * diff).Date, completedClasses);
            }

            return Ok(new GetUserActivityResponseDbo()
            {
                ActivityMatrix = activityMatrix
            });
        }

        [HttpGet]
        [Route("classes/{id}/problems")]
        public IActionResult GetProblemsForClass(string id)
        {
            if (String.IsNullOrEmpty(id))
            {
                return BadRequest("Invalid class ID");
            }

            Regex classIdFormat = new Regex(@"^[0-9]+-[0-1]:[0-9]+-[0-9]+$");
            var idIsValid = classIdFormat.IsMatch(id);

            if (!idIsValid)
            {
                return BadRequest("Invalid class ID");
            }

            var classFound = _academaticaDbContext.Classes.Where(x => x.Id == id).FirstOrDefault();

            if (classFound == null)
            {
                return NotFound($"Class with ID {id} was not found.");
            }

            var problemList = _academaticaDbContext.Problems.Where(x => x.ClassId == id).AsEnumerable().OrderBy(x => Guid.NewGuid()).Take((int)classFound.ProblemNum);

            return Ok(new GetClassProblemsResponseDto()
            {
                Problems = problemList.Select(x => new ProblemDto()
                {
                    Id = x.Id,
                    ClassId = x.ClassId,
                    TopicId = x.TopicId,
                    CorrectAnswers = x.CorrectAnswers,
                    Description = x.Description,
                    Difficulty = x.Difficulty,
                    Expression = x.Expression,
                    ImageUrl = x.ImageUrl,
                    Options = x.Options,
                    ProblemType = x.ProblemType,
                    Task = x.Task
                })
            });
        }

        [HttpPost]
        [Route("practice/custom/problems")]
        public IActionResult GetCustomPracticeProblems(GetCustomPracticeProblemsRequestDto problemsRequestDto)
        {
            if (ModelState.IsValid)
            {
                Regex topicIdFormat = new Regex(@"^[0-9]+-[0-1]:[0-9]+$");

                if (problemsRequestDto.TopicData.Count() == 0 || problemsRequestDto.TopicData == null)
                {
                    return BadRequest("Topic data container no entries");
                }

                var problems = new List<ProblemDto>();

                foreach (var entry in problemsRequestDto.TopicData)
                {
                    var idIsValid = topicIdFormat.IsMatch(entry.Key);

                    if (!idIsValid)
                    {
                        return BadRequest($"Invalid topic ID {entry.Key}.");
                    }

                    if (entry.Value == null)
                    {
                        return BadRequest($"Invalid topic ID {entry.Key} data.");
                    }

                    var foundTopic = _academaticaDbContext.Topics.Where(x => x.Id == entry.Key).FirstOrDefault();

                    if (foundTopic == null)
                    {
                        return BadRequest($"Invalid topic ID {entry.Key}.");
                    }

                    var topicProblems = _academaticaDbContext.Problems
                        .Where(x => x.TopicId == foundTopic.Id && x.Difficulty == entry.Value.Difficulty)
                        .AsEnumerable().OrderBy(x => Guid.NewGuid()).Take(entry.Value.Count);
                    problems.AddRange(topicProblems.Select(x => new ProblemDto()
                    {
                        Id = x.Id,
                        ClassId = x.ClassId,
                        TopicId = x.TopicId,
                        CorrectAnswers = x.CorrectAnswers,
                        Description = x.Description,
                        Difficulty = x.Difficulty,
                        Expression = x.Expression,
                        ImageUrl = x.ImageUrl,
                        Options = x.Options,
                        ProblemType = x.ProblemType,
                        Task = x.Task
                    }));
                }

                return Ok(new GetCustomPracticeProblemsResponseDto()
                {
                    Problems = problems
                });
            } else
            {
                var message = string.Join(" | ", ModelState.Values
                                    .SelectMany(v => v.Errors)
                                    .Select(e => e.ErrorMessage));
                return BadRequest(message);
            }
        }

        [HttpGet]
        [Route("practice/random/problems")]
        public IActionResult GetRandomPracticeProblems()
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

            var completedTopicsIds = _academaticaDbContext.Topics.Join(_academaticaDbContext.UserTopic.Where(x => x.UserId.ToString() == userId),
                t => t.Id,
                ut => ut.TopicId,
                (t, ut) => t.Id).AsEnumerable();

            var randomProblems = _academaticaDbContext.Problems.Where(x => completedTopicsIds.Contains(x.TopicId)).AsEnumerable().OrderBy(x => Guid.NewGuid()).Take(15);

            return Ok(new GetRandomPracticeProblemsResponseDto()
            {
                Problems = randomProblems.Select(x => new ProblemDto()
                {
                    Id = x.Id,
                    ClassId = x.ClassId,
                    TopicId = x.TopicId,
                    CorrectAnswers = x.CorrectAnswers,
                    Description = x.Description,
                    Difficulty = x.Difficulty,
                    Expression = x.Expression,
                    ImageUrl = x.ImageUrl,
                    Options = x.Options,
                    ProblemType = x.ProblemType,
                    Task = x.Task
                })
            });
        }

        [HttpPost]
        [Route("classes/{classId}/finish")]
        public async Task<IActionResult> FinishClassForUser(string classId, [FromBody] FinishClassForUserRequestDto finishClassDto)
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

            var finishedClass = _academaticaDbContext.Classes.Where(x => x.Id == classId).FirstOrDefault();

            if (finishedClass == null)
            {
                return NotFound("Invalid class ID.");
            }

            var userCompletedClasses = _academaticaDbContext.UserClasses.Where(x => x.UserId == user.Id).AsEnumerable();

            if (userCompletedClasses.Select(x => x.ClassId).Contains(finishedClass.Id))
            {
                return BadRequest($"Class ID {classId} already finished.");
            }

            await _academaticaDbContext.UserClasses.AddAsync(new UserClass()
            {
                Class = finishedClass,
                ClassId = classId,
                CompletedAt = DateTime.UtcNow,
                User = user,
                UserId = user.Id
            });

            var finishedClassTopic = _academaticaDbContext.Topics.Where(x => x.Id == finishedClass.TopicId).FirstOrDefault();

            if (finishedClassTopic == null)
            {
                return NotFound("Invalid class topic.");
            }

            var userTopicMistakes = _academaticaDbContext.UserTopicMistakes.Where(x => x.TopicId == finishedClass.TopicId).FirstOrDefault();

            if (userTopicMistakes == null)
            {
                await _academaticaDbContext.UserTopicMistakes.AddAsync(new UserTopicMistake()
                {
                    User = user,
                    UserId = user.Id,
                    MistakeCount = (ulong)finishClassDto.MistakeCount,
                    Topic = finishedClassTopic,
                    TopicId = finishedClassTopic.Id
                });
            } else
            {
                userTopicMistakes.MistakeCount += (ulong)finishClassDto.MistakeCount;
            }

            var topicClassesIds = _academaticaDbContext.Classes.Where(x => x.TopicId == finishedClass.TopicId).Select(x => x.Id).ToList();
            var completedTopicClassesIds = _academaticaDbContext.UserClasses.Where(x => topicClassesIds.Contains(x.ClassId)).Select(x => x.ClassId).ToList();
            completedTopicClassesIds.Add(classId);

            if (Enumerable.SequenceEqual(topicClassesIds, completedTopicClassesIds))
            {
                await _academaticaDbContext.UserTopic.AddAsync(new UserTopic()
                {
                    User = user,
                    UserId = user.Id,
                    CompletedAt = DateTime.UtcNow,
                    Topic = finishedClassTopic,
                    TopicId = finishedClassTopic.Id
                });

                var tier = _academaticaDbContext.Tiers.Where(x => x.Id == finishedClassTopic.TierId).FirstOrDefault();

                if (tier != null)
                {
                    var tierTopicsIds = _academaticaDbContext.Topics.Where(x => x.TierId == finishedClassTopic.TierId).Select(x => x.Id).ToList();
                    var completedTierTopicsIds = _academaticaDbContext.UserTopic.Where(x => tierTopicsIds.Contains(x.TopicId)).Select(x => x.TopicId).ToList();
                    completedTierTopicsIds.Add(finishedClassTopic.Id);

                    if (Enumerable.SequenceEqual(tierTopicsIds, completedTierTopicsIds))
                    {
                        await _academaticaDbContext.UserTier.AddAsync(new UserTier()
                        {
                            User = user,
                            UserId = user.Id,
                            CompletedAt = DateTime.UtcNow,
                            Tier = tier,
                            TierId = tier.Id
                        });
                    }
                }
            }

            var statsEntry = _academaticaDbContext.UserStats.Where(x => x.UserId == user.Id).FirstOrDefault();

            if (statsEntry != null)
            {
                statsEntry.UserExp += statsEntry.UserExp + finishedClass.ExpReward <= 15000 ? finishedClass.ExpReward : 0;
                statsEntry.UserExpThisWeek += finishedClass.ExpReward;
            }

            await _academaticaDbContext.SaveChangesAsync();

            var receivedAchievements = _practiceAchievementsDataClient.GetPracticeAchievements(user.Id, classId, finishClassDto.MistakeCount);

            return Ok(new FinishClassResponseDto()
            {
                Exp = (int)finishedClass.ExpReward,
                Achievements = receivedAchievements
            });
        }

        [HttpPost]
        [Route("practice/finish")]
        public async Task<IActionResult> FinishPracticeForUser([FromBody] FinishPracticeForUserRequestDto finishPracticeDto)
        {
            if (ModelState.IsValid)
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

                var buoyAdded = false;
                var statsEntry = _academaticaDbContext.UserStats.Where(x => x.UserId == user.Id).FirstOrDefault();

                if (statsEntry != null)
                {
                    statsEntry.UserExp += statsEntry.UserExp + 50 <= 15000 ? 50u : 0;
                    statsEntry.UserExpThisWeek += 50u;

                    if (statsEntry.BuoysLeft < 5 && !finishPracticeDto.IsCustomPractice)
                    {
                        buoyAdded = true;
                        statsEntry.BuoysLeft++;
                    }
                }

                var topicId = finishPracticeDto.TopicId;

                if (!string.IsNullOrEmpty(finishPracticeDto.ClassId))
                {
                    var practiceClass = _academaticaDbContext.Classes.Where(x => x.Id == finishPracticeDto.ClassId).FirstOrDefault();

                    if (practiceClass != null)
                    {
                        topicId = practiceClass.TopicId;
                    }
                }

                if (!string.IsNullOrEmpty(finishPracticeDto.TopicId))
                {
                    var finishedPracticeTopic = _academaticaDbContext.Topics.Where(x => x.Id == finishPracticeDto.TopicId).FirstOrDefault();

                    if (finishedPracticeTopic == null)
                    {
                        return NotFound("Invalid class topic.");
                    }

                    var userTopicMistakes = _academaticaDbContext.UserTopicMistakes.Where(x => x.TopicId == finishedPracticeTopic.Id).FirstOrDefault();

                    if (userTopicMistakes != null)
                    {
                        userTopicMistakes.MistakeCount = (ulong)finishPracticeDto.MistakeCount;
                    }
                }

                await _academaticaDbContext.SaveChangesAsync();

                return Ok(new FinishPracticeResponseDto()
                {
                    Exp = 50,
                    BuoyAdded = buoyAdded
                });
            } else
            {
                var message = string.Join(" | ", ModelState.Values
                                    .SelectMany(v => v.Errors)
                                    .Select(e => e.ErrorMessage));
                return BadRequest(message);
            }
        }
    }
}
