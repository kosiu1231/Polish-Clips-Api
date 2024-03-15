namespace Polish_Clips.Services.ClipService
{
    public interface IClipService
    {
        Task<ServiceResponse<GetClipDto>> AddClip(AddClipDto newClip);
    }
}
