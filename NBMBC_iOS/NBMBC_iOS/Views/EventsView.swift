import SwiftUI

struct EventsView: View {
    @EnvironmentObject var store: DataStore

    var body: some View {
        ZStack {
            Theme.background.ignoresSafeArea()
            ConstellationView(particleCount: 40)
                .ignoresSafeArea()
                .opacity(0.4)

            ScrollView(showsIndicators: false) {
                LazyVStack(spacing: 0) {
                    EventsHero()

                    VStack(spacing: 16) {
                        if store.events.isEmpty {
                            EmptyStateView(icon: "calendar.badge.exclamationmark", title: "No Events Scheduled", subtitle: "Check back soon for upcoming events and gatherings.")
                                .padding(.top, 40)
                        } else {
                            ForEach(store.events) { event in
                                NavigationLink(destination: EventDetailView(event: event)) {
                                    EventCard(event: event)
                                }
                                .buttonStyle(.plain)
                            }
                        }
                    }
                    .padding(.horizontal, 16)
                    .padding(.top, 24)
                    .padding(.bottom, 120)
                }
            }
        }
        .navigationTitle("Events")
        .navigationBarTitleDisplayMode(.inline)
        .toolbarColorScheme(.dark, for: .navigationBar)
        .toolbarBackground(Theme.surface1, for: .navigationBar)
    }
}

private struct EventsHero: View {
    var body: some View {
        ZStack {
            LinearGradient(colors: [Theme.surface2, Theme.background], startPoint: .top, endPoint: .bottom)
            GridPattern()
            VStack(spacing: 16) {
                EyebrowBadge(title: "N.B.M.B.C.")
                Text("Upcoming Events")
                    .font(.system(size: 38, weight: .black, design: .rounded))
                    .foregroundStyle(
                        LinearGradient(colors: [.white, Theme.goldLight, Theme.gold], startPoint: .topLeading, endPoint: .bottomTrailing)
                    )
                Text("Find events to attend and get connected with our community in Winter Haven, Florida.")
                    .font(.system(size: 15))
                    .foregroundColor(Theme.textMuted)
                    .multilineTextAlignment(.center)
                    .lineSpacing(4)
            }
            .padding(36)
        }
    }
}

struct EventCard: View {
    let event: Event

    var body: some View {
        HStack(spacing: 14) {
            // Date Badge
            VStack(spacing: 2) {
                Text(event.monthAbbr)
                    .font(.system(size: 11, weight: .bold))
                    .tracking(1.5)
                    .foregroundColor(Theme.gold)
                Text(event.dayNumber)
                    .font(.system(size: 28, weight: .black, design: .rounded))
                    .foregroundColor(.white)
            }
            .frame(width: 58)
            .padding(.vertical, 12)
            .background {
                RoundedRectangle(cornerRadius: 12, style: .continuous)
                    .fill(Theme.surface2)
                    .overlay {
                        RoundedRectangle(cornerRadius: 12, style: .continuous)
                            .stroke(Theme.goldBorder, lineWidth: 1)
                    }
            }

            // Icon
            ZStack {
                Circle()
                    .fill(Theme.gold.opacity(0.12))
                    .frame(width: 44, height: 44)
                Image(systemName: event.icon)
                    .font(.system(size: 18))
                    .foregroundStyle(Theme.goldTextGradient)
            }

            // Info
            VStack(alignment: .leading, spacing: 4) {
                Text(event.title)
                    .font(.system(size: 16, weight: .bold))
                    .foregroundColor(.white)
                    .lineLimit(2)
                HStack(spacing: 8) {
                    Label(event.formattedTime, systemImage: "clock")
                    if let loc = event.location {
                        Label(loc, systemImage: "mappin")
                    }
                }
                .font(.system(size: 12))
                .foregroundColor(Theme.textMuted)
                if let cap = event.capacity {
                    Label("\(cap) spots available", systemImage: "person.2")
                        .font(.system(size: 11))
                        .foregroundColor(Theme.gold.opacity(0.7))
                }
            }

            Spacer()

            Image(systemName: "chevron.right")
                .font(.system(size: 12, weight: .semibold))
                .foregroundColor(Theme.textMuted)
        }
        .padding(14)
        .background {
            RoundedRectangle(cornerRadius: 18, style: .continuous)
                .fill(Theme.surface1)
                .overlay {
                    RoundedRectangle(cornerRadius: 18, style: .continuous)
                        .stroke(Theme.goldBorder, lineWidth: 1)
                }
                .overlay(alignment: .top) {
                    RoundedRectangle(cornerRadius: 18, style: .continuous)
                        .fill(LinearGradient(colors: [Theme.gold.opacity(0.12), .clear], startPoint: .leading, endPoint: .trailing))
                        .frame(height: 2)
                        .clipShape(RoundedRectangle(cornerRadius: 18, style: .continuous))
                }
        }
    }
}

#Preview {
    NavigationStack {
        EventsView()
    }
    .environmentObject(DataStore())
}
