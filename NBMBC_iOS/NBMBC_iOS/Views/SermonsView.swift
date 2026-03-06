import SwiftUI

struct SermonsView: View {
    @EnvironmentObject var store: DataStore

    private let columns = [GridItem(.flexible()), GridItem(.flexible())]

    var body: some View {
        ZStack {
            Theme.background.ignoresSafeArea()
            ConstellationView(particleCount: 40)
                .ignoresSafeArea()
                .opacity(0.4)

            ScrollView(showsIndicators: false) {
                LazyVStack(spacing: 0) {
                    // Hero
                    SermonsHero()

                    // Grid
                    LazyVGrid(columns: columns, spacing: 16) {
                        ForEach(store.sermons) { sermon in
                            NavigationLink(destination: SermonDetailView(sermon: sermon)) {
                                SermonGridCard(sermon: sermon)
                            }
                            .buttonStyle(.plain)
                        }
                        if store.sermons.isEmpty {
                            EmptyStateView(icon: "play.rectangle", title: "No Sermons Yet", subtitle: "Check back soon — messages are on the way.")
                                .gridCellColumns(2)
                        }
                    }
                    .padding(.horizontal, 16)
                    .padding(.top, 24)
                    .padding(.bottom, 120)
                }
            }
        }
        .navigationTitle("Sermons")
        .navigationBarTitleDisplayMode(.inline)
        .toolbarColorScheme(.dark, for: .navigationBar)
        .toolbarBackground(Theme.surface1, for: .navigationBar)
    }
}

private struct SermonsHero: View {
    @State private var navigateToLive = false

    var body: some View {
        ZStack {
            LinearGradient(
                colors: [Theme.surface2, Theme.background],
                startPoint: .top,
                endPoint: .bottom
            )
            .overlay {
                // Grid pattern
                GeometryReader { geo in
                    let gridSpacing: CGFloat = 60
                    Path { p in
                        var x: CGFloat = 0
                        while x <= geo.size.width {
                            p.move(to: CGPoint(x: x, y: 0))
                            p.addLine(to: CGPoint(x: x, y: geo.size.height))
                            x += gridSpacing
                        }
                        var y: CGFloat = 0
                        while y <= geo.size.height {
                            p.move(to: CGPoint(x: 0, y: y))
                            p.addLine(to: CGPoint(x: geo.size.width, y: y))
                            y += gridSpacing
                        }
                    }
                    .stroke(Theme.gold.opacity(0.04), lineWidth: 1)
                    .mask {
                        RadialGradient(
                            colors: [.black, .clear],
                            center: .center,
                            startRadius: 0,
                            endRadius: max(geo.size.width, geo.size.height) * 0.6
                        )
                    }
                }
            }

            VStack(spacing: 20) {
                EyebrowBadge(title: "Word of God")

                Text("Sermon Library")
                    .font(.system(size: 38, weight: .black, design: .rounded))
                    .foregroundStyle(
                        LinearGradient(
                            colors: [.white, Theme.goldLight, Theme.gold],
                            startPoint: .topLeading,
                            endPoint: .bottomTrailing
                        )
                    )

                Text("Watch our latest teachings and grow in the Word.\nEvery message is a fresh encounter with God.")
                    .font(.system(size: 15))
                    .foregroundColor(Theme.textMuted)
                    .multilineTextAlignment(.center)
                    .lineSpacing(4)

                NavigationLink(destination: LiveView()) {
                    HStack(spacing: 8) {
                        ZStack {
                            Circle()
                                .fill(Color.white.opacity(0.9))
                                .frame(width: 8, height: 8)
                            Circle()
                                .stroke(Color.white.opacity(0.5), lineWidth: 2)
                                .frame(width: 14, height: 14)
                                .scaleEffect(1.0)
                                .animation(.easeOut(duration: 1.2).repeatForever(), value: true)
                        }
                        Text("WATCH LIVE STREAM")
                            .font(.system(size: 13, weight: .bold))
                            .tracking(1.5)
                    }
                    .padding(.horizontal, 24)
                    .padding(.vertical, 12)
                    .background(
                        LinearGradient(
                            colors: [Color(hex: "E53935"), Color(hex: "8A0000")],
                            startPoint: .leading,
                            endPoint: .trailing
                        )
                    )
                    .foregroundColor(.white)
                    .clipShape(Capsule())
                    .shadow(color: Color(hex: "E53935").opacity(0.4), radius: 12, y: 6)
                }
                .buttonStyle(.plain)
            }
            .padding(36)
        }
    }
}

struct SermonGridCard: View {
    let sermon: Sermon

    var body: some View {
        VStack(alignment: .leading, spacing: 0) {
            // Thumbnail
            ZStack {
                LinearGradient(
                    colors: [Theme.surface2, Theme.surface3],
                    startPoint: .topLeading,
                    endPoint: .bottomTrailing
                )
                VStack(spacing: 8) {
                    Image(systemName: "play.circle.fill")
                        .font(.system(size: 40))
                        .foregroundColor(Theme.gold.opacity(0.8))
                    if let series = sermon.series {
                        Text(series.uppercased())
                            .font(.system(size: 8, weight: .semibold))
                            .tracking(2)
                            .foregroundColor(Theme.gold.opacity(0.6))
                    }
                }
            }
            .frame(height: 110)
            .overlay(alignment: .top) {
                LinearGradient(colors: [.clear, Theme.surface1.opacity(0.6)], startPoint: .top, endPoint: .bottom)
            }

            // Body
            VStack(alignment: .leading, spacing: 6) {
                if let series = sermon.series {
                    Text(series.uppercased())
                        .font(.system(size: 9, weight: .semibold))
                        .tracking(2)
                        .foregroundColor(Theme.gold)
                }
                Text(sermon.title)
                    .font(.system(size: 13, weight: .semibold))
                    .foregroundColor(.white)
                    .lineLimit(2)
                    .fixedSize(horizontal: false, vertical: true)
                Text(sermon.formattedDate)
                    .font(.system(size: 11))
                    .foregroundColor(Theme.textMuted)

                HStack(spacing: 4) {
                    Image(systemName: "play.fill")
                        .font(.system(size: 9))
                    Text("Watch Now")
                        .font(.system(size: 11, weight: .semibold))
                }
                .foregroundColor(Theme.goldLight)
                .padding(.top, 4)
            }
            .padding(12)
        }
        .background {
            RoundedRectangle(cornerRadius: 16, style: .continuous)
                .fill(Theme.surface1)
                .overlay {
                    RoundedRectangle(cornerRadius: 16, style: .continuous)
                        .stroke(Theme.goldBorder, lineWidth: 1)
                }
                .overlay(alignment: .top) {
                    RoundedRectangle(cornerRadius: 16, style: .continuous)
                        .fill(
                            LinearGradient(
                                colors: [Theme.gold.opacity(0.15), .clear],
                                startPoint: .leading,
                                endPoint: .trailing
                            )
                        )
                        .frame(height: 2)
                        .clipShape(RoundedRectangle(cornerRadius: 16, style: .continuous))
                }
        }
        .clipShape(RoundedRectangle(cornerRadius: 16, style: .continuous))
    }
}

#Preview {
    NavigationStack {
        SermonsView()
    }
    .environmentObject(DataStore())
}
