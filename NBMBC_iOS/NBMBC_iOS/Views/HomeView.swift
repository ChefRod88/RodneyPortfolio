import SwiftUI

struct HomeView: View {
    @EnvironmentObject var store: DataStore
    @Binding var showGive: Bool
    @State private var navigateToAbout = false
    @State private var navigateToSermons = false
    @State private var navigateToEvents = false
    @State private var navigateToLive = false

    var body: some View {
        NavigationStack {
            ZStack {
                Theme.background.ignoresSafeArea()
                ConstellationView(particleCount: 70)
                    .ignoresSafeArea()
                    .opacity(0.55)

                ScrollView(showsIndicators: false) {
                    LazyVStack(spacing: 0) {
                        HeroSection(showGive: $showGive, navigateToAbout: $navigateToAbout, navigateToLive: $navigateToLive)
                        StatsBar()
                        ThreeCards(navigateToAbout: $navigateToAbout, navigateToEvents: $navigateToEvents)
                        MarqueeBanner()
                        MissionSection(navigateToAbout: $navigateToAbout)
                        MinistriesSection()
                        SermonsPreviewSection(sermons: store.latestSermons, navigateToSermons: $navigateToSermons)
                        GiveSection(showGive: $showGive)
                        Spacer(minLength: 120)
                    }
                }
            }
            .navigationDestination(isPresented: $navigateToAbout) { AboutView() }
            .navigationDestination(isPresented: $navigateToLive)  { LiveView() }
            .navigationDestination(isPresented: $navigateToEvents) { EventsView() }
            .navigationDestination(isPresented: $navigateToSermons) { SermonsView() }
            .navigationBarHidden(true)
        }
    }
}

// MARK: - Hero Section
private struct HeroSection: View {
    @Binding var showGive: Bool
    @Binding var navigateToAbout: Bool
    @Binding var navigateToLive: Bool

    var body: some View {
        ZStack(alignment: .bottom) {
            // Background image placeholder (use AsyncImage with real URL)
            LinearGradient(
                colors: [Theme.surface2, Theme.surface3, Theme.background],
                startPoint: .top,
                endPoint: .bottom
            )
            .frame(height: UIScreen.main.bounds.height * 0.82)
            .overlay {
                // Simulated hero image pattern
                GeometryReader { geo in
                    ZStack {
                        RadialGradient(
                            colors: [Theme.gold.opacity(0.12), .clear],
                            center: .center,
                            startRadius: 0,
                            endRadius: geo.size.width * 0.8
                        )
                    }
                }
            }

            // Vignette
            LinearGradient(
                colors: [.clear, Theme.background.opacity(0.6), Theme.background],
                startPoint: .top,
                endPoint: .bottom
            )

            // Content
            VStack(spacing: 0) {
                Spacer()
                VStack(spacing: 24) {
                    EyebrowBadge(title: "Winter Haven, Florida")

                    VStack(spacing: 12) {
                        Text("Rooted In Faith.")
                            .font(.system(size: 42, weight: .black, design: .rounded))
                        Text("Committed To")
                            .font(.system(size: 42, weight: .black, design: .rounded))
                        Text("Community.")
                            .font(.system(size: 42, weight: .black, design: .rounded))
                    }
                    .multilineTextAlignment(.center)
                    .foregroundStyle(
                        LinearGradient(
                            colors: [.white, Theme.goldLight, .white, Theme.gold],
                            startPoint: .topLeading,
                            endPoint: .bottomTrailing
                        )
                    )

                    Text("Every Sunday we gather, worship, and grow\ntogether as one family in Christ.")
                        .font(.system(size: 16, weight: .regular))
                        .foregroundColor(Theme.textMuted)
                        .multilineTextAlignment(.center)
                        .lineSpacing(4)

                    HStack(spacing: 12) {
                        GoldButton(title: "Plan A Visit", icon: "arrow.right") {
                            navigateToAbout = true
                        }
                        GhostButton(title: "Watch Live", icon: "play.fill") {
                            navigateToLive = true
                        }
                    }
                }
                .padding(.horizontal, 24)
                .padding(.bottom, 48)
            }
        }
        .frame(height: UIScreen.main.bounds.height * 0.82)
    }
}

