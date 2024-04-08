using Polish_Clips.Dtos.Report;

namespace Polish_Clips
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Clip, GetClipDto>();
            CreateMap<AddClipDto, Clip>();
            CreateMap<User, GetUserDto>();
            CreateMap<Game, GetGameDto>();
            CreateMap<Like, GetLikeDto>();
            CreateMap<AddCommentDto, Comment>();
            CreateMap<Comment, GetCommentDto>();
            CreateMap<Report, GetReportDto>();
            CreateMap<AddReportDto, Report>();
        }
    }
}
