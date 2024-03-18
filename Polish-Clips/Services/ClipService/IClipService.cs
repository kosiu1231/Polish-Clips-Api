namespace Polish_Clips.Services.ClipService
{
    public interface IClipService
    {
        Task<ServiceResponse<GetClipDto>> AddClip(AddClipDto newClip);
        //get
        Task<ServiceResponse<GetClipDto>> LikeClip(int id);
        Task<ServiceResponse<GetClipDto>> DislikeClip(int id);
    }
}
