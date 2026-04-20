namespace ChurchWebsite.Services;

public interface IChurchImageryRegistry
{
    ChurchPexelsPhoto? Get(string slot);
}
