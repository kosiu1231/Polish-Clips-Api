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
        }
    }
}
