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
using Academatica.Api.Users.DTOs;
using Academatica.Api.Users.Services.RabbitMQ;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace Academatica.Api.Course.Controllers
{
    /// <summary>
    /// Controller responsible for handling requests related to the Academatica course.
    /// </summary>
    [ApiController]
    [Route("api/course")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class CourseController : ControllerBase
    {
        private readonly AcadematicaDbContext _academaticaDbContext;
        private readonly IPracticeAchievementsDataClient _practiceAchievementsDataClient;
        private readonly IMessageBusClient _messageBusClient;

        public CourseController(
            AcadematicaDbContext academaticaDbContext,
            IPracticeAchievementsDataClient practiceAchievementsDataClient,
            IMessageBusClient messageBusClient)
        {
            _academaticaDbContext = academaticaDbContext;
            _practiceAchievementsDataClient = practiceAchievementsDataClient;
            _messageBusClient = messageBusClient;
        }

        /// <summary>
        /// Endpoint used to get upcoming lessons for the authenticated user.
        /// </summary>
        /// <returns>Upcoming classes.</returns>
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

            List<UpcomingClassDto> upcomingClasses = new List<UpcomingClassDto>();

            foreach (var alClass in algebraClasses)
            {
                var isFinished = _academaticaDbContext.UserClasses
                    .Where(x => x.UserId.ToString() == userId && x.ClassId == alClass.Id)
                    .FirstOrDefault() != null;

                if (!isFinished)
                {
                    Topic classTopic = _academaticaDbContext.Topics.Where(x => x.Id == alClass.TopicId).FirstOrDefault();

                    if (classTopic == null)
                    {
                        continue;
                    }

                    int topicClassCount = _academaticaDbContext.Classes.Where(x => x.TopicId == classTopic.Id).Count();
                    string topicName = classTopic.Name;
                    int classNumber = int.Parse(alClass.Id.Split('-').LastOrDefault());

                    upcomingClasses.Add(new UpcomingClassDto() { 
                        Id = alClass.Id,
                        TopicId = alClass.TopicId,
                        TierId = alClass.TierId,
                        Name = alClass.Name,
                        Description = alClass.Description,
                        ExpReward = alClass.ExpReward,
                        ImageUrl = alClass.ImageUrl,
                        ClassNumber = classNumber,
                        IsAlgebraClass = alClass.IsAlgebraClass,
                        ProblemNum = alClass.ProblemNum,
                        TheoryUrl = alClass.TheoryUrl,
                        TopicClassCount = topicClassCount,
                        TopicName = topicName
                    });
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
                    Topic classTopic = _academaticaDbContext.Topics.Where(x => x.Id == geomClass.TopicId).FirstOrDefault();

                    if (classTopic == null)
                    {
                        continue;
                    }

                    int topicClassCount = _academaticaDbContext.Classes.Where(x => x.TopicId == classTopic.Id).Count();
                    string topicName = classTopic.Name;
                    int classNumber = int.Parse(geomClass.Id.Split('-').LastOrDefault());

                    upcomingClasses.Add(new UpcomingClassDto()
                    {
                        Id = geomClass.Id,
                        TopicId = geomClass.TopicId,
                        TierId = geomClass.TierId,
                        Name = geomClass.Name,
                        Description = geomClass.Description,
                        ExpReward = geomClass.ExpReward,
                        ImageUrl = geomClass.ImageUrl,
                        ClassNumber = classNumber,
                        IsAlgebraClass = geomClass.IsAlgebraClass,
                        ProblemNum = geomClass.ProblemNum,
                        TheoryUrl = geomClass.TheoryUrl,
                        TopicClassCount = topicClassCount,
                        TopicName = topicName
                    });
                    break;
                }
            }

            return Ok(new GetUpcomingClassesResponseDto
            {
                UpcomingClasses = upcomingClasses
            });
        }

        /// <summary>
        /// Endpoind used to get recommended practice topic for the authenticated user.
        /// </summary>
        /// <returns>Recommended practice topic.</returns>
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
                        Name = topic.Name
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
                    Name = recommendedTopic.Name
                }
            });
        }

        /// <summary>
        /// Endpoind used to get completed topics for the authenticated user.
        /// </summary>
        /// <returns>Completed topics.</returns>
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

            return Ok(new GetCompletedTopicsResponseDto()
            {
                Topics = completedTopics
            });
        }

        /// <summary>
        /// Endpoind used to get class information for given class ID and authenticated user.
        /// </summary>
        /// <param name="id">Class ID.</param>
        /// <returns>Class information.</returns>
        [HttpGet]
        [Route("classes/{id}")]
        public IActionResult GetClass(string id)
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

        /// <summary>
        /// Endpoint used to get user activity for the authenticated user.
        /// </summary>
        /// <returns>User activity data.</returns>
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

        /// <summary>
        /// Endpoint used to get random problems for given class ID.
        /// </summary>
        /// <param name="id">Class ID.</param>
        /// <returns>Problem list.</returns>
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

        /// <summary>
        /// Endpoint used to get problems for custom practice with given parameters.
        /// </summary>
        /// <param name="problemsRequestDto">Custom practice problem request parameters.</param>
        /// <returns>Custom practice problem list.</returns>
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

        /// <summary>
        /// Endpoint used to get problems for completed topics practice.
        /// </summary>
        /// <returns>Problems list.</returns>
        [HttpGet]
        [Route("practice/completed/problems")]
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

            var randomProblems = _academaticaDbContext.Problems.Where(x => completedTopicsIds.Contains(x.TopicId)).AsEnumerable().OrderBy(x => Guid.NewGuid()).Take(10);

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

        /// <summary>
        /// Endpoint used to get problems for a topic practice.
        /// </summary>
        /// <returns>Problems list.</returns>
        [HttpGet]
        [Route("practice/topic/problems")]
        public IActionResult GetTopicPracticeProblems([FromQuery] string topicId)
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

            var topic = _academaticaDbContext.Topics.Where(x => x.Id == topicId).FirstOrDefault();

            if (topic == null)
            {
                return NotFound("Invalid topic ID.");
            }

            var randomProblems = _academaticaDbContext.Problems.Where(x => x.TopicId == topicId).AsEnumerable().OrderBy(x => Guid.NewGuid()).Take(10);

            return Ok(new GetTopicPracticeProblemsResponseDto()
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

        /// <summary>
        /// Endpoint used to finish the class with given ID (or practice of a class with given ID) for authenticated user.
        /// </summary>
        /// <param name="classId">Class ID.</param>
        /// <param name="finishClassDto">Class finish params - amount of mistakes.</param>
        /// <returns>Action result with received exp and achievements.</returns>
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
                var stats = _academaticaDbContext.UserStats.Where(x => x.UserId == user.Id).FirstOrDefault();

                if (stats != null)
                {
                    stats.UserExp += stats.UserExp + finishedClass.ExpReward <= 15000 ? finishedClass.ExpReward : 0;
                    stats.UserExpThisWeek += finishedClass.ExpReward;
                    ExpChangePublishDto expChangePublishDto = new ExpChangePublishDto()
                    {
                        UserId = user.Id,
                        ExpThisWeek = stats.UserExpThisWeek,
                        Event = "EXP_CHANGE"
                    };
                    _messageBusClient.PublishExpChange(expChangePublishDto);
                }

                await _academaticaDbContext.SaveChangesAsync();

                var achievements = _practiceAchievementsDataClient.GetPracticeAchievements(user.Id, null, finishClassDto.MistakeCount);

                return Ok(new FinishClassResponseDto()
                {
                    Exp = (int)finishedClass.ExpReward,
                    Achievements = achievements
                });
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
                ExpChangePublishDto expChangePublishDto = new ExpChangePublishDto()
                {
                    UserId = user.Id,
                    ExpThisWeek = statsEntry.UserExpThisWeek,
                    Event = "EXP_CHANGE"
                };
                _messageBusClient.PublishExpChange(expChangePublishDto);
            }

            await _academaticaDbContext.SaveChangesAsync();

            var receivedAchievements = _practiceAchievementsDataClient.GetPracticeAchievements(user.Id, classId, finishClassDto.MistakeCount);

            return Ok(new FinishClassResponseDto()
            {
                Exp = (int)finishedClass.ExpReward,
                Achievements = receivedAchievements
            });
        }

        /// <summary>
        /// Endpoint used to finish the practice for authenticated user.
        /// </summary>
        /// <param name="finishPracticeDto">Practice finish params - amount of mistakes.</param>
        /// <returns>Action result with received exp and achievements.</returns>
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
                    ExpChangePublishDto expChangePublishDto = new ExpChangePublishDto()
                    {
                        UserId = user.Id,
                        ExpThisWeek = statsEntry.UserExpThisWeek,
                        Event = "EXP_CHANGE"
                    };
                    _messageBusClient.PublishExpChange(expChangePublishDto);


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

        /// <summary>
        /// Endpoint used to get tiers info for authenticated user.
        /// </summary>
        /// <returns>Tier list.</returns>
        [HttpGet]
        [Route("tiers")]
        public IActionResult GetTiersForUser()
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

            List<GetTierForUserResponseDto> tiers = new List<GetTierForUserResponseDto>();

            var tiersList = _academaticaDbContext.Tiers.AsEnumerable().OrderBy(x => x.Id, new TierIdComparer());

            for (int i = 0; i < tiersList.Count(); ++i)
            {
                var tier = tiersList.ElementAt(i);

                var tierTopicsIds = _academaticaDbContext.Topics.Where(x => x.TierId == tier.Id).Select(x => x.Id).ToList();
                var completedTierTopicsIds = _academaticaDbContext.UserTopic.Where(x => tierTopicsIds.Contains(x.TopicId) && x.UserId == user.Id).Select(x => x.TopicId).ToList();

                var tierIsComplete = _academaticaDbContext.UserTier.Where(x => x.UserId.ToString() == userId)
                    .Select(x => x.TierId).Contains(tier.Id);
                var tierIsUnlocked = false;

                if (i == 0)
                {
                    tierIsUnlocked = true;
                }
                else
                {
                    tierIsUnlocked = _academaticaDbContext.UserTier.Where(x => x.UserId.ToString() == userId)
                        .Select(x => x.TierId).Contains(tiersList.ElementAt(i - 1).Id);
                }

                tiers.Add(new GetTierForUserResponseDto()
                {
                    Id = tier.Id,
                    Name = tier.Name,
                    Description = tier.Description,
                    CompletionRate = (int)((double)completedTierTopicsIds.Count / tierTopicsIds.Count * 100),
                    IsComplete = tierIsComplete,
                    IsUnlocked = tierIsUnlocked
                });
            }

            return Ok(new GetTiersForUserResponseDto()
            {
                Tiers = tiers
            });
        }

        /// <summary>
        /// Endpoint used to get topic info for authenticated user in the given tier.
        /// </summary>
        /// <param name="id">Tier ID.</param>
        /// <returns>Topic list.</returns>
        [HttpGet]
        [Route("tiers/{id}")]
        public IActionResult GetTopicsForUser(string id)
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

            var tier = _academaticaDbContext.Tiers.Where(x => x.Id == id).FirstOrDefault();

            if (tier == null)
            {
                return NotFound("Invalid tier ID");
            }

            List<GetTopicForUserResponseDto> topics = new List<GetTopicForUserResponseDto>();

            var topicsList = _academaticaDbContext.Topics.Where(x => x.TierId == id).AsEnumerable().OrderBy(x => x.Id, new TopicIdComparer());

            var algebraTopicsIds = _academaticaDbContext.Topics.Where(x => x.IsAlgebraTopic).AsEnumerable().OrderBy(x => x.Id, new TopicIdComparer()).Select(x => x.Id).ToList();
            var geometryTopicsIds = _academaticaDbContext.Topics.Where(x => !x.IsAlgebraTopic).AsEnumerable().OrderBy(x => x.Id, new TopicIdComparer()).Select(x => x.Id).ToList();

            foreach (var entry in algebraTopicsIds)
            {
                Console.WriteLine(entry);
            }
            Console.WriteLine();
            foreach (var entry in geometryTopicsIds)
            {
                Console.WriteLine(entry);
            }

            for (int i = 0; i < topicsList.Count(); ++i)
            {
                var topicClassesIds = _academaticaDbContext.Classes.Where(x => x.TopicId == topicsList.ElementAt(i).Id).Select(x => x.Id).ToList();
                var completedTopicClassesIds = _academaticaDbContext.UserClasses.Where(x => topicClassesIds.Contains(x.ClassId) && x.UserId == user.Id)
                    .Select(x => x.ClassId).ToList();

                var topicIsComplete = _academaticaDbContext.UserTopic.Where(x => x.UserId.ToString() == userId)
                    .Select(x => x.TopicId).Contains(topicsList.ElementAt(i).Id);
                var topicIsUnlocked = false;

                var isAlgebraTopic = topicsList.ElementAt(i).IsAlgebraTopic;
                var elementId = 0;

                if (isAlgebraTopic)
                {
                    elementId = algebraTopicsIds.IndexOf(topicsList.ElementAt(i).Id);

                    if (elementId == 0)
                    {
                        topicIsUnlocked = true;
                    }
                    else
                    {
                        topicIsUnlocked = _academaticaDbContext.UserTopic.Where(x => x.UserId.ToString() == userId)
                            .Select(x => x.TopicId).Contains(algebraTopicsIds[elementId - 1]);
                    }
                }
                else
                {
                    elementId = geometryTopicsIds.IndexOf(topicsList.ElementAt(i).Id);

                    if (elementId == 0)
                    {
                        topicIsUnlocked = true;
                    }
                    else
                    {
                        topicIsUnlocked = _academaticaDbContext.UserTopic.Where(x => x.UserId.ToString() == userId)
                            .Select(x => x.TopicId).Contains(geometryTopicsIds[elementId - 1]);
                    }
                }

                topics.Add(new GetTopicForUserResponseDto()
                {
                    Id = topicsList.ElementAt(i).Id,
                    Name = topicsList.ElementAt(i).Name,
                    Description = topicsList.ElementAt(i).Description,
                    ImageUrl = topicsList.ElementAt(i).ImageUrl,
                    IsAlgebraTopic = topicsList.ElementAt(i).IsAlgebraTopic,
                    CompletionRate = topicClassesIds.Count == 0 ? 0 : (int)(((double)completedTopicClassesIds.Count) / topicClassesIds.Count * 100),
                    IsComplete = topicIsComplete,
                    IsUnlocked = topicIsUnlocked,
                    ClassCount = topicClassesIds.Count
                });
            }

            return Ok(new GetTopicsForUserResponseDto()
            {
                Topics = topics
            });
        }

        /// <summary>
        /// Endpoint used to get classes info for authenticated user in the given tier and given topic.
        /// </summary>
        /// <param name="topicId">Topic ID.</param>
        /// <returns>Topic list.</returns>
        [HttpGet]
        [Route("topics/{topicId}")]
        public IActionResult GetClassesForUser(string topicId)
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

            var topic = _academaticaDbContext.Topics.Where(x => x.Id == topicId).FirstOrDefault();

            if (topic == null)
            {
                return NotFound("Invalid topic ID");
            }

            List<GetClassForUserResponseDto> classes = new List<GetClassForUserResponseDto>();

            var classesList = _academaticaDbContext.Classes.Where(x => x.TopicId == topicId).AsEnumerable().OrderBy(x => x.Id, new ClassIdComparer());

            for (int i = 0; i < classesList.Count(); ++i)
            {
                var currentClass = classesList.ElementAt(i);

                var classIsComplete = _academaticaDbContext.UserClasses.Where(x => x.UserId.ToString() == userId)
                    .Select(x => x.ClassId).Contains(currentClass.Id);
                var classIsUnlocked = false;

                if (i == 0 || classesList.ElementAt(i - 1).TopicId != currentClass.TopicId)
                {
                    classIsUnlocked = true;
                }
                else
                {
                    classIsUnlocked = _academaticaDbContext.UserClasses.Where(x => x.UserId.ToString() == userId)
                        .Select(x => x.ClassId).Contains(classesList.ElementAt(i - 1).Id);
                }

                classes.Add(new GetClassForUserResponseDto()
                {
                    Id = currentClass.Id,
                    Name = currentClass.Name,
                    Description = currentClass.Description,
                    IsComplete = classIsComplete,
                    ImageUrl = currentClass.ImageUrl,
                    ProblemNum = currentClass.ProblemNum,
                    TheoryUrl = currentClass.TheoryUrl,
                    TopicName = topic.Name,
                    IsUnlocked = classIsUnlocked
                });
            }

            return Ok(new GetClassesForUserResponseDto()
            {
                Classes = classes
            });
        }
    }
}
