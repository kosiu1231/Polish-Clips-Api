namespace Polish_Clips.Services.ClipService
{
    public interface IClipService
    {
        Task<ServiceResponse<GetClipDto>> AddClip(AddClipDto newClip);
        Task<ServiceResponse<GetClipDto>> GetClip(int id);
        Task<ServiceResponse<List<GetClipDto>>> GetClips([FromQuery] QueryObject query);
        Task<ServiceResponse<GetClipDto>> LikeClip(int id);
        Task<ServiceResponse<GetClipDto>> DislikeClip(int id);
    }
}
