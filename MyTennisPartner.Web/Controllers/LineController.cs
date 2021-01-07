using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MyTennisPartner.Data.Managers;
using MyTennisPartner.Data.Models;
using MyTennisPartner.Web.Exceptions;
using MyTennisPartner.Web.Services;

namespace MyTennisPartner.Web.Controllers
{
    [Produces("application/json")]
    [Route("api/Line")]
    [Authorize]
    [ApiController]
    public partial class LineController : BaseController
    {

        #region members
        private readonly LineManager _lineManager;
        #endregion

        #region constructor
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="logger"></param>
        public LineController(ILogger<LineController> logger, LineManager lineManager, NotificationService notificationService) : base(logger, notificationService) {
            _lineManager = lineManager;
        }

        #endregion

        #region get
        // GET: api/Line
        [HttpGet]
        public async Task<IActionResult> GetLines()
        {
            var lines = await _lineManager.GetAllLinesAsync();
            return ApiOk(lines);
        }

        // GET: api/Line/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetLine([FromRoute] int id)
        {
            var line = await _lineManager.GetLineAsync(id);
            return ApiOk(line);
        }

        /// <summary>
        /// get line info by match.
        /// </summary>
        /// <param name="matchId"></param>
        /// <param name="memberId"></param>
        /// <returns></returns>
        [HttpGet("GetByMatch")]
        public async Task<IActionResult> GetByMatch([FromQuery] int matchId)
        {
            var linesWithAvailability = await _lineManager.GetLinesByMatchAsync(matchId);

            return ApiOk(linesWithAvailability);
        }
        #endregion

        #region put
        // PUT: api/Line/5
        [ApiValidationFilter]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutLine([FromRoute] int id, [FromBody] Line line)
        {
            if (line is null) return ApiBad("line", "cannot be null");

            if (id != line.LineId)
            {
                return ApiBad("lineId", $"Line Id in path ({id}) does not match line in body ({line.LineId})");
            }
            var updatedLine = await _lineManager.UpdateLineAsync(line);

            return ApiOk(updatedLine);
        }
        #endregion

        #region post
        // POST: api/Line
        [ApiValidationFilter]
        [HttpPost]
        public async Task<IActionResult> PostLine([FromBody] Line line)
        {
            var newLine = await _lineManager.CreateLineAsync(line);

            return ApiOk(newLine);
        }
        #endregion

        #region delete
        // DELETE: api/Line/5
        [ApiValidationFilter]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLine([FromRoute] int id)
        {
            var line = await _lineManager.RemoveLineAsync(id);
            return Ok(line);
        }
        #endregion

    }
}