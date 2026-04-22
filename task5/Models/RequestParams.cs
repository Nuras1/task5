namespace task5.Models
{
        public class RequestParams
        {
        public string Region { get; set; } = "en";
        public long Seed { get; set; }
        public double LikesAvg { get; set; }
        public int Page { get; set; } = 1;
    }
}