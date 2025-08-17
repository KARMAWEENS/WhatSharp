using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using WhatSharp.Shared.DTO;
using WhatSharp.Shared.Models;
using WhatSharp.Srv.Data;
using WhatSharp.Srv.Hubs;

namespace WhatSharp.Srv.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatsController : ControllerBase
    {
        private readonly AppDbContext _dbContext;
        private readonly IHubContext<ChatHub> _hub;

        public ChatsController(AppDbContext dbContext, IHubContext<ChatHub> hub)
        {
            _dbContext = dbContext;
            _hub = hub;
        }

        [HttpGet("user/{userId:int}")]
        public async Task<ActionResult<List<ChatListItemDTO>>> GetChatsForUser(int userId)
        {
            var chats = await _dbContext.Chats
                .Include(c => c.Participants)
                .Include(c => c.Messages)
                .Where(c => c.Participants.Any(p => p.Id == userId))
                .ToListAsync();

            var readStates = await _dbContext.ChatReadStates
                .Where(s => s.UserId == userId)
                .ToDictionaryAsync(s => s.ChatId, s => s.LastReadAt);

            var result = chats.Select(c => 
            {
                
                var hasState = readStates.TryGetValue(c.Id, out var lastReadAt);
                var lastRead = hasState ? lastReadAt : DateTime.MinValue;

                var unread = c.Messages.Count(m => m.SentAt > lastRead && m.UserId != userId);
                
                
                return new ChatListItemDTO
                {
                    Id = c.Id,
                    Name = c.Name,
                    ParticipantIds = c.Participants.Select(p => p.Id).ToList(),
                    LastMessage = c.Messages
                        .OrderByDescending(m => m.SentAt)
                        .Select(m => new LastMessageDTO
                        {
                            Id = m.Id, UserId = m.UserId, Content = m.Content, SentAt = m.SentAt
                        })
                        .FirstOrDefault(),
                    UnreadCount = unread
                };
            }).ToList();

            return Ok(result);
        }
        
        [HttpDelete("{chatId:int}/participants/{userId:int}")]
        public async Task<IActionResult> LeaveChat(int chatId, int userId)
        {
            var chat = await _dbContext.Chats
                .Include(c => c.Participants)
                .FirstOrDefaultAsync(c => c.Id == chatId);
            if (chat is null) return NotFound();

            var user = chat.Participants.FirstOrDefault(p => p.Id == userId);
            if (user is null) return NoContent(); // déjà parti

            chat.Participants.Remove(user);

            // (facultatif) si plus personne, supprimer la conv
            if (!chat.Participants.Any())
                _dbContext.Chats.Remove(chat);

            await _dbContext.SaveChangesAsync();

            // (facultatif) notifier les autres via SignalR: "ParticipantLeft"
            // await _hub.Clients.Group($"chat:{chatId}")
            //     .SendAsync("ParticipantLeft", chatId, userId);

            return NoContent();
        }

        [HttpPost]
        public async Task<IActionResult> CreateChat([FromBody] ChatCreateDTO chatDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var ids = chatDto.ParticipantIds.Distinct().ToList();
            if (ids.Count == 0) return BadRequest("Au moins un participant requis.");

            var participants = await _dbContext.Users
                .Where(u => chatDto.ParticipantIds.Contains(u.Id))
                .ToListAsync();

            if (participants.Count != chatDto.ParticipantIds.Count)
                return BadRequest("Certains participants n'existent pas.");

            var chat = new Chat { Name = chatDto.Name?.Trim() ?? string.Empty };
            foreach (var u in participants) chat.Participants.Add(u);

            _dbContext.Chats.Add(chat);
            await _dbContext.SaveChangesAsync();

            var created = new ChatListItemDTO
            {
                Id = chat.Id,
                Name = chat.Name,
                ParticipantIds = chat.Participants.Select(p => p.Id).ToList(),
                LastMessage = null,
                UnreadCount = 0 
            };

            foreach (var uid in created.ParticipantIds)
            {
                await _hub.Clients.Group($"user:{uid}")
                    .SendAsync("ChatCreated", created);
            }
            
            
            var response = new
            {
                chat.Id,
                chat.Name,
                ParticipantIds = created.ParticipantIds,
                ParticipantNames = chat.Participants.Select(p => p.Login).ToList()
            };

            return CreatedAtAction(nameof(GetById), new { id = chat.Id }, response);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ChatDetailsResponseDTO>> GetById(int id)
        {
            var chat = await _dbContext.Chats
                .Include(c => c.Participants)
                .Include(c => c.Messages)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (chat is null) return NotFound();

            return Ok(new
            {
                chat.Id,
                chat.Name,
                ParticipantIds = chat.Participants.Select(p => p.Id).ToList(),
                ParticipantNames = chat.Participants.Select(p => p.Login).ToList(), // <— corrigé
                Messages = chat.Messages
                    .OrderBy(m => m.SentAt)
                    .Select(m => new MessagesDTO
                    {
                        Id = m.Id,
                        UserId = m.UserId,
                        Content = m.Content,
                        SentAt = m.SentAt
                    }).ToList()
            });
        }

        [HttpPost("{chatId}/participants")]
        public async Task<IActionResult> AddParticipant(int chatId, [FromBody] AddParticipantDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var chat = await _dbContext.Chats.Include(c => c.Participants)
                .FirstOrDefaultAsync(c => c.Id == chatId);
            if (chat is null) return NotFound("Conversation non trouvée.");

            var user = await _dbContext.Users.FindAsync(dto.UserId);
            if (user is null) return NotFound("Utilisateur non trouvé.");

            if (chat.Participants.Any(p => p.Id == user.Id))
                return BadRequest("Utilisateur déjà dans la conversation.");

            chat.Participants.Add(user);
            await _dbContext.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("{chatId}/messages")]
        public async Task<ActionResult<List<MessagesDTO>>> GetMessages(int chatId)
        {
            var messages = await _dbContext.Messages
                .Where(m => m.ChatId == chatId)
                .OrderBy(m => m.SentAt)
                .Select(m => new MessagesDTO
                {
                    Id = m.Id, UserId = m.UserId, Content = m.Content, SentAt = m.SentAt
                })
                .ToListAsync();

            return Ok(messages);
        }

        [HttpPost("{chatId}/messages")]
        public async Task<IActionResult> PostMessage(int chatId, [FromBody] MessagesDTO dto)
        {
            var chat = await _dbContext.Chats
                .Include(c => c.Participants)
                .FirstOrDefaultAsync(c => c.Id == chatId);
            if (chat == null) return NotFound("Conversation inexistante");

            var message = new Message
            {
                ChatId = chatId,
                UserId = dto.UserId,
                Content = dto.Content,
                SentAt = DateTime.UtcNow
            };

            _dbContext.Messages.Add(message);
            await _dbContext.SaveChangesAsync();

            // renvoyer le message créé
            var outDto = new MessagesDTO
            {
                Id = message.Id,
                UserId = message.UserId,
                Content = message.Content,
                SentAt = message.SentAt
            };

            await _hub.Clients.Group($"chat:{chatId}")
                .SendAsync("MessageReceived", chatId, outDto);

            return Ok(outDto);
        }

        [HttpPut("{chatId:int}/read")]
        public async Task<IActionResult> MarkRead(int chatId, [FromBody] MarkReadDTO dto)
        {
            var chatExists = await _dbContext.Chats.AnyAsync(c => c.Id == chatId);
            if (!chatExists) return NotFound();
            
            var state = await _dbContext.ChatReadStates
                .FirstOrDefaultAsync(s => s.ChatId == chatId && s.UserId == dto.UserId);
            
            if (state is null)
            {
                state = new ChatReadState { ChatId = chatId, UserId = dto.UserId };
                _dbContext.ChatReadStates.Add(state);
            }
            
            var lastMsgTime = await _dbContext.Messages
                .Where(m => m.ChatId == chatId)
                .MaxAsync(m => (DateTime?)m.SentAt) ?? DateTime.UtcNow;

            state.LastReadAt = dto.ReadAt ?? lastMsgTime;

            await _dbContext.SaveChangesAsync();
            return NoContent();
        }
    }
}
