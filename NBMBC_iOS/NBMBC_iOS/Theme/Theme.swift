import SwiftUI

enum Theme {
    // MARK: - Colors
    static let gold        = Color(hex: "C9A84C")
    static let goldLight   = Color(hex: "F0D080")
    static let background  = Color(hex: "070710")
    static let surface1    = Color(hex: "0E0E1C")
    static let surface2    = Color(hex: "14142A")
    static let surface3    = Color(hex: "1C1C38")
    static let textPrimary = Color(hex: "F0EDE6")
    static let textMuted   = Color(hex: "9A9485")

    // MARK: - Derived
    static var goldBorder: Color  { gold.opacity(0.18) }
    static var goldGlow: Color    { gold.opacity(0.35) }
    static var glass: Color       { Color.white.opacity(0.04) }

    // MARK: - Gradients
    static var goldGradient: LinearGradient {
        LinearGradient(
            colors: [gold, Color(hex: "7A5500")],
            startPoint: .topLeading,
            endPoint: .bottomTrailing
        )
    }
    static var goldTextGradient: LinearGradient {
        LinearGradient(
            colors: [.white, goldLight, gold],
            startPoint: .topLeading,
            endPoint: .bottomTrailing
        )
    }
    static var heroGradient: LinearGradient {
        LinearGradient(
            colors: [background.opacity(0), background.opacity(0.9)],
            startPoint: .top,
            endPoint: .bottom
        )
    }
    static var surfaceGradient: LinearGradient {
        LinearGradient(
            colors: [surface2, background, surface1],
            startPoint: .topLeading,
            endPoint: .bottomTrailing
        )
    }

    // MARK: - Typography
    static func largeTitle(_ text: String) -> some View {
        Text(text)
            .font(.system(size: 38, weight: .black, design: .rounded))
            .foregroundStyle(goldTextGradient)
    }

    static func sectionTitle(_ text: String) -> some View {
        Text(text)
            .font(.system(size: 26, weight: .bold, design: .rounded))
            .foregroundColor(textPrimary)
    }
}

// MARK: - Color Hex Extension
extension Color {
    init(hex: String) {
        let hex = hex.trimmingCharacters(in: CharacterSet.alphanumerics.inverted)
        var int: UInt64 = 0
        Scanner(string: hex).scanHexInt64(&int)
        let a, r, g, b: UInt64
        switch hex.count {
        case 3:
            (a, r, g, b) = (255, (int >> 8) * 17, (int >> 4 & 0xF) * 17, (int & 0xF) * 17)
        case 6:
            (a, r, g, b) = (255, int >> 16, int >> 8 & 0xFF, int & 0xFF)
        case 8:
            (a, r, g, b) = (int >> 24, int >> 16 & 0xFF, int >> 8 & 0xFF, int & 0xFF)
        default:
            (a, r, g, b) = (255, 0, 0, 0)
        }
        self.init(
            .sRGB,
            red: Double(r) / 255,
            green: Double(g) / 255,
            blue: Double(b) / 255,
            opacity: Double(a) / 255
        )
    }
}

// MARK: - View Modifiers
struct GlassCardModifier: ViewModifier {
    var padding: CGFloat = 20
    var cornerRadius: CGFloat = 20
    var borderOpacity: Double = 1.0

    func body(content: Content) -> some View {
        content
            .padding(padding)
            .background {
                RoundedRectangle(cornerRadius: cornerRadius, style: .continuous)
                    .fill(.ultraThinMaterial)
                    .overlay {
                        RoundedRectangle(cornerRadius: cornerRadius, style: .continuous)
                            .fill(Theme.surface1.opacity(0.7))
                    }
                    .overlay {
                        RoundedRectangle(cornerRadius: cornerRadius, style: .continuous)
                            .stroke(Theme.goldBorder.opacity(borderOpacity), lineWidth: 1)
                    }
                    .overlay(alignment: .top) {
                        RoundedRectangle(cornerRadius: cornerRadius, style: .continuous)
                            .fill(
                                LinearGradient(
                                    colors: [Theme.gold.opacity(0.15), .clear],
                                    startPoint: .leading,
                                    endPoint: .trailing
                                )
                            )
                            .frame(height: 2)
                            .clipShape(RoundedRectangle(cornerRadius: cornerRadius, style: .continuous))
                    }
            }
    }
}

extension View {
    func glassCard(padding: CGFloat = 20, cornerRadius: CGFloat = 20, borderOpacity: Double = 1.0) -> some View {
        modifier(GlassCardModifier(padding: padding, cornerRadius: cornerRadius, borderOpacity: borderOpacity))
    }
}