// MARK: - Stats Bar
private struct StatsBar: View {
    @State private var animated = false

    private let stats = [
        ("1965", "Established"),
        ("500+", "Members"),
        ("52",   "Services / Year"),
        ("1",    "Community"),
    ]

    var body: some View {
        HStack(spacing: 0) {
            ForEach(stats, id: \.0) { stat in
                VStack(spacing: 4) {
                    Text(stat.0)
                        .font(.system(size: 28, weight: .black, design: .rounded))
                        .foregroundStyle(Theme.goldTextGradient)
                    Text(stat.1)
                        .font(.system(size: 10, weight: .semibold))
                        .tracking(2)
                        .textCase(.uppercase)
                        .foregroundColor(Theme.textMuted)
                }
                .frame(maxWidth: .infinity)
                if stat.1 != stats.last!.1 {
                    Divider()
                        .background(Theme.goldBorder)
                        .frame(height: 36)
                }
            }
        }
        .padding(.vertical, 24)
        .padding(.horizontal, 16)
        .background(Theme.surface1)
        .overlay(alignment: .top)    { Divider().background(Theme.goldBorder) }
        .overlay(alignment: .bottom) { Divider().background(Theme.goldBorder) }
    }
}

// MARK: - Three Feature Cards
private struct ThreeCards: View {
    @Binding var navigateToAbout: Bool
    @Binding var navigateToEvents: Bool
    @State private var navigateToGroups = false

    private let cards: [(eye: String, title: String, cta: String, gradient: [Color], icon: String)] = [
        ("Who We Are",     "Get To Know Us",   "Learn More",    [Color(hex: "1C1C38"), Color(hex: "0E0E1C")], "person.3.fill"),
        ("What's Happening", "Events",          "See Schedule",  [Color(hex: "14142A"), Color(hex: "0E0E1C")], "calendar.badge.plus"),
        ("Your Journey",   "Next Steps",        "Get Connected", [Color(hex: "1C1C38"), Color(hex: "14142A")], "figure.walk"),
    ]

    var body: some View {
        NavigationLink(destination: GroupsView(), isActive: $navigateToGroups) { EmptyView() }

        VStack(spacing: 0) {
            ForEach(Array(cards.enumerated()), id: \.offset) { index, card in
                Button {
                    if index == 0 { navigateToAbout = true }
                    else if index == 1 { navigateToEvents = true }
                    else { navigateToGroups = true }
                } label: {
                    FeatureCard(eye: card.eye, title: card.title, cta: card.cta,
                                gradient: card.gradient, icon: card.icon)
                }
                .buttonStyle(.plain)
            }
        }
    }
}

private struct FeatureCard: View {
    let eye: String
    let title: String
    let cta: String
    let gradient: [Color]
    let icon: String
    @State private var hovered = false

    var body: some View {
        ZStack(alignment: .bottomLeading) {
            LinearGradient(colors: gradient, startPoint: .topLeading, endPoint: .bottomTrailing)
                .frame(height: 220)

            // Icon watermark
            Image(systemName: icon)
                .font(.system(size: 80))
                .foregroundColor(Theme.gold.opacity(0.08))
                .frame(maxWidth: .infinity, maxHeight: .infinity, alignment: .trailing)
                .padding(.trailing, 24)
                .padding(.bottom, 20)

            // Gradient overlay
            LinearGradient(
                colors: [.clear, Theme.background.opacity(0.85)],
                startPoint: .top,
                endPoint: .bottom
            )

            // Content
            VStack(alignment: .leading, spacing: 4) {
                Text(eye)
                    .font(.system(size: 11, weight: .semibold))
                    .tracking(3)
                    .textCase(.uppercase)
                    .foregroundColor(Theme.gold)
                Text(title)
                    .font(.system(size: 24, weight: .bold))
                    .foregroundColor(.white)
                HStack(spacing: 4) {
                    Text(cta)
                    Image(systemName: "arrow.right")
                }
                .font(.system(size: 12, weight: .semibold))
                .tracking(1.5)
                .textCase(.uppercase)
                .foregroundColor(Theme.goldLight)
            }
            .padding(28)

            // Gold bottom bar
            LinearGradient(
                colors: [.clear, Theme.gold, .clear],
                startPoint: .leading,
                endPoint: .trailing
            )
            .frame(height: 2)
            .frame(maxWidth: .infinity, maxHeight: .infinity, alignment: .bottom)
        }
        .frame(height: 220)
        .clipped()
    }
}

