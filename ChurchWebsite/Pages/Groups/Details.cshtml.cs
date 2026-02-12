using ChurchWebsite.Models;
using ChurchWebsite.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ChurchWebsite.Pages.Groups;

public class DetailsModel : PageModel
{
    private readonly GroupService _groupService;

    public DetailsModel(GroupService groupService)
    {
        _groupService = groupService;
    }

    public Group? Group { get; set; }

    public IActionResult OnGet(int id)
    {
        Group = _groupService.GetById(id);
        if (Group == null)
            return NotFound();
        return Page();
    }
}
