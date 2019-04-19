namespace BillionSongs.Pages {
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    [ResponseCache(VaryByHeader = "User-Agent", Duration = 24 * 60 * 60)]
    public class PrivacyModel : PageModel {
        public void OnGet() {
        }
    }
}