// MARK: - Marquee Banner
private struct MarqueeBanner: View {
    var body: some View {
        TimelineView(.animation) { tl in
            let elapsed = tl.date.timeIntervalSinceReferenceDate
            Canvas { ctx, size in
                let text = "WELCOME TO NEW BETHEL  ✦  WELCOME TO NEW BETHEL  ✦  WELCOME TO NEW BETHEL  ✦  "
                let attrs: [NSAttributedString.Key: Any] = [
                    .font: UIFont.systemFont(ofSize: 13, weight: .bold),
                    .foregroundColor: UIColor(Theme.gold),
                    .kern: 5
                ]
                let attrString = NSAttributedString(string: text, attributes: attrs)
                let textWidth = attrString.size().width
                let offset = (elapsed * 40).truncatingRemainder(dividingBy: textWidth)
                let startX = -offset
                var x = startX
                while x < size.width + textWidth {
                    ctx.draw(Text(AttributedString(attrString)).tracking(5), at: CGPoint(x: x + textWidth / 2, y: size.height / 2))
                    x += textWidth
                }
            }
        }
        .frame(height: 44)
        .background(Theme.surface2)
        .overlay(alignment: .top)    { Divider().background(Theme.goldBorder) }
        .overlay(alignment: .bottom) { Divider().background(Theme.goldBorder) }
    }
}

// MARK: - Mission Section
private struct MissionSection: View {
    @EnvironmentObject var store: DataStore
    @Binding var navigateToAbout: Bool

    var body: some View {
        ZStack {
            LinearGradient(
                colors: [Theme.background, Theme.surface2, Theme.background],
                startPoint: .topLeading,
                endPoint: .bottomTrailing
            )
            RadialGradient(
                colors: [Theme.gold.opacity(0.07), .clear],
                center: .center,
                startRadius: 0,
                endRadius: 300
            )

            VStack(spacing: 20) {
                Text("\"")
                    .font(.system(size: 80, weight: .black, design: .serif))
                    .foregroundColor(Theme.gold.opacity(0.10))
                    .offset(y: 20)

                EyebrowBadge(title: "Our Purpose", showDot: false)

                Text(store.church.missionStatement)
                    .font(.system(size: 26, weight: .bold, design: .rounded))
                    .multilineTextAlignment(.center)
                    .foregroundColor(.white)

                // Gold divider
                Rectangle()
                    .fill(
                        LinearGradient(colors: [.clear, Theme.gold, .clear], startPoint: .leading, endPoint: .trailing)
                    )
                    .frame(width: 56, height: 2)

                Text(store.church.missionSubtext)
                    .font(.system(size: 15))
                    .foregroundColor(Theme.textMuted)
                    .multilineTextAlignment(.center)
                    .lineSpacing(5)

                GoldButton(title: "What We Believe", icon: "book.fill") {
                    navigateToAbout = true
                }
            }
            .padding(36)
        }
    }
}

// MARK: - Ministries Section
private struct MinistriesSection: View {
    @State private var navigateToGroups = false

    private let ministries = [
        (tag: "Ministry",  title: "Kids",             desc: "A vibrant, safe place where children discover Jesus and grow in faith.",               icon: "figure.and.child.holdinghands"),
        (tag: "Ministry",  title: "Men's Ministry",   desc: "Men growing in faith, brotherhood, and servant leadership together.",                  icon: "person.3.fill"),
        (tag: "Outreach",  title: "City Wide Mission", desc: "Reaching the streets of Winter Haven with love and the Gospel.",                      icon: "heart.circle.fill"),
    ]

