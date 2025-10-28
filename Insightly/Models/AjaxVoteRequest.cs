using Microsoft.AspNetCore.SignalR;

namespace Insightly.Models
{
    public class AjaxVoteRequest
    {
        public int ArticleId { get; set; }
        public bool IsUpvote { get; set; }
    }
}
