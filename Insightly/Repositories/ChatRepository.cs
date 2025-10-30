using Insightly.Models;
using Microsoft.EntityFrameworkCore;

namespace Insightly.Repositories
{
    public class ChatRepository : IChatRepository
    {
        private readonly AppDbContext context;

        public ChatRepository(AppDbContext context)
        {
            this.context = context;
        }
        public async Task<IEnumerable<Chat>>GetChats(string UserId)
        {
            var chats = await context.Chats
            .Include(c => c.Messages)
             .Include(c => c.User)        
            .Include(c => c.OtherUser)   
           .Where(c => c.UserId == UserId || c.OtherUserId == UserId)
           .ToListAsync();
            return chats;
        }
        public async Task<IEnumerable<ChatMessage>> GetAllMessages(string SenderId, string ReceiverId)
        {
            var chat = await  context.Chats.Include(m=>m.Messages).FirstOrDefaultAsync(u => (u.UserId == SenderId && u.OtherUserId == ReceiverId)
              || (u.UserId == ReceiverId && u.OtherUserId == SenderId));
            if (chat == null) 
            {
              return Enumerable.Empty<ChatMessage>();
            }
            return chat.Messages.OrderBy(m => m.SentAt).ToList();
        }
        public async Task AddMessge(ChatMessage chatMessage)
        {
            await context.ChatMessages.AddAsync(chatMessage);

        }
        public async Task<Chat?> GetChatBetweenUsers(string userId, string otherUserId)
        {
            return await context.Chats
                .Include(c => c.Messages)
                .FirstOrDefaultAsync(c =>
                    (c.UserId == userId && c.OtherUserId == otherUserId) ||
                    (c.UserId == otherUserId && c.OtherUserId == userId));
        }
        public async Task AddChat(Chat chat)
        {
            await context.Chats.AddAsync(chat);
        }

    }


}
