using ChurchWebsite.Models;
using ChurchWebsite.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ChurchWebsite.Pages.Groups;

/// <summary>Groups list page. USE CASE: Display all groups from GroupService.</summary>
public class IndexModel : PageModel
{
    private readonly IGroupService _groupService;

    public IndexModel(IGroupService groupService)
    {
        _groupService = groupService;
    }

    public List<Group> Groups { get; set; } = [];

    public void OnGet()
    {
        Groups = _groupService.GetAll();
    }
}
