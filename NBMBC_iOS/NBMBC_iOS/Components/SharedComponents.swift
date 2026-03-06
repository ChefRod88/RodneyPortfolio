import SwiftUI

// MARK: - Empty State View
struct EmptyStateView: View {
    let icon: String
    let title: String
    let subtitle: String

    var body: some View {
        VStack(spacing: 16) {
            Image(systemName: icon)
                .font(.system(size: 52))
                .foregroundColor(Theme.gold.opacity(0.4))
            Text(title)
                .font(.system(size: 20, weight: .bold))
                .foregroundColor(Theme.textPrimary.opacity(0.5))
            Text(subtitle)
                .font(.system(size: 14))
                .foregroundColor(Theme.textMuted)
                .multilineTextAlignment(.center)
        }
        .padding(40)
        .frame(maxWidth: .infinity)
    }
}

// MARK: - Grid Pattern Background
struct GridPattern: View {
    var opacity: Double = 0.04
    var spacing: CGFloat = 60

    var body: some View {
        GeometryReader { geo in
            Path { p in
                var x: CGFloat = 0
                while x <= geo.size.width {
                    p.move(to: CGPoint(x: x, y: 0))
                    p.addLine(to: CGPoint(x: x, y: geo.size.height))
                    x += spacing
                }
                var y: CGFloat = 0
                while y <= geo.size.height {
                    p.move(to: CGPoint(x: 0, y: y))
                    p.addLine(to: CGPoint(x: geo.size.width, y: y))
                    y += spacing
                }
            }
            .stroke(Theme.gold.opacity(opacity), lineWidth: 1)
            .mask {
                RadialGradient(
                    colors: [.black, .clear],
                    center: .center,
                    startRadius: 0,
                    endRadius: max(geo.size.width, geo.size.height) * 0.65
                )
            }
        }
        .allowsHitTesting(false)
    }
}

// MARK: - Page Header
struct PageHeader: View {
    let eyebrow: String
    let title: String
    let subtitle: String?

    init(eyebrow: String, title: String, subtitle: String? = nil) {
        self.eyebrow = eyebrow
        self.title = title
        self.subtitle = subtitle
    }

    var body: some View {
        ZStack {
            LinearGradient(colors: [Theme.surface2, Theme.background], startPoint: .top, endPoint: .bottom)
            GridPattern()
            VStack(spacing: 16) {
                EyebrowBadge(title: eyebrow)
                Text(title)
                    .font(.system(size: 36, weight: .black, design: .rounded))
                    .foregroundStyle(
                        LinearGradient(colors: [.white, Theme.goldLight, Theme.gold], startPoint: .topLeading, endPoint: .bottomTrailing)
                    )
                if let sub = subtitle {
                    Text(sub)
                        .font(.system(size: 15))
                        .foregroundColor(Theme.textMuted)
                        .multilineTextAlignment(.center)
                        .lineSpacing(4)
                }
            }
            .padding(36)
        }
    }
}

// MARK: - Gold Divider
struct GoldDivider: View {
    var body: some View {
        Rectangle()
            .fill(
                LinearGradient(colors: [.clear, Theme.gold, .clear], startPoint: .leading, endPoint: .trailing)
            )
            .frame(height: 1)
    }
}

// MARK: - Scroll Reveal Modifier
struct ScrollRevealModifier: ViewModifier {
    @State private var visible = false

    func body(content: Content) -> some View {
        content
            .opacity(visible ? 1 : 0)
            .offset(y: visible ? 0 : 24)
            .animation(.spring(response: 0.6, dampingFraction: 0.75), value: visible)
            .onAppear { visible = true }
    }
}

extension View {
    func scrollReveal() -> some View {
        modifier(ScrollRevealModifier())
    }
}
