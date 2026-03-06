import SwiftUI

struct MoreView: View {
    @EnvironmentObject var store: DataStore
    @Binding var showGive: Bool

    private let menuItems: [(icon: String, label: String, sub: String, dest: AnyView)] = []

    var body: some View {
        NavigationStack {
            ZStack {
                Theme.background.ignoresSafeArea()
                ConstellationView(particleCount: 35)
                    .ignoresSafeArea()
                    .opacity(0.4)

                ScrollView(showsIndicators: false) {
                    VStack(spacing: 24) {
                        // Header
                        MoreHeader(church: store.church)

                        // Menu Groups
                        MenuGroup(title: "Media", items: [
                            MenuItem(icon: "play.rectangle.fill", color: Color(hex: "E53935"), label: "Watch Live", subtitle: "Join our live stream", destination: AnyView(LiveView())),
                            MenuItem(icon: "books.vertical.fill",    color: Theme.gold,            label: "Sermons",     subtitle: "Browse the library",   destination: AnyView(SermonsView())),
                        ])

                        MenuGroup(title: "Connect", items: [
                            MenuItem(icon: "person.3.fill",          color: Color(hex: "3D8EFF"), label: "Groups",       subtitle: "Find your community", destination: AnyView(GroupsView())),
                            MenuItem(icon: "calendar.badge.plus",    color: Color(hex: "34C759"), label: "Events",       subtitle: "What's happening",    destination: AnyView(EventsView())),
                            MenuItem(icon: "hands.sparkles.fill",    color: Color(hex: "AF52DE"), label: "Prayer",       subtitle: "Request prayer",      destination: AnyView(PrayerView())),
                        ])

                        MenuGroup(title: "About", items: [
                            MenuItem(icon: "building.columns.fill",  color: Theme.gold,            label: "Who We Are",   subtitle: "Our story & beliefs", destination: AnyView(AboutView())),
                            MenuItem(icon: "location.fill",          color: Color(hex: "FF6B35"),  label: "Location",     subtitle: "Find us in Winter Haven", destination: AnyView(LocationView())),
                        ])

                        // Give Button (special)
                        GiveMenuCard(showGive: $showGive)
                            .padding(.horizontal, 16)

                        // App Info
                        AppInfoFooter()
                            .padding(.horizontal, 16)

                        Spacer(minLength: 100)
                    }
                }
            }
            .navigationTitle("More")
            .navigationBarTitleDisplayMode(.inline)
            .toolbarColorScheme(.dark, for: .navigationBar)
            .toolbarBackground(Theme.surface1, for: .navigationBar)
        }
    }
}

private struct MoreHeader: View {
    let church: ChurchInfo

    var body: some View {
        VStack(spacing: 14) {
            ZStack {
                Circle()
                    .fill(Theme.goldGradient)
                    .frame(width: 80, height: 80)
                    .shadow(color: Theme.goldGlow, radius: 20)
                Image(systemName: "building.columns.fill")
                    .font(.system(size: 34))
                    .foregroundColor(Theme.background)
            }

            VStack(spacing: 4) {
                Text(church.shortName)
                    .font(.system(size: 22, weight: .black, design: .rounded))
                    .foregroundStyle(Theme.goldTextGradient)
                Text(church.location)
                    .font(.system(size: 13))
                    .tracking(1.5)
                    .textCase(.uppercase)
                    .foregroundColor(Theme.textMuted)
            }
        }
        .padding(.top, 20)
    }
}

struct MenuItem: Identifiable {
    let id = UUID()
    let icon: String
    let color: Color
    let label: String
    let subtitle: String
    let destination: AnyView
}

struct MenuGroup: View {
    let title: String
    let items: [MenuItem]

