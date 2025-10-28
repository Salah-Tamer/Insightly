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
        public async Task<IEnumerable<ChatMessage>> GetAllMessages(string SenderId , string ReceiverId)
        {
            return await context.ChatMessages.Where(u=>(u.SenderId==SenderId&&u.ReceiverId==ReceiverId)||(u.ReceiverId==SenderId&&u.SenderId==ReceiverId))
                .OrderBy(c=>c.SentAt).ToListAsync();
        }
        public async Task AddMessge(ChatMessage chatMessage)
        {
          await  context.ChatMessages.AddAsync(chatMessage);
         
        }
    }
}
