using ChurchWebsite.Models;

namespace ChurchWebsite.Services;

public interface IEventService
{
    List<ChurchEvent> GetAll();
    ChurchEvent? GetById(int id);
}
