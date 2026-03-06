package com.rcdev.nbmbc.data

import java.time.LocalDate
import java.time.LocalDateTime
import java.time.format.DateTimeFormatter

// ── Church Info ──────────────────────────────────────────────────────────────
data class ChurchInfo(
    val name: String = "New Bethel Missionary Baptist Church",
    val shortName: String = "N.B.M.B.C.",
    val tagline: String = "Rooted in Faith. Committed to Community.",
    val location: String = "Winter Haven, Florida",
    val address: String = "2901 Avenue T NW, Winter Haven, FL 33881",
    val phone: String = "(863) 299-1226",
    val email: String = "info@nbmbcwh.org",
    val cashAppTag: String = "\$blueboy78",
    val cashAppUrl: String = "https://cash.app/\$blueboy78",
    val facebookUrl: String = "https://www.facebook.com/nbmbcwh",
    val youtubeUrl: String = "https://www.youtube.com/@nbmbcwh",
    val established: Int = 1965,
    val memberCount: String = "500+",
    val servicesPerYear: Int = 52,
    val missionStatement: String = "Rooted in Faith. Reaching the Lost. Raising the Next Generation.",
    val missionSubtext: String = "We are a Bible-believing, Spirit-filled church committed to loving God, serving our community, and sharing the Gospel of Jesus Christ throughout Winter Haven and beyond.",
    val beliefs: List<Belief> = defaultBeliefs()
)

data class Belief(
    val id: Int,
    val icon: String,
    val title: String,
    val description: String
)

fun defaultBeliefs() = listOf(
    Belief(1, "book", "The Bible", "We believe the Holy Bible is the inspired Word of God, our final authority for faith and practice."),
    Belief(2, "cross", "Salvation", "We believe in salvation by grace through faith in Jesus Christ alone — His death, burial, and resurrection."),
    Belief(3, "water_drop", "Baptism", "We practice believer's baptism by immersion as a public declaration of faith."),
    Belief(4, "people", "The Church", "We believe the local church is the body of Christ, called to worship, fellowship, and mission."),
)

// ── Sermon ───────────────────────────────────────────────────────────────────
data class Sermon(
    val id: Int,
    val title: String,
    val speaker: String,
    val series: String? = null,
    val date: LocalDate,
    val description: String? = null,
    val videoUrl: String? = null
) {
    val formattedDate: String get() = date.format(DateTimeFormatter.ofPattern("MMMM d, yyyy"))
}

// ── Event ────────────────────────────────────────────────────────────────────
data class Event(
    val id: Int,
    val title: String,
    val dateTime: LocalDateTime,
    val location: String? = null,
    val description: String? = null,
    val capacity: Int? = null,
    val icon: String = "event"
) {
    val formattedDate: String get() = dateTime.format(DateTimeFormatter.ofPattern("MMM d, yyyy"))
    val formattedTime: String get() = dateTime.format(DateTimeFormatter.ofPattern("h:mm a"))
    val dayNumber: String get() = dateTime.format(DateTimeFormatter.ofPattern("d"))
    val monthAbbr: String get() = dateTime.format(DateTimeFormatter.ofPattern("MMM")).uppercase()
}

// ── Group / Ministry ─────────────────────────────────────────────────────────
data class Group(
    val id: Int,
    val title: String,
    val category: String,
    val description: String,
    val icon: String,
    val leader: String? = null,
    val meetingTime: String? = null
)

// ── Prayer Categories ────────────────────────────────────────────────────────
data class PrayerCategory(
    val emoji: String,
    val label: String,
    val placeholder: String
)

val prayerCategories = listOf(
    PrayerCategory("🏥", "Healing", "I'm requesting prayer for healing and health..."),
    PrayerCategory("👨‍👩‍👧", "Family", "I'm requesting prayer for my family..."),
    PrayerCategory("💼", "Finances", "I'm requesting prayer for financial breakthrough..."),
    PrayerCategory("🌱", "Spiritual Growth", "I'm requesting prayer for spiritual direction..."),
    PrayerCategory("💙", "Grief & Loss", "I'm requesting prayer as I navigate grief..."),
    PrayerCategory("🙏", "General", "I have a prayer request I'd like to share...")
)
