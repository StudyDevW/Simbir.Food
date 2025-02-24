using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram_Components.Interfaces;

namespace Telegram_Components.Controllers
{
    [Route("/")]
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class TelegramBotController : ControllerBase
    {
        private readonly IMessageReceiver _messageReceiver;

        public TelegramBotController(IMessageReceiver messageReceiver)
        {
            _messageReceiver = messageReceiver;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Update update)
        {
         
            switch (update.Type)
            {
                case UpdateType.Message:
                    await _messageReceiver.handleMessage(update.Message);
                    break;
                case UpdateType.CallbackQuery:
                    await _messageReceiver.handleCallbackQuery(update.CallbackQuery); 
                    break;
            }
            
            return Ok();
        }

        [HttpGet]
        public string Get()
        {
            return "Telegram bot was started";
        }
    }
}
