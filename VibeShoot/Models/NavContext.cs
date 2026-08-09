namespace VibeShoot.Models
{
    public class NavContext
    {
        public string Slug { get; set; } = "";
        // "ABOUT", "ALBUM", or "SCHEDULE"
        public string ActiveTab { get; set; } = "";
    }
}
