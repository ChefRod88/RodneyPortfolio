import SwiftUI

struct GoldButton: View {
    let title: String
    var icon: String? = nil
    var fullWidth: Bool = false
    let action: () -> Void

    @State private var isPressed = false

    var body: some View {
        Button(action: action) {
            HStack(spacing: 8) {
                if let icon {
                    Image(systemName: icon)
                        .font(.system(size: 14, weight: .semibold))
                }
                Text(title)
                    .font(.system(size: 14, weight: .bold))
                    .tracking(1.2)
                    .textCase(.uppercase)
            }
            .padding(.horizontal, fullWidth ? 0 : 28)
            .padding(.vertical, 14)
            .frame(maxWidth: fullWidth ? .infinity : nil)
            .background(Theme.goldGradient)
            .foregroundColor(Theme.background)
            .clipShape(Capsule())
            .shadow(color: Theme.goldGlow, radius: isPressed ? 8 : 16, y: isPressed ? 4 : 8)
            .scaleEffect(isPressed ? 0.96 : 1.0)
        }
        .buttonStyle(.plain)
        .onLongPressGesture(minimumDuration: 0, maximumDistance: 50) {} onPressingChanged: { pressing in
            withAnimation(.spring(response: 0.25, dampingFraction: 0.7)) {
                isPressed = pressing
            }
        }
    }
}

struct GhostButton: View {
    let title: String
    var icon: String? = nil
    var fullWidth: Bool = false
    let action: () -> Void

    var body: some View {
        Button(action: action) {
            HStack(spacing: 8) {
                if let icon {
                    Image(systemName: icon)
                        .font(.system(size: 14, weight: .semibold))
                }
                Text(title)
                    .font(.system(size: 14, weight: .semibold))
                    .tracking(1.0)
                    .textCase(.uppercase)
            }
            .padding(.horizontal, fullWidth ? 0 : 28)
            .padding(.vertical, 13)
            .frame(maxWidth: fullWidth ? .infinity : nil)
            .foregroundColor(Theme.goldLight)
            .background {
                Capsule()
                    .stroke(Theme.gold.opacity(0.45), lineWidth: 1)
            }
        }
        .buttonStyle(.plain)
    }
}

struct EyebrowBadge: View {
    let title: String
    var showDot: Bool = true

    var body: some View {
        HStack(spacing: 8) {
            if showDot {
                AnimatedDot()
            }
            Text(title)
                .font(.system(size: 11, weight: .semibold))
                .tracking(3.5)
                .textCase(.uppercase)
                .foregroundColor(Theme.gold)
        }
        .padding(.horizontal, 20)
        .padding(.vertical, 5)
        .background(.ultraThinMaterial)
        .overlay {
            Capsule()
                .stroke(Theme.goldBorder, lineWidth: 1)
        }
        .clipShape(Capsule())
    }
}

struct AnimatedDot: View {
    @State private var on = true

    var body: some View {
        Circle()
            .fill(Theme.gold)
            .frame(width: 6, height: 6)
            .shadow(color: Theme.gold, radius: 4)
            .opacity(on ? 1 : 0.2)
            .animation(.easeInOut(duration: 1).repeatForever(), value: on)
            .onAppear { on = false }
    }
}

#Preview {
    VStack(spacing: 16) {
        GoldButton(title: "Plan A Visit", icon: "arrow.right") {}
        GhostButton(title: "Watch Online", icon: "play.fill") {}
        EyebrowBadge(title: "Winter Haven, Florida")
    }
    .padding()
    .background(Theme.background)
}
