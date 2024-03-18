namespace Polish_Clips.Services.CommentService
{
    public interface ICommentService
    {
        Task<ServiceResponse<GetClipDto>> AddComment(AddCommentDto newComment);
        Task<ServiceResponse<GetClipDto>> DeleteComment(int id);
    }
}
