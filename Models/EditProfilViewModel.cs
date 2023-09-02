using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TwitterClone.Models;

public class EditProfileViewModel
{
    public string Id { get; set; }
    public string UserName { get; set; }
    [EmailAddress]
    public string Email { get; set; }
    //[Phone]
    // public string PhoneNumber { get; set; }
    // public string Bio { get; set; }
    // public string ProfilePicture { get; set; }

}
