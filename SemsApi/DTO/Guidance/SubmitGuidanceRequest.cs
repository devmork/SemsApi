namespace SemsApi.DTO.Guidance
{
    public class SubmitGuidanceRequest //Purpose: Payload when student submits guidance evaluation answers
    {
        public int StudentId { get; set; }
        public string Sy { get; set; } = "2026-2027";
        public int Term { get; set; } = 1;
        public List<GuidanceAnswerDto> Answers { get; set; } = new();
    }
}
