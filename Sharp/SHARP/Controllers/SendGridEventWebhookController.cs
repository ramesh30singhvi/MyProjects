
using Microsoft.AspNetCore.Mvc;
using SHARP.BusinessLogic.SendGridEvents.Parser;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Event = SHARP.BusinessLogic.SendGridEvents.Models.Event;

namespace SHARP.Controllers
{
    [Route("/")]
    [ApiController]
    public class SendGridEventWebhookController : Controller
    {
        [Route("/open-email")]
        [HttpPost]
        public async Task<IActionResult> OpenEmail()
        {
            try
            {
                var sentMessageId = await GetMessageId(Request.Body);
            }catch(System.Exception exp)
            {

            }
            return Ok();
        }

        private async Task<string> GetMessageId(Stream body){
            IEnumerable<Event> events = await EventParser.ParseAsync(body);
            if (events.ToArray().Any())
            {
                var messageId = events.FirstOrDefault().SendGridMessageId;
                var ids = messageId.Split(".");
                if(ids.Any())
                    return ids.First();
            }
            return string.Empty;
        } 
        [Route("/click-email")]
        [HttpPost]
        public async Task<IActionResult> ClickEmail()
        {
            try
            {
                var sentMessageId = await GetMessageId(Request.Body);
            }
            catch (System.Exception exp)
            {

            }
            return Ok();
        }
    }


}
