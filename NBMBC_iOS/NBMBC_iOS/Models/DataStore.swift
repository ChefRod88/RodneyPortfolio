import Foundation
import Combine

@MainActor
class DataStore: ObservableObject {
    @Published var sermons: [Sermon] = []
    @Published var events: [Event] = []
    @Published var groups: [Group] = []
    @Published var isLoading = false

    let church = ChurchInfo()

    init() {
        loadMockData()
    }

    // MARK: - Mock Data (replace with API calls)
    private func loadMockData() {
        let cal = Calendar.current
        let now = Date()

        sermons = [
            Sermon(id: 1, title: "Walking in Purpose", speaker: "Pastor",
                   series: "The Abundant Life", date: cal.date(byAdding: .day, value: -7, to: now)!,
                   description: "Discovering God's unique calling for your life and walking boldly in His purpose for you.",
                   videoURL: "https://www.youtube.com/watch?v=dQw4w9WgXcQ",
                   thumbnailSeed: "sermon1"),
            Sermon(id: 2, title: "Faith Over Fear", speaker: "Pastor",
                   series: "The Abundant Life", date: cal.date(byAdding: .day, value: -14, to: now)!,
                   description: "How to stand firm in faith when the storms of life are raging all around you.",
                   videoURL: nil, thumbnailSeed: "sermon2"),
            Sermon(id: 3, title: "The Power of Prayer", speaker: "Pastor",
                   series: "Prayer Warriors", date: cal.date(byAdding: .day, value: -21, to: now)!,
                   description: "Unlocking the transformative power of a consistent, fervent prayer life.",
                   videoURL: nil, thumbnailSeed: "sermon3"),
            Sermon(id: 4, title: "Grace That Amazes", speaker: "Minister Johnson",
                   series: "Amazing Grace", date: cal.date(byAdding: .day, value: -28, to: now)!,
                   description: "An exploration of God's unmerited favor and how it shapes our daily lives.",
                   videoURL: nil, thumbnailSeed: "sermon4"),
            Sermon(id: 5, title: "Community and Connection", speaker: "Pastor",
                   series: "Building Together", date: cal.date(byAdding: .day, value: -35, to: now)!,
                   description: "Why the local church community is essential to every believer's spiritual growth.",
                   videoURL: nil, thumbnailSeed: "sermon5"),
            Sermon(id: 6, title: "The Prodigal Son", speaker: "Minister Williams",
                   series: "Parables of Jesus", date: cal.date(byAdding: .day, value: -42, to: now)!,
                   description: "A deep dive into one of Jesus' most beloved parables about redemption and the Father's love.",
                   videoURL: nil, thumbnailSeed: "sermon6"),
        ]

        events = [
            Event(id: 1, title: "Sunday Morning Worship", date: nextSunday(from: now), endDate: nil,
                  location: "Main Sanctuary", description: "Join us for our weekly Sunday morning worship service. All are welcome!",
                  imageURL: nil, capacity: nil, icon: "music.note.house.fill"),
            Event(id: 2, title: "Men's Bible Study", date: cal.date(byAdding: .day, value: 3, to: now)!,
                  endDate: nil, location: "Fellowship Hall", description: "A powerful time in the Word for men of all ages.",
                  imageURL: nil, capacity: 40, icon: "book.fill"),
            Event(id: 3, title: "City Wide Mission Outreach", date: cal.date(byAdding: .day, value: 5, to: now)!,
                  endDate: nil, location: "Downtown Winter Haven", description: "Reaching the streets of Winter Haven with love, food, and the Gospel.",
                  imageURL: nil, capacity: nil, icon: "heart.fill"),
            Event(id: 4, title: "Youth Night", date: cal.date(byAdding: .day, value: 8, to: now)!,
                  endDate: nil, location: "Youth Center", description: "A fun and faith-filled evening for teens and young adults.",
                  imageURL: nil, capacity: 75, icon: "star.fill"),
            Event(id: 5, title: "Women's Tea & Fellowship", date: cal.date(byAdding: .day, value: 12, to: now)!,
                  endDate: nil, location: "Fellowship Hall", description: "Ladies, come enjoy fellowship, tea, and uplifting conversation.",
                  imageURL: nil, capacity: 50, icon: "cup.and.heat.waves.fill"),
            Event(id: 6, title: "Annual Church Picnic", date: cal.date(byAdding: .day, value: 18, to: now)!,
                  endDate: nil, location: "Cypress Gardens Park", description: "Bring the whole family for a day of food, fun, and fellowship!",
                  imageURL: nil, capacity: nil, icon: "tree.fill"),
        ]

        groups = [
            Group(id: 1, title: "Kids Ministry", category: "Children",
                  description: "A vibrant, safe place where children discover Jesus and grow in faith through exciting lessons, worship, and activities.",
                  icon: "figure.and.child.holdinghands", leader: "Sis. Davis", meetingTime: "Sundays at 10:30 AM", imageName: nil),
            Group(id: 2, title: "Men's Ministry", category: "Men",
                  description: "Men growing in faith, brotherhood, and servant leadership together. Iron sharpening iron.",
                  icon: "person.3.fill", leader: "Bro. Thompson", meetingTime: "Thursdays at 7 PM", imageName: nil),
            Group(id: 3, title: "Women's Ministry", category: "Women",
                  description: "A sisterhood of faith where women are encouraged, equipped, and empowered to live out their God-given purpose.",
                  icon: "person.2.fill", leader: "Sis. Williams", meetingTime: "Tuesdays at 6:30 PM", imageName: nil),
            Group(id: 4, title: "Youth Ministry", category: "Youth",
                  description: "Raising up the next generation of bold, faith-filled leaders through discipleship, fun, and real community.",
                  icon: "figure.2.arms.open", leader: "Min. Jackson", meetingTime: "Fridays at 7 PM", imageName: nil),
            Group(id: 5, title: "City Wide Mission", category: "Outreach",
                  description: "Reaching the streets of Winter Haven with love, food, and the Gospel. Serving our city in the name of Jesus.",
                  icon: "heart.circle.fill", leader: "Deacon Brown", meetingTime: "1st Saturdays at 9 AM", imageName: nil),
            Group(id: 6, title: "Senior Saints", category: "Seniors",
                  description: "Honoring and celebrating our faithful seniors through fellowship, Bible study, and shared wisdom.",
                  icon: "person.crop.circle.badge.checkmark", leader: "Min. Roberts", meetingTime: "Wednesdays at 11 AM", imageName: nil),
        ]
    }

    private func nextSunday(from date: Date) -> Date {
        var cal = Calendar.current
        cal.firstWeekday = 1
        var comps = cal.dateComponents([.weekday], from: date)
        let daysUntilSunday = (8 - (comps.weekday ?? 1)) % 7
        let offset = daysUntilSunday == 0 ? 7 : daysUntilSunday
        var target = cal.date(byAdding: .day, value: offset, to: date)!
        comps = cal.dateComponents([.year, .month, .day], from: target)
        comps.hour = 10
        comps.minute = 30
        comps.second = 0
        return cal.date(from: comps) ?? target
    }

    var latestSermons: [Sermon] { Array(sermons.prefix(3)) }
    var upcomingEvents: [Event] { Array(events.prefix(3)) }
}
