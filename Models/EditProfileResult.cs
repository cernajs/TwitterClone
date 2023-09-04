namespace TwitterClone.Models;

public class EditProfileResult
{
    public bool Success { get; set; }
    public List<string> Errors { get; set; } = new List<string>();
}
