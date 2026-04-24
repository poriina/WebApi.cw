namespace WebApi.Models
{
    public class favorModel
    {
        public int Id { get; set; } 
        public int MalId { get; set; } 
        public string Title { get; set; } 
        public string Type { get; set; } 
        public string PersonalNote { get; set; } 
        public float? Score { get; set; }
    }
}
