using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyTennisPartner.Data.Context;
using MyTennisPartner.Data.Models;
using MyTennisPartner.Web.Exceptions;
using MyTennisPartner.Web.Services;
using ImageMagick;

namespace MyTennisPartner.Web.Controllers
{
    [Produces("application/json")]
    [Route("api/MemberImage")]
    [Authorize]
    public class MemberImageController : BaseController
    {

        public MemberImageController(ILogger<MemberImageController> logger, TennisContext context, NotificationService notificationService) : base(logger, notificationService, context)
        {
        }

        // GET: api/MemberImage
        [HttpGet]
        public IEnumerable<MemberImage> GetMemberImages()
        {
            return Context.MemberImages;
        }

        // GET: api/MemberImage/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetMemberImage([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var memberImage = await Context.MemberImages.SingleOrDefaultAsync(m => m.ImageId == id);

            if (memberImage == null)
            {
                return NotFound();
            }

            return Ok(memberImage);
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 60)]
        [HttpGet("file/{memberId}")]
        public FileContentResult GetMemberImageFile([FromRoute] int memberId)
        {

            var imageBytes = Context.MemberImages
            .Where(i => i.MemberId == memberId)
            .Select(i => i.ImageBytes)
            .FirstOrDefault();

            if (imageBytes != null) return File(imageBytes, "image/png");

            // Image not found
            //Response.StatusCode = (int)HttpStatusCode.NotFound;
            Response.StatusCode = (int)HttpStatusCode.OK;
            return File(new byte[0], "image/png");

            //TODO: log this
            //    Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            //    return File(new byte[0], JpegContentType);
            
        }

        // POST: api/MemberImage
        [ApiValidationFilter]
        [HttpPost("{memberId}")]
        public async Task<IActionResult> PostMemberImage([FromRoute] int memberId)
        {
            var imageExists = true;

            var files = Request.Form.Files;
            if (!files.Any())
            {
                return ApiBad("","No file attached!");
            }

            // todo: add security check here to allow only the logged in member to create/update their image

            var memberImage = Context.MemberImages
                .Where(i => i.MemberId == memberId).FirstOrDefault();

            if (memberImage == null) {
                imageExists = false;
                memberImage = new MemberImage { MemberId = memberId };
            }


            using (var memoryStream = new MemoryStream())
            {
                await files[0].CopyToAsync(memoryStream);
                //memberImage.ImageBytes = memoryStream.ToArray();

                // auto-orient and strip any EXIF meta-data before saving
                var imageBytes = memoryStream.ToArray();
                var image = new MagickImage(imageBytes);
                image.AutoOrient();
                image.Strip();

                // now save to data model
                memberImage.ImageBytes = image.ToByteArray();
            }

            if (imageExists)
            {
                Context.Entry(memberImage).State = EntityState.Modified;
            }
            else
            {
                Context.MemberImages.Add(memberImage);
            }
            await Context.SaveChangesAsync();

            return ApiOk(new { id = memberImage.ImageId });
        }

        // DELETE: api/MemberImage/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMemberImage([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var memberImage = await Context.MemberImages.SingleOrDefaultAsync(m => m.ImageId == id);
            if (memberImage == null)
            {
                return NotFound();
            }

            Context.MemberImages.Remove(memberImage);
            await Context.SaveChangesAsync();

            return Ok(memberImage);
        }

    }
}