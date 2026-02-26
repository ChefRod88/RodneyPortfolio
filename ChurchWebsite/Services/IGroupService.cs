using ChurchWebsite.Models;

namespace ChurchWebsite.Services;

public interface IGroupService
{
    List<Group> GetAll();
    Group? GetById(int id);
}