    var body: some View {
        NavigationLink(destination: GroupsView(), isActive: $navigateToGroups) { EmptyView() }

        VStack(alignment: .leading, spacing: 20) {
            VStack(alignment: .leading, spacing: 6) {
                Text("GET INVOLVED")
                    .font(.system(size: 11, weight: .semibold))
                    .tracking(3)
                    .foregroundColor(Theme.gold)
                Text("Get Connected")
                    .font(.system(size: 28, weight: .bold, design: .rounded))
                    .foregroundColor(.white)
            }

            ScrollView(.horizontal, showsIndicators: false) {
                HStack(spacing: 14) {
                    ForEach(ministries, id: \.title) { min in
                        MinistryCard(tag: min.tag, title: min.title, desc: min.desc, icon: min.icon) {
                            navigateToGroups = true
                        }
                    }
                }
                .padding(.horizontal, 2)
            }
        }
        .padding(.horizontal, 20)
        .padding(.vertical, 36)
        .background(Theme.surface1)
    }
}

private struct MinistryCard: View {
    let tag: String
    let title: String
    let desc: String
    let icon: String
    let action: () -> Void

    var body: some View {
        Button(action: action) {
            VStack(alignment: .leading, spacing: 10) {
                ZStack {
                    RoundedRectangle(cornerRadius: 16, style: .continuous)
                        .fill(
                            LinearGradient(
                                colors: [Theme.surface2, Theme.surface3],
                                startPoint: .topLeading,
                                endPoint: .bottomTrailing
                            )
                        )
                    Image(systemName: icon)
                        .font(.system(size: 44))
                        .foregroundStyle(Theme.goldTextGradient)
                }
                .frame(width: 200, height: 130)

                Text(tag.uppercased())
                    .font(.system(size: 10, weight: .semibold))
                    .tracking(2.5)
                    .foregroundColor(Theme.gold)
                Text(title)
                    .font(.system(size: 18, weight: .bold))
                    .foregroundColor(.white)
                Text(desc)
                    .font(.system(size: 13))
                    .foregroundColor(Theme.textMuted)
                    .lineSpacing(3)
                    .lineLimit(3)

                HStack(spacing: 4) {
                    Text("Learn More")
                    Image(systemName: "arrow.right")
                }
                .font(.system(size: 12, weight: .semibold))
                .foregroundColor(Theme.gold)
            }
            .frame(width: 200)
            .padding(16)
            .background {
                RoundedRectangle(cornerRadius: 20, style: .continuous)
                    .fill(Theme.surface1)
                    .overlay {
                        RoundedRectangle(cornerRadius: 20, style: .continuous)
                            .stroke(Theme.goldBorder, lineWidth: 1)
                    }
            }
        }
        .buttonStyle(.plain)
    }
}

// MARK: - Sermons Preview Section
private struct SermonsPreviewSection: View {
    let sermons: [Sermon]
    @Binding var navigateToSermons: Bool

    var body: some View {
        VStack(alignment: .leading, spacing: 20) {
            HStack(alignment: .top) {
                VStack(alignment: .leading, spacing: 6) {
                    Text("WORD OF GOD")
                        .font(.system(size: 11, weight: .semibold))
                        .tracking(3)
                        .foregroundColor(Theme.gold)
                    Text("Latest Sermons")
                        .font(.system(size: 26, weight: .bold, design: .rounded))
                        .foregroundColor(.white)
                    Text("Catch up on our latest teachings.")
                        .font(.system(size: 14))
                        .foregroundColor(Theme.textMuted)
                }
                Spacer()
                GoldButton(title: "All") {
                    navigateToSermons = true
                }
            }

            if sermons.isEmpty {
                Text("Check back soon for new messages.")
                    .foregroundColor(Theme.textMuted)
                    .frame(maxWidth: .infinity)
                    .padding(.vertical, 40)
            } else {
                ForEach(sermons) { sermon in
                    NavigationLink(destination: SermonDetailView(sermon: sermon)) {
                        SermonRowCard(sermon: sermon)
                    }
                    .buttonStyle(.plain)
                }
            }
        }
        .padding(20)
        .background(Theme.background)
    }
}

private struct SermonRowCard: View {
    let sermon: Sermon

