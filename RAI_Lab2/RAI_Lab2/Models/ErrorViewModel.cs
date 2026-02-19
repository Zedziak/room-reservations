namespace RAI_Lab2.Models
{
    public class ErrorViewModel
    {
        public string? requestId { get; set; }

        public bool showRequestId => !string.IsNullOrEmpty(requestId);
    }
}
