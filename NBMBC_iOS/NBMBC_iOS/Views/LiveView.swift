import SwiftUI
import WebKit

struct LiveView: View {
    @EnvironmentObject var store: DataStore
    @State private var isLive = false

    var body: some View {
        ZStack {
            Theme.background.ignoresSafeArea()
            ConstellationView(particleCount: 30)
                .ignoresSafeArea()
                .opacity(0.3)

            ScrollView(showsIndicators: false) {
                VStack(spacing: 0) {
                    // Live Stream Player
                    LivePlayerSection(facebookURL: store.church.facebookURL)

                    VStack(spacing: 20) {
                        // Status
                        LiveStatusBar()

                        // Info Cards
                        ServiceTimesCard()
                        SocialLinksCard(church: store.church)
                    }
                    .padding(20)
                    .padding(.bottom, 100)
                }
            }
        }
        .navigationTitle("Watch Live")
        .navigationBarTitleDisplayMode(.inline)
        .toolbarColorScheme(.dark, for: .navigationBar)
        .toolbarBackground(Theme.surface1, for: .navigationBar)
    }
}

private struct LivePlayerSection: View {
    let facebookURL: String

    var body: some View {
        ZStack {
            Color.black
            VStack(spacing: 20) {
                ZStack {
                    Circle()
                        .fill(Color(hex: "E53935").opacity(0.15))
                        .frame(width: 100, height: 100)
                    Circle()
                        .fill(Color(hex: "E53935").opacity(0.08))
                        .frame(width: 140, height: 140)
                    Image(systemName: "play.rectangle.fill")
                        .font(.system(size: 50))
                        .foregroundColor(Color(hex: "E53935").opacity(0.9))
                }
                Text("Live Stream")
                    .font(.system(size: 20, weight: .bold))
                    .foregroundColor(.white)
                Text("Tap to open our Facebook Live stream")
                    .font(.system(size: 14))
                    .foregroundColor(.gray)

                Link(destination: URL(string: facebookURL)!) {
                    HStack(spacing: 8) {
                        Image(systemName: "play.fill")
                        Text("Open Facebook Live")
                    }
                    .font(.system(size: 14, weight: .bold))
                    .foregroundColor(.white)
                    .padding(.horizontal, 24)
                    .padding(.vertical, 12)
                    .background(Color(hex: "1877F2"))
                    .clipShape(Capsule())
                }
                .buttonStyle(.plain)
            }
        }
        .frame(height: 280)
        .overlay(alignment: .top) {
            LinearGradient(colors: [Theme.surface2, .clear], startPoint: .top, endPoint: .bottom)
                .frame(height: 3)
        }
    }
}

private struct LiveStatusBar: View {
    var body: some View {
        HStack(spacing: 12) {
            HStack(spacing: 6) {
                LivePulsingDot()
                Text("LIVE SUNDAYS 10:30 AM EST")
                    .font(.system(size: 12, weight: .bold))
                    .tracking(1.5)
                    .foregroundColor(.white)
            }
            Spacer()
            Text("New Bethel MBBC")
                .font(.system(size: 12))
                .foregroundColor(Theme.textMuted)
        }
        .padding(14)
        .glassCard(padding: 0, cornerRadius: 14)
        .padding(0)
        .overlay {
            RoundedRectangle(cornerRadius: 14, style: .continuous)
                .fill(.clear)
                .overlay {
                    RoundedRectangle(cornerRadius: 14, style: .continuous)
                        .stroke(Color(hex: "E53935").opacity(0.35), lineWidth: 1)
                }
        }
        .padding(.horizontal, 0)
    }
}

struct LivePulsingDot: View {
    @State private var animate = false

    var body: some View {
        ZStack {
            Circle()
                .fill(Color(hex: "E53935").opacity(0.3))
                .frame(width: 18, height: 18)
                .scaleEffect(animate ? 1.5 : 1.0)
                .opacity(animate ? 0 : 1)
            Circle()
                .fill(Color(hex: "E53935"))
                .frame(width: 8, height: 8)
        }
        .onAppear {
            withAnimation(.easeOut(duration: 1.2).repeatForever(autoreverses: false)) {
                animate = true
            }
        }
    }
}

private struct ServiceTimesCard: View {
    private let services = [
        ("Sunday School",    "9:30 AM"),
        ("Morning Worship",  "10:30 AM"),
        ("Bible Study",      "Wednesday 7 PM"),
    ]

    var body: some View {
        VStack(alignment: .leading, spacing: 14) {
            Text("SERVICE TIMES")
                .font(.system(size: 11, weight: .semibold))
                .tracking(2.5)
                .foregroundColor(Theme.gold)
            ForEach(services, id: \.0) { service in
                HStack {
                    HStack(spacing: 8) {
                        Circle()
                            .fill(Theme.gold)
                            .frame(width: 6, height: 6)
                        Text(service.0)
                            .font(.system(size: 15, weight: .medium))
                            .foregroundColor(.white)
                    }
                    Spacer()
                    Text(service.1)
                        .font(.system(size: 14, weight: .bold))
                        .foregroundColor(Theme.goldLight)
                }
                if service.0 != services.last!.0 {
                    Divider().background(Theme.goldBorder)
                }
            }
        }
        .glassCard(padding: 18, cornerRadius: 18)
    }
}

private struct SocialLinksCard: View {
    let church: ChurchInfo

    var body: some View {
        VStack(alignment: .leading, spacing: 14) {
            Text("CONNECT WITH US")
                .font(.system(size: 11, weight: .semibold))
                .tracking(2.5)
                .foregroundColor(Theme.gold)

            SocialLink(icon: "play.rectangle.fill", color: Color(hex: "FF0000"), label: "YouTube", url: church.youtubeURL)
            Divider().background(Theme.goldBorder)
            SocialLink(icon: "f.square.fill", color: Color(hex: "1877F2"), label: "Facebook", url: church.facebookURL)
        }
        .glassCard(padding: 18, cornerRadius: 18)
    }
}

private struct SocialLink: View {
    let icon: String
    let color: Color
    let label: String
    let url: String

    var body: some View {
        Link(destination: URL(string: url)!) {
            HStack(spacing: 12) {
                Image(systemName: icon)
                    .font(.system(size: 22))
                    .foregroundColor(color)
                    .frame(width: 32)
                Text("Watch on \(label)")
                    .font(.system(size: 15, weight: .medium))
                    .foregroundColor(.white)
                Spacer()
                Image(systemName: "arrow.up.right")
                    .font(.system(size: 12))
                    .foregroundColor(Theme.textMuted)
            }
        }
        .buttonStyle(.plain)
    }
}

#Preview {
    NavigationStack {
        LiveView()
    }
    .environmentObject(DataStore())
}
