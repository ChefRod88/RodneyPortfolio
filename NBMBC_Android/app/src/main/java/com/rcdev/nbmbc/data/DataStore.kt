package com.rcdev.nbmbc.data

import androidx.lifecycle.ViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.asStateFlow
import java.time.LocalDate
import java.time.LocalDateTime

class AppViewModel : ViewModel() {
    val church = ChurchInfo()

    private val _sermons = MutableStateFlow(mockSermons())
    val sermons = _sermons.asStateFlow()

    private val _events = MutableStateFlow(mockEvents())
    val events = _events.asStateFlow()

    private val _groups = MutableStateFlow(mockGroups())
    val groups = _groups.asStateFlow()

    val latestSermons get() = _sermons.value.take(3)
    val upcomingEvents get() = _events.value.take(3)
}

private fun mockSermons(): List<Sermon> {
    val today = LocalDate.now()
    return listOf(
        Sermon(1, "Walking in Purpose", "Pastor", "The Abundant Life", today.minusDays(7),
            "Discovering God's unique calling for your life and walking boldly in His purpose.", "https://www.youtube.com/watch?v=dQw4w9WgXcQ"),
        Sermon(2, "Faith Over Fear", "Pastor", "The Abundant Life", today.minusDays(14),
            "How to stand firm in faith when the storms of life are raging around you."),
        Sermon(3, "The Power of Prayer", "Pastor", "Prayer Warriors", today.minusDays(21),
            "Unlocking the transformative power of a consistent, fervent prayer life."),
        Sermon(4, "Grace That Amazes", "Minister Johnson", "Amazing Grace", today.minusDays(28),
            "An exploration of God's unmerited favor and how it shapes our daily lives."),
        Sermon(5, "Community and Connection", "Pastor", "Building Together", today.minusDays(35),
            "Why the local church community is essential to every believer's spiritual growth."),
        Sermon(6, "The Prodigal Son", "Minister Williams", "Parables of Jesus", today.minusDays(42),
            "A deep dive into one of Jesus' most beloved parables about redemption."),
    )
}

private fun mockEvents(): List<Event> {
    val now = LocalDateTime.now()
    return listOf(
        Event(1, "Sunday Morning Worship", now.plusDays(6).withHour(10).withMinute(30),
            "Main Sanctuary", "Join us for our weekly Sunday morning worship service. All are welcome!", null, "church"),
        Event(2, "Men's Bible Study", now.plusDays(3).withHour(19).withMinute(0),
            "Fellowship Hall", "A powerful time in the Word for men of all ages.", 40, "book"),
        Event(3, "City Wide Mission Outreach", now.plusDays(5).withHour(9).withMinute(0),
            "Downtown Winter Haven", "Reaching the streets of Winter Haven with love, food, and the Gospel.", null, "favorite"),
        Event(4, "Youth Night", now.plusDays(8).withHour(19).withMinute(0),
            "Youth Center", "A fun and faith-filled evening for teens and young adults.", 75, "star"),
        Event(5, "Women's Tea & Fellowship", now.plusDays(12).withHour(14).withMinute(0),
            "Fellowship Hall", "Ladies, come enjoy fellowship, tea, and uplifting conversation.", 50, "local_cafe"),
        Event(6, "Annual Church Picnic", now.plusDays(18).withHour(11).withMinute(0),
            "Cypress Gardens Park", "Bring the whole family for a day of food, fun, and fellowship!", null, "park"),
    )
}

private fun mockGroups(): List<Group> = listOf(
    Group(1, "Kids Ministry", "Children", "A vibrant, safe place where children discover Jesus and grow in faith through exciting lessons, worship, and activities.", "child_care", "Sis. Davis", "Sundays at 10:30 AM"),
    Group(2, "Men's Ministry", "Men", "Men growing in faith, brotherhood, and servant leadership together. Iron sharpening iron.", "group", "Bro. Thompson", "Thursdays at 7 PM"),
    Group(3, "Women's Ministry", "Women", "A sisterhood of faith where women are encouraged, equipped, and empowered to live out their God-given purpose.", "people", "Sis. Williams", "Tuesdays at 6:30 PM"),
    Group(4, "Youth Ministry", "Youth", "Raising up the next generation of bold, faith-filled leaders through discipleship, fun, and real community.", "sports_esports", "Min. Jackson", "Fridays at 7 PM"),
    Group(5, "City Wide Mission", "Outreach", "Reaching the streets of Winter Haven with love, food, and the Gospel. Serving our city in the name of Jesus.", "favorite", "Deacon Brown", "1st Saturdays at 9 AM"),
    Group(6, "Senior Saints", "Seniors", "Honoring and celebrating our faithful seniors through fellowship, Bible study, and shared wisdom.", "elderly", "Min. Roberts", "Wednesdays at 11 AM"),
)