    var body: some View {
        HStack(spacing: 14) {
            ZStack {
                RoundedRectangle(cornerRadius: 12, style: .continuous)
                    .fill(LinearGradient(colors: [Theme.surface2, Theme.surface3], startPoint: .topLeading, endPoint: .bottomTrailing))
                    .frame(width: 80, height: 60)
                Image(systemName: "play.circle.fill")
                    .font(.system(size: 28))
                    .foregroundColor(Theme.gold.opacity(0.8))
            }

            VStack(alignment: .leading, spacing: 4) {
                if let series = sermon.series {
                    Text(series.uppercased())
                        .font(.system(size: 10, weight: .semibold))
                        .tracking(2)
                        .foregroundColor(Theme.gold)
                }
                Text(sermon.title)
                    .font(.system(size: 15, weight: .semibold))
                    .foregroundColor(.white)
                    .lineLimit(2)
                Text("\(sermon.speaker)  •  \(sermon.formattedDate)")
                    .font(.system(size: 12))
                    .foregroundColor(Theme.textMuted)
            }
            Spacer()
            Image(systemName: "chevron.right")
                .font(.system(size: 12, weight: .semibold))
                .foregroundColor(Theme.textMuted)
        }
        .padding(14)
        .background {
            RoundedRectangle(cornerRadius: 14, style: .continuous)
                .fill(Theme.surface1)
                .overlay {
                    RoundedRectangle(cornerRadius: 14, style: .continuous)
                        .stroke(Theme.goldBorder, lineWidth: 1)
                }
        }
    }
}

// MARK: - Give Section
private struct GiveSection: View {
    @Binding var showGive: Bool

    var body: some View {
        ZStack {
            LinearGradient(
                colors: [Theme.surface2, Theme.background, Theme.surface1],
                startPoint: .topLeading,
                endPoint: .bottomTrailing
            )
            RadialGradient(
                colors: [Theme.gold.opacity(0.07), .clear],
                center: .center,
                startRadius: 0,
                endRadius: 250
            )

            VStack(spacing: 24) {
                VStack(spacing: 10) {
                    Text("SUPPORT THE MINISTRY")
                        .font(.system(size: 11, weight: .semibold))
                        .tracking(3)
                        .foregroundColor(Theme.gold)
                    Text("Give & Make An Impact")
                        .font(.system(size: 28, weight: .bold, design: .rounded))
                        .foregroundColor(.white)
                        .multilineTextAlignment(.center)
                    Rectangle()
                        .fill(LinearGradient(colors: [.clear, Theme.gold, .clear], startPoint: .leading, endPoint: .trailing))
                        .frame(width: 56, height: 2)
                }

                Text("Your generosity powers the mission — feeding the hungry, reaching the streets of Winter Haven, and sharing the Gospel of Jesus Christ.")
                    .font(.system(size: 15))
                    .foregroundColor(Theme.textMuted)
                    .multilineTextAlignment(.center)
                    .lineSpacing(4)

                // Scripture
                VStack(alignment: .leading, spacing: 8) {
                    Text(""Each of you should give what you have decided in your heart to give, not reluctantly or under compulsion, for God loves a cheerful giver."")
                        .font(.system(size: 14, weight: .regular, design: .serif))
                        .italic()
                        .foregroundColor(Theme.textPrimary.opacity(0.7))
                    Text("— 2 Corinthians 9:7")
                        .font(.system(size: 11, weight: .semibold))
                        .tracking(1.5)
                        .textCase(.uppercase)
                        .foregroundColor(Theme.gold)
                }
                .padding(16)
                .background {
                    RoundedRectangle(cornerRadius: 12, style: .continuous)
                        .fill(Theme.gold.opacity(0.06))
                        .overlay(alignment: .leading) {
                            Rectangle()
                                .fill(Theme.gold)
                                .frame(width: 2)
                                .cornerRadius(2)
                        }
                }

                GoldButton(title: "Give via Cash App", icon: "dollarsign.circle.fill", fullWidth: true) {
                    showGive = true
                }
            }
            .padding(28)
        }
    }
}

#Preview {
    HomeView(showGive: .constant(false))
        .environmentObject(DataStore())
}
