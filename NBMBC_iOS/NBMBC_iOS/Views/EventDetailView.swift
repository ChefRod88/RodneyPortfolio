import SwiftUI

struct EventDetailView: View {
    let event: Event

    var body: some View {
        ZStack {
            Theme.background.ignoresSafeArea()
            ConstellationView(particleCount: 30)
                .ignoresSafeArea()
                .opacity(0.35)

            ScrollView(showsIndicators: false) {
                VStack(spacing: 0) {
                    // Hero Header
                    EventDetailHero(event: event)

                    // Details Body
                    VStack(alignment: .leading, spacing: 24) {
                        // Quick info chips
                        ScrollView(.horizontal, showsIndicators: false) {
                            HStack(spacing: 10) {
                                InfoChip(icon: "calendar", text: event.formattedDate)
                                InfoChip(icon: "clock", text: event.formattedTime)
                                if let loc = event.location {
                                    InfoChip(icon: "mappin.and.ellipse", text: loc)
                                }
                                if let cap = event.capacity {
                                    InfoChip(icon: "person.2.fill", text: "\(cap) spots")
                                }
                            }
                        }

                        Rectangle()
                            .fill(LinearGradient(colors: [.clear, Theme.gold, .clear], startPoint: .leading, endPoint: .trailing))
                            .frame(height: 1)

                        if let desc = event.description {
                            VStack(alignment: .leading, spacing: 8) {
                                Text("About This Event")
                                    .font(.system(size: 13, weight: .semibold))
                                    .tracking(2)
                                    .textCase(.uppercase)
                                    .foregroundColor(Theme.gold)
                                Text(desc)
                                    .font(.system(size: 16))
                                    .foregroundColor(Theme.textPrimary.opacity(0.85))
                                    .lineSpacing(6)
                            }
                        }

                        // Add to Calendar CTA
                        VStack(spacing: 12) {
                            GoldButton(title: "Add to Calendar", icon: "calendar.badge.plus", fullWidth: true) {
                                // Calendar integration
                            }
                            if event.location != nil {
                                GhostButton(title: "Get Directions", icon: "map.fill", fullWidth: true) {
                                    // Maps
                                }
                            }
                        }
                    }
                    .padding(24)
                    .padding(.bottom, 100)
                }
            }
        }
        .navigationTitle(event.title)
        .navigationBarTitleDisplayMode(.inline)
        .toolbarColorScheme(.dark, for: .navigationBar)
        .toolbarBackground(Theme.surface1, for: .navigationBar)
    }
}

private struct EventDetailHero: View {
    let event: Event

    var body: some View {
        ZStack {
            LinearGradient(
                colors: [Theme.surface2, Theme.surface3, Theme.background],
                startPoint: .top,
                endPoint: .bottom
            )

            VStack(spacing: 20) {
                // Large icon
                ZStack {
                    Circle()
                        .fill(Theme.goldGradient)
                        .frame(width: 88, height: 88)
                        .shadow(color: Theme.goldGlow, radius: 24)
                    Image(systemName: event.icon)
                        .font(.system(size: 38))
                        .foregroundColor(Theme.background)
                }

                Text(event.title)
                    .font(.system(size: 28, weight: .bold, design: .rounded))
                    .foregroundStyle(
                        LinearGradient(colors: [.white, Theme.goldLight], startPoint: .top, endPoint: .bottom)
                    )
                    .multilineTextAlignment(.center)

                Text("N.B.M.B.C.  •  Winter Haven, FL")
                    .font(.system(size: 12, weight: .semibold))
                    .tracking(2)
                    .textCase(.uppercase)
                    .foregroundColor(Theme.textMuted)
            }
            .padding(36)
        }
    }
}

private struct InfoChip: View {
    let icon: String
    let text: String

    var body: some View {
        HStack(spacing: 6) {
            Image(systemName: icon)
                .font(.system(size: 12))
            Text(text)
                .font(.system(size: 13, weight: .medium))
        }
        .foregroundColor(Theme.goldLight)
        .padding(.horizontal, 14)
        .padding(.vertical, 8)
        .background {
            Capsule()
                .fill(Theme.gold.opacity(0.1))
                .overlay { Capsule().stroke(Theme.gold.opacity(0.3), lineWidth: 1) }
        }
    }
}

#Preview {
    NavigationStack {
        EventDetailView(event: DataStore().events[0])
    }
    .environmentObject(DataStore())
}
