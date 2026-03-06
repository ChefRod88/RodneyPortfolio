import SwiftUI

struct AboutView: View {
    @EnvironmentObject var store: DataStore
    @State private var showGive = false

    var body: some View {
        ZStack {
            Theme.background.ignoresSafeArea()
            ConstellationView(particleCount: 40)
                .ignoresSafeArea()
                .opacity(0.4)

            ScrollView(showsIndicators: false) {
                LazyVStack(spacing: 0) {
                    // Hero
                    AboutHero(church: store.church)

                    // Mission
                    MissionBlock(church: store.church)

                    // Beliefs
                    BeliefsSection(beliefs: store.church.beliefs)

                    // Stats
                    AboutStatsSection(church: store.church)

                    // Leadership
                    LeadershipSection()

                    // Contact
                    ContactSection(church: store.church, showGive: $showGive)

                    Spacer(minLength: 120)
                }
            }
        }
        .navigationTitle("About")
        .navigationBarTitleDisplayMode(.inline)
        .toolbarColorScheme(.dark, for: .navigationBar)
        .toolbarBackground(Theme.surface1, for: .navigationBar)
        .sheet(isPresented: $showGive) { GiveView() }
    }
}

private struct AboutHero: View {
    let church: ChurchInfo

    var body: some View {
        ZStack {
            LinearGradient(colors: [Theme.surface2, Theme.surface3, Theme.background], startPoint: .top, endPoint: .bottom)
            RadialGradient(colors: [Theme.gold.opacity(0.10), .clear], center: .center, startRadius: 0, endRadius: 300)

            VStack(spacing: 20) {
                // Church emblem
                ZStack {
                    Circle()
                        .fill(Theme.goldGradient)
                        .frame(width: 90, height: 90)
                        .shadow(color: Theme.goldGlow, radius: 24)
                    Image(systemName: "building.columns.fill")
                        .font(.system(size: 38))
                        .foregroundColor(Theme.background)
                }

                Text(church.name)
                    .font(.system(size: 28, weight: .black, design: .rounded))
                    .foregroundStyle(
                        LinearGradient(colors: [.white, Theme.goldLight, Theme.gold], startPoint: .topLeading, endPoint: .bottomTrailing)
                    )
                    .multilineTextAlignment(.center)

                Text("Est. \(church.established)  •  \(church.location)")
                    .font(.system(size: 13, weight: .semibold))
                    .tracking(2)
                    .textCase(.uppercase)
                    .foregroundColor(Theme.textMuted)
            }
            .padding(36)
        }
    }
}

private struct MissionBlock: View {
    let church: ChurchInfo

    var body: some View {
        VStack(spacing: 20) {
            Text("OUR MISSION")
                .font(.system(size: 11, weight: .semibold))
                .tracking(3)
                .foregroundColor(Theme.gold)
            Text(church.missionStatement)
                .font(.system(size: 22, weight: .bold, design: .rounded))
                .foregroundColor(.white)
                .multilineTextAlignment(.center)
            Rectangle()
                .fill(LinearGradient(colors: [.clear, Theme.gold, .clear], startPoint: .leading, endPoint: .trailing))
                .frame(width: 56, height: 2)
            Text(church.missionSubtext)
                .font(.system(size: 15))
                .foregroundColor(Theme.textMuted)
                .multilineTextAlignment(.center)
                .lineSpacing(5)
        }
        .padding(28)
        .background(
            LinearGradient(colors: [Theme.background, Theme.surface2, Theme.background], startPoint: .topLeading, endPoint: .bottomTrailing)
        )
    }
}

private struct BeliefsSection: View {
    let beliefs: [Belief]

    var body: some View {
        VStack(alignment: .leading, spacing: 20) {
            VStack(alignment: .leading, spacing: 6) {
                Text("WHAT WE BELIEVE")
                    .font(.system(size: 11, weight: .semibold))
                    .tracking(3)
                    .foregroundColor(Theme.gold)
                Text("Core Beliefs")
                    .font(.system(size: 26, weight: .bold, design: .rounded))
                    .foregroundColor(.white)
            }

            ForEach(beliefs) { belief in
                HStack(alignment: .top, spacing: 14) {
                    ZStack {
                        Circle()
                            .fill(Theme.gold.opacity(0.12))
                            .frame(width: 44, height: 44)
                        Image(systemName: belief.icon)
                            .font(.system(size: 18))
                            .foregroundStyle(Theme.goldTextGradient)
                    }
                    VStack(alignment: .leading, spacing: 4) {
                        Text(belief.title)
                            .font(.system(size: 16, weight: .bold))
                            .foregroundColor(.white)
                        Text(belief.description)
                            .font(.system(size: 14))
                            .foregroundColor(Theme.textMuted)
                            .lineSpacing(4)
                    }
                }
            }
        }
        .padding(24)
        .background(Theme.surface1)
    }
}

private struct AboutStatsSection: View {
    let church: ChurchInfo

