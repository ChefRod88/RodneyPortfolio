using ChurchWebsite.Models;

namespace ChurchWebsite.Services;

public interface ISermonService
{
    List<Sermon> GetAll();
    Sermon? GetById(int id);
}
