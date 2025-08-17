using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WhatSharp.Shared.DTO;
using WhatSharp.Shared.Models;
using WhatSharp.Srv.Data;

namespace WhatSharp.Srv.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessageController : ControllerBase
    {
        private readonly AppDbContext _dbContext;

        public MessageController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageDTO dto)
        {
            var message = new Message
            {
                ChatId = dto.ChatId,
                UserId = dto.UserId,
                Content = dto.Content,
                SentAt = DateTime.UtcNow
            };

            _dbContext.Messages.Add(message);
            await _dbContext.SaveChangesAsync();

            return Ok(message);
        }

        [HttpGet("chat/{chatId}")]
        public async Task<IActionResult> GetMessagesForChat(int chatId)
        {
            var messages = await _dbContext.Messages
                .Where(m => m.ChatId == chatId)
                .OrderBy(m => m.SentAt)
                .ToListAsync();

            return Ok(messages);
        }
    }
}
