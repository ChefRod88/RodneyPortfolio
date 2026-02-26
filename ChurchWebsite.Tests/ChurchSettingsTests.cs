using ChurchWebsite.Models;

namespace ChurchWebsite.Tests;

public class ChurchSettingsTests
{
    [Fact]
    public void GoogleMapsUrl_UsesFullAddressFromSettings()
    {
        var settings = new ChurchSettings
        {
            Address = new AddressSettings
            {
                Street = "123 Ave Y NE",
                City = "Winter Haven",
                State = "FL",
                Zip = "33881"
            }
        };

        var url = settings.GoogleMapsUrl;

        Assert.Contains("https://www.google.com/maps/search/?api=1&query=", url);
        Assert.Contains("123%20Ave%20Y%20NE%2C%20Winter%20Haven%2C%20FL%2033881", url);
    }

    [Fact]
    public void RoutingDestination_ActsAsSingleSourceOfTruth()
    {
        var settings = new ChurchSettings
        {
            Routing = new RoutingSettings
            {
                ChurchDestination = new ChurchDestinationSettings
                {
                    Name = "New Bethel Missionary Baptist Church",
                    AddressLabel = "123 Ave Y NE, Winter Haven, FL 33881",
                    Latitude = 28.03064,
                    Longitude = -81.72895
                }
            }
        };

        Assert.Equal("New Bethel Missionary Baptist Church", settings.Routing.ChurchDestination.Name);
        Assert.Equal("123 Ave Y NE, Winter Haven, FL 33881", settings.Routing.ChurchDestination.AddressLabel);
        Assert.Equal(28.03064, settings.Routing.ChurchDestination.Latitude);
        Assert.Equal(-81.72895, settings.Routing.ChurchDestination.Longitude);
    }
}
