using ChurchWebsite.Models;
using ChurchWebsite.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ChurchWebsite.Pages.Events;

public class IndexModel : PageModel
{
    private readonly EventService _eventService;

    public IndexModel(EventService eventService)
    {
        _eventService = eventService;
    }

    public List<ChurchEvent> Events { get; set; } = [];

    public void OnGet()
    {
        Events = _eventService.GetAll();
    }
}
