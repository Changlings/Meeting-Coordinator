using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MeetingCoordinator.Models
{
    /// <summary>
    /// This is a View Model, entirely seperate from our Database models.
    /// This is used to provide validation and rendering capabilities to 
    /// the Razor templating engine for rendering our Login page
    /// </summary>
    public class LoginViewModel
    {
        /// <summary>
        /// Define a username input text field
        /// that's required and has the label "Username"
        /// </summary>
        [Required]
        [Display(Name = "Username")]
        [DataType(DataType.Text)]
        public string Username { get; set; }
        /// <summary>
        /// Define a password input text field
        /// that's required and has the label "Password"
        /// </summary>
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }
        /// <summary>
        /// Define a checkbox (boolean) field to enable
        /// remember me functionality
        /// </summary>
        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }
}
