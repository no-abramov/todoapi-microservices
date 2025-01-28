using Asp.Versioning;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TasksService.Data;
using TasksService.Models;

namespace TasksService.Controllers
{
    /// <summary>
    /// Контроллер для работы с задачами (Tasks).
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class TasksController : ControllerBase
    {
        #region Fields

        /// <summary>
        /// Контекст базы данных для работы с задачами.
        /// </summary>
        private readonly TasksDbContext _context;

        #endregion

        #region Constructor

        /// <summary>
        /// Инициализирует новый экземпляр <see cref="TasksController"/>.
        /// </summary>
        /// <param name="context">Контекст базы данных для задач.</param>
        public TasksController(TasksDbContext context)
        {
            _context = context;
        }

        #endregion

        #region CRUD

        /// <summary>
        /// Получает все задачи по идентификатору пользователя.
        /// </summary>
        /// <param name="userId">Идентификатор пользователя.</param>
        /// <returns>Задача с указанным идентификатором.</returns>
        [HttpGet("user/{userId}")]
        [MapToApiVersion("1.0")]
        public IActionResult GetTasksByUserId(int userId)
        {
            var tasks = _context.Tasks.Where(t => t.UserId == userId).ToList();
            if (!tasks.Any())
                return NotFound($"No tasks found for UserId: {userId}");

            return Ok(tasks);
        }

        /// <summary>
        /// Создаёт новую задачу.
        /// </summary>
        /// <param name="taskItem">Объект задачи для создания.</param>
        /// <returns>Созданная задача с её идентификатором.</returns>
        [HttpPost]
        [MapToApiVersion("1.0")]
        public IActionResult CreateTask([FromBody] TaskItem taskItem)
        {
            if (taskItem == null || taskItem.UserId <= 0)
                return BadRequest("Invalid task data.");

            _context.Tasks.Add(taskItem);
            _context.SaveChanges();
            return Ok(taskItem);
        }

        /// <summary>
        /// Получает список всех задач.
        /// </summary>
        /// <returns>Список задач.</returns>
        [HttpGet]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<TaskItem>>> GetTasks()
        {
            return await _context.Tasks.ToListAsync();
        }

        #endregion

        #region Private

        /// <summary>
        /// Проверяет, существует ли задача с указанным идентификатором.
        /// </summary>
        /// <param name="id">Идентификатор задачи.</param>
        /// <returns>True, если задача существует; иначе False.</returns>
        private bool TaskExists(int id)
        {
            return _context.Tasks.Any(e => e.Id == id);
        }

        #endregion
    }
}
