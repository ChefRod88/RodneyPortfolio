import Foundation

// MARK: - Church Info
struct ChurchInfo {
    let name = "New Bethel Missionary Baptist Church"
    let shortName = "N.B.M.B.C."
    let tagline = "Rooted in Faith. Committed to Community."
    let location = "Winter Haven, Florida"
    let address = "2901 Avenue T NW, Winter Haven, FL 33881"
    let phone = "(863) 299-1226"
    let email = "info@nbmbcwh.org"
    let cashAppTag = "$blueboy78"
    let cashAppURL = "https://cash.app/$blueboy78"
    let facebookURL = "https://www.facebook.com/nbmbcwh"
    let youtubeURL = "https://www.youtube.com/@nbmbcwh"
    let established = 1965
    let memberCount = "500+"
    let servicesPerYear = 52
    let missionStatement = "Rooted in Faith. Reaching the Lost. Raising the Next Generation."
    let missionSubtext = "We are a Bible-believing, Spirit-filled church committed to loving God, serving our community, and sharing the Gospel of Jesus Christ throughout Winter Haven and beyond."
    let beliefs: [Belief] = [
        Belief(icon: "book.fill",       title: "The Bible",      description: "We believe the Holy Bible is the inspired Word of God, our final authority for faith and practice."),
        Belief(icon: "cross.fill",      title: "Salvation",      description: "We believe in salvation by grace through faith in Jesus Christ alone — His death, burial, and resurrection."),
        Belief(icon: "drop.fill",       title: "Baptism",        description: "We practice believer's baptism by immersion as a public declaration of faith."),
        Belief(icon: "figure.2.and.child.holdinghands", title: "The Church", description: "We believe the local church is the body of Christ, called to worship, fellowship, and mission."),
    ]
}

struct Belief: Identifiable {
    let id = UUID()
    let icon: String
    let title: String
    let description: String
}

// MARK: - Sermon
struct Sermon: Identifiable {
    let id: Int
    let title: String
    let speaker: String
    let series: String?
    let date: Date
    let description: String?
    let videoURL: String?
    let thumbnailSeed: String

    var formattedDate: String {
        let f = DateFormatter()
        f.dateStyle = .long
        return f.string(from: date)
    }
}

// MARK: - Event
struct Event: Identifiable {
    let id: Int
    let title: String
    let date: Date
    let endDate: Date?
    let location: String?
    let description: String?
    let imageURL: String?
    let capacity: Int?
    let icon: String

    var formattedDate: String {
        let f = DateFormatter()
        f.dateFormat = "MMM d, yyyy"
        return f.string(from: date)
    }

    var formattedTime: String {
        let f = DateFormatter()
        f.dateFormat = "h:mm a"
        return f.string(from: date)
    }

    var dayNumber: String {
        let f = DateFormatter()
        f.dateFormat = "d"
        return f.string(from: date)
    }

    var monthAbbr: String {
        let f = DateFormatter()
        f.dateFormat = "MMM"
        return f.string(from: date).uppercased()
    }
}

// MARK: - Group / Ministry
struct Group: Identifiable {
    let id: Int
    let title: String
    let category: String
    let description: String
    let icon: String
    let leader: String?
    let meetingTime: String?
    let imageName: String?
}

// MARK: - Prayer Request
struct PrayerRequest {
    var name: String = ""
    var email: String = ""
    var request: String = ""
    var category: String = "General"
    var shareAnonymously: Bool = false
}

// MARK: - Prayer Category
struct PrayerCategory: Identifiable {
    let id = UUID()
    let icon: String
    let label: String
    let placeholder: String
}