    var body: some View {
        VStack(alignment: .leading, spacing: 10) {
            Text(title.uppercased())
                .font(.system(size: 11, weight: .semibold))
                .tracking(2.5)
                .foregroundColor(Theme.textMuted)
                .padding(.horizontal, 20)

            VStack(spacing: 0) {
                ForEach(Array(items.enumerated()), id: \.element.id) { index, item in
                    NavigationLink(destination: item.destination) {
                        MenuRow(item: item, isLast: index == items.count - 1)
                    }
                    .buttonStyle(.plain)
                }
            }
            .background {
                RoundedRectangle(cornerRadius: 18, style: .continuous)
                    .fill(Theme.surface1)
                    .overlay {
                        RoundedRectangle(cornerRadius: 18, style: .continuous)
                            .stroke(Theme.goldBorder, lineWidth: 1)
                    }
            }
            .clipShape(RoundedRectangle(cornerRadius: 18, style: .continuous))
            .padding(.horizontal, 16)
        }
    }
}

struct MenuRow: View {
    let item: MenuItem
    let isLast: Bool

    var body: some View {
        VStack(spacing: 0) {
            HStack(spacing: 14) {
                ZStack {
                    RoundedRectangle(cornerRadius: 10, style: .continuous)
                        .fill(item.color.opacity(0.18))
                        .frame(width: 42, height: 42)
                    Image(systemName: item.icon)
                        .font(.system(size: 18))
                        .foregroundColor(item.color)
                }
                VStack(alignment: .leading, spacing: 2) {
                    Text(item.label)
                        .font(.system(size: 16, weight: .semibold))
                        .foregroundColor(.white)
                    Text(item.subtitle)
                        .font(.system(size: 12))
                        .foregroundColor(Theme.textMuted)
                }
                Spacer()
                Image(systemName: "chevron.right")
                    .font(.system(size: 12, weight: .semibold))
                    .foregroundColor(Theme.textMuted)
            }
            .padding(.horizontal, 16)
            .padding(.vertical, 14)

            if !isLast {
                Divider()
                    .background(Theme.goldBorder)
                    .padding(.leading, 74)
            }
        }
    }
}

private struct GiveMenuCard: View {
    @Binding var showGive: Bool

    var body: some View {
        Button {
            showGive = true
        } label: {
            HStack(spacing: 16) {
                ZStack {
                    RoundedRectangle(cornerRadius: 14, style: .continuous)
                        .fill(
                            LinearGradient(
                                colors: [Color(hex: "00D632"), Color(hex: "009922")],
                                startPoint: .topLeading,
                                endPoint: .bottomTrailing
                            )
                        )
                        .frame(width: 52, height: 52)
                        .shadow(color: Color(hex: "00D632").opacity(0.4), radius: 12)
                    Text("$")
                        .font(.system(size: 24, weight: .black))
                        .foregroundColor(.white)
                }

                VStack(alignment: .leading, spacing: 3) {
                    Text("Give Online")
                        .font(.system(size: 17, weight: .bold))
                        .foregroundColor(.white)
                    Text("Support the ministry via Cash App")
                        .font(.system(size: 13))
                        .foregroundColor(Theme.textMuted)
                }
                Spacer()
                Image(systemName: "heart.fill")
                    .font(.system(size: 16))
                    .foregroundColor(Color(hex: "00D632"))
            }
            .padding(16)
            .background {
                RoundedRectangle(cornerRadius: 18, style: .continuous)
                    .fill(Theme.surface1)
                    .overlay {
                        RoundedRectangle(cornerRadius: 18, style: .continuous)
                            .stroke(Color(hex: "00D632").opacity(0.3), lineWidth: 1)
                    }
            }
        }
        .buttonStyle(.plain)
    }
}

private struct AppInfoFooter: View {
    var body: some View {
        VStack(spacing: 8) {
            Text("RC DEV")
                .font(.system(size: 13, weight: .bold))
                .tracking(3)
                .foregroundColor(Theme.gold.opacity(0.5))
            Text("Version 1.0.0  •  New Bethel MBBC")
                .font(.system(size: 11))
                .foregroundColor(Theme.textMuted.opacity(0.5))
            Text("Winter Haven, Florida")
                .font(.system(size: 11))
                .foregroundColor(Theme.textMuted.opacity(0.4))
        }
        .frame(maxWidth: .infinity)
        .padding(.vertical, 20)
    }
}

#Preview {
    MoreView(showGive: .constant(false))
        .environmentObject(DataStore())
}
