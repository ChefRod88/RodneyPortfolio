import SwiftUI

struct GiveView: View {
    @Environment(\.dismiss) private var dismiss
    @EnvironmentObject var store: DataStore
    @State private var selectedAmount = 25

    private let amounts = [10, 25, 50, 100]

    var body: some View {
        ZStack {
            Theme.background.ignoresSafeArea()
            ConstellationView(particleCount: 30)
                .ignoresSafeArea()
                .opacity(0.4)

            ScrollView(showsIndicators: false) {
                VStack(spacing: 28) {
                    // Handle bar indicator
                    RoundedRectangle(cornerRadius: 2)
                        .fill(Theme.textMuted.opacity(0.3))
                        .frame(width: 40, height: 4)
                        .padding(.top, 12)

                    // Header
                    VStack(spacing: 12) {
                        ZStack {
                            Circle()
                                .fill(Theme.goldGradient)
                                .frame(width: 72, height: 72)
                                .shadow(color: Theme.goldGlow, radius: 20)
                            Image(systemName: "heart.fill")
                                .font(.system(size: 30))
                                .foregroundColor(Theme.background)
                        }
                        Text("Support The Ministry")
                            .font(.system(size: 26, weight: .black, design: .rounded))
                            .foregroundStyle(Theme.goldTextGradient)
                        Text("Your generosity fuels the Gospel mission at New Bethel — feeding the hungry, reaching the streets, and sharing the love of Christ.")
                            .font(.system(size: 15))
                            .foregroundColor(Theme.textMuted)
                            .multilineTextAlignment(.center)
                            .lineSpacing(4)
                            .padding(.horizontal, 8)
                    }

                    // Amount Selector
                    VStack(alignment: .leading, spacing: 12) {
                        Text("SELECT AN AMOUNT")
                            .font(.system(size: 11, weight: .semibold))
                            .tracking(2.5)
                            .foregroundColor(Theme.gold)

                        HStack(spacing: 10) {
                            ForEach(amounts, id: \.self) { amount in
                                Button {
                                    withAnimation(.spring(response: 0.3)) {
                                        selectedAmount = amount
                                    }
                                } label: {
                                    Text("$\(amount)")
                                        .font(.system(size: 16, weight: .bold))
                                        .foregroundColor(selectedAmount == amount ? Theme.background : Theme.goldLight)
                                        .frame(maxWidth: .infinity)
                                        .padding(.vertical, 14)
                                        .background {
                                            Capsule()
                                                .fill(selectedAmount == amount ? Theme.goldGradient : AnyShapeStyle(Theme.gold.opacity(0.08)))
                                                .overlay {
                                                    Capsule()
                                                        .stroke(selectedAmount == amount ? Theme.gold : Theme.gold.opacity(0.22), lineWidth: 1)
                                                }
                                        }
                                        .shadow(color: selectedAmount == amount ? Theme.goldGlow : .clear, radius: 8)
                                }
                                .buttonStyle(.plain)
                            }
                        }

                        Text("Or give any amount directly in Cash App")
                            .font(.system(size: 12))
                            .foregroundColor(Theme.textMuted)
                    }
                    .glassCard(padding: 20, cornerRadius: 20)

                    // Cash App Card
                    VStack(spacing: 16) {
                        HStack(spacing: 16) {
                            ZStack {
                                RoundedRectangle(cornerRadius: 14, style: .continuous)
                                    .fill(
                                        LinearGradient(colors: [Color(hex: "00D632"), Color(hex: "009922")], startPoint: .topLeading, endPoint: .bottomTrailing)
                                    )
                                    .frame(width: 56, height: 56)
                                    .shadow(color: Color(hex: "00D632").opacity(0.4), radius: 12)
                                Text("$")
                                    .font(.system(size: 26, weight: .black))
                                    .foregroundColor(.white)
                            }

                            VStack(alignment: .leading, spacing: 4) {
                                Text("PASTOR'S CASH APP")
                                    .font(.system(size: 10, weight: .semibold))
                                    .tracking(2)
                                    .foregroundColor(Theme.textMuted)
                                Text(store.church.cashAppTag)
                                    .font(.system(size: 22, weight: .bold))
                                    .foregroundColor(Theme.goldLight)
                                    .tracking(1)
                            }
                            Spacer()
                        }

                        // Open Cash App Button
                        Link(destination: URL(string: store.church.cashAppURL)!) {
                            HStack(spacing: 10) {
                                Text("Open Cash App")
                                    .font(.system(size: 17, weight: .bold))
                                Image(systemName: "arrow.right")
                                    .font(.system(size: 15, weight: .bold))
                            }
                            .foregroundColor(.white)
                            .frame(maxWidth: .infinity)
                            .padding(.vertical, 16)
                            .background(
                                LinearGradient(
                                    colors: [Color(hex: "00D632"), Color(hex: "009922")],
                                    startPoint: .leading,
                                    endPoint: .trailing
                                )
                            )
                            .clipShape(RoundedRectangle(cornerRadius: 16, style: .continuous))
                            .shadow(color: Color(hex: "00D632").opacity(0.4), radius: 14, y: 6)
                        }
                        .buttonStyle(.plain)

                        HStack(spacing: 6) {
                            Image(systemName: "lock.fill")
                                .font(.system(size: 11))
                            Text("Secure payment via Cash App  •  \(store.church.cashAppTag)")
                                .font(.system(size: 12))
                        }
                        .foregroundColor(Theme.textMuted)
                    }
                    .glassCard(padding: 20, cornerRadius: 20)

                    // Scripture
                    VStack(alignment: .leading, spacing: 10) {
                        Text(""Each of you should give what you have decided in your heart to give, not reluctantly or under compulsion, for God loves a cheerful giver."")
                            .font(.system(size: 14, weight: .regular, design: .serif))
                            .italic()
                            .foregroundColor(Theme.textPrimary.opacity(0.7))
                            .lineSpacing(4)
                        Text("— 2 Corinthians 9:7")
                            .font(.system(size: 11, weight: .semibold))
                            .tracking(1.5)
                            .textCase(.uppercase)
                            .foregroundColor(Theme.gold)
                    }
                    .padding(16)
                    .background {
                        RoundedRectangle(cornerRadius: 14, style: .continuous)
                            .fill(Theme.gold.opacity(0.05))
                            .overlay(alignment: .leading) {
                                RoundedRectangle(cornerRadius: 2)
                                    .fill(Theme.gold)
                                    .frame(width: 2)
                            }
                    }

                    Spacer(minLength: 40)
                }
                .padding(.horizontal, 20)
            }
        }
    }
}

#Preview {
    GiveView()
        .environmentObject(DataStore())
}
