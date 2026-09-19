namespace SemsApi.DTO
{
    public class SubmitGuidanceRequest //Purpose: Payload when student submits guidance evaluation answers
    {
        public string StudentId { get; set; } = string.Empty;
        public string Sy { get; set; } = "2026-2027";
        public int Term { get; set; } = 1;
        public List<GuidanceAnswerDto> Answers { get; set; } = new();
    }
}
