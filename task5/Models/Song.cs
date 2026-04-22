namespace task5.Models
{
    public class Song
    {
        public int Id { get; set; }          
        public int Likes { get; set; }       
        public string Title { get; set; } = "";
        public string Artist { get; set; } = "";
        public string Album { get; set; } = "";
        public string Genre { get; set; } = "";
        public string CoverUrl { get; set; } = "";
        public string AudioUrl { get; set; } = "";
        public List<string> Lyrics { get; set; } = new();
    }
}