    var body: some View {
        HStack(spacing: 0) {
            StatBlock(value: "\(church.established)", label: "Established")
            Divider().background(Theme.goldBorder).frame(height: 40)
            StatBlock(value: church.memberCount, label: "Members")
            Divider().background(Theme.goldBorder).frame(height: 40)
            StatBlock(value: "\(church.servicesPerYear)", label: "Services/Yr")
        }
        .padding(.vertical, 28)
        .background(Theme.background)
        .overlay(alignment: .top) { Divider().background(Theme.goldBorder) }
        .overlay(alignment: .bottom) { Divider().background(Theme.goldBorder) }
    }
}

private struct StatBlock: View {
    let value: String
    let label: String

    var body: some View {
        VStack(spacing: 4) {
            Text(value)
                .font(.system(size: 28, weight: .black, design: .rounded))
                .foregroundStyle(Theme.goldTextGradient)
            Text(label)
                .font(.system(size: 10, weight: .semibold))
                .tracking(2)
                .textCase(.uppercase)
                .foregroundColor(Theme.textMuted)
        }
        .frame(maxWidth: .infinity)
    }
}

private struct LeadershipSection: View {
    var body: some View {
        VStack(alignment: .leading, spacing: 20) {
            VStack(alignment: .leading, spacing: 6) {
                Text("OUR LEADERSHIP")
                    .font(.system(size: 11, weight: .semibold))
                    .tracking(3)
                    .foregroundColor(Theme.gold)
                Text("Who Leads Us")
                    .font(.system(size: 26, weight: .bold, design: .rounded))
                    .foregroundColor(.white)
            }

            LeaderCard(name: "Senior Pastor", role: "Lead Pastor", description: "Leading New Bethel with vision, faith, and a heart for the community of Winter Haven.")
            LeaderCard(name: "Deacon Board", role: "Servant Leaders", description: "Faithful men who serve the congregation and support the mission of the church.")
            LeaderCard(name: "Minister Team", role: "Ministry Staff", description: "Gifted ministers who teach, preach, and shepherd the flock of New Bethel.")
        }
        .padding(24)
        .background(Theme.surface1)
    }
}

private struct LeaderCard: View {
    let name: String
    let role: String
    let description: String

    var body: some View {
        HStack(spacing: 14) {
            ZStack {
                Circle()
                    .fill(Theme.goldGradient)
                    .frame(width: 52, height: 52)
                Image(systemName: "person.fill")
                    .font(.system(size: 22))
                    .foregroundColor(Theme.background)
            }
            VStack(alignment: .leading, spacing: 3) {
                Text(name)
                    .font(.system(size: 16, weight: .bold))
                    .foregroundColor(.white)
                Text(role)
                    .font(.system(size: 12, weight: .semibold))
                    .tracking(1)
                    .textCase(.uppercase)
                    .foregroundColor(Theme.gold)
                Text(description)
                    .font(.system(size: 13))
                    .foregroundColor(Theme.textMuted)
                    .lineLimit(2)
            }
        }
    }
}

private struct ContactSection: View {
    let church: ChurchInfo
    @Binding var showGive: Bool

    var body: some View {
        VStack(alignment: .leading, spacing: 20) {
            VStack(alignment: .leading, spacing: 6) {
                Text("GET IN TOUCH")
                    .font(.system(size: 11, weight: .semibold))
                    .tracking(3)
                    .foregroundColor(Theme.gold)
                Text("Contact Us")
                    .font(.system(size: 26, weight: .bold, design: .rounded))
                    .foregroundColor(.white)
            }

            ContactRow(icon: "location.fill",  text: church.address,  url: "maps://?q=\(church.address.addingPercentEncoding(withAllowedCharacters: .urlQueryAllowed) ?? "")")
            ContactRow(icon: "phone.fill",     text: church.phone,    url: "tel:\(church.phone.filter { $0.isNumber })")
            ContactRow(icon: "envelope.fill",  text: church.email,    url: "mailto:\(church.email)")

            GoldButton(title: "Give Online", icon: "heart.fill", fullWidth: true) {
                showGive = true
            }
        }
        .padding(24)
        .background(Theme.background)
    }
}

private struct ContactRow: View {
    let icon: String
    let text: String
    let url: String

    var body: some View {
        Link(destination: URL(string: url)!) {
            HStack(spacing: 14) {
                ZStack {
                    Circle()
                        .fill(Theme.gold.opacity(0.12))
                        .frame(width: 40, height: 40)
                    Image(systemName: icon)
                        .font(.system(size: 16))
                        .foregroundStyle(Theme.goldTextGradient)
                }
                Text(text)
                    .font(.system(size: 14))
                    .foregroundColor(Theme.textPrimary)
                Spacer()
                Image(systemName: "arrow.up.right")
                    .font(.system(size: 12))
                    .foregroundColor(Theme.textMuted)
            }
            .padding(14)
            .background {
                RoundedRectangle(cornerRadius: 14, style: .continuous)
                    .fill(Theme.surface1)
                    .overlay { RoundedRectangle(cornerRadius: 14, style: .continuous).stroke(Theme.goldBorder, lineWidth: 1) }
            }
        }
        .buttonStyle(.plain)
    }
}

#Preview {
    NavigationStack {
        AboutView()
    }
    .environmentObject(DataStore())
}
