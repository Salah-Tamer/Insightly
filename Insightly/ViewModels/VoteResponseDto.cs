namespace Insightly.ViewModels
{
    public class VoteResponseDto
    {
        public bool IsRemoved { get; set; }
    }

    public class VoteCountDto
    {
        public int Score { get; set; }
    }

    public class UserVoteStatusDto
    {
        public bool HasVoted { get; set; }
        public bool? IsUpvote { get; set; }
    }
}

