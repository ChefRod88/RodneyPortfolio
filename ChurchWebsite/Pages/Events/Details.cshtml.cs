using ChurchWebsite.Models;
using ChurchWebsite.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ChurchWebsite.Pages.Events;

public class DetailsModel : PageModel
{
    private readonly EventService _eventService;

    public DetailsModel(EventService eventService)
    {
        _eventService = eventService;
    }

    public ChurchEvent? Event { get; set; }

    public IActionResult OnGet(int id)
    {
        Event = _eventService.GetById(id);
        if (Event == null)
            return NotFound();
        return Page();
    }
}
