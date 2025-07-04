namespace SHARP.BusinessLogic.DTO.Common
{
    public class MessageResultDto<TResult>
    {
        public TResult Result { get; set; }

        public string UserTimeZone { get; set; }
    }
}
