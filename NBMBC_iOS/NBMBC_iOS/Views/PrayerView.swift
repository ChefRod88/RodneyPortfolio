import SwiftUI

struct PrayerView: View {
    @State private var request = PrayerRequest()
    @State private var submitted = false
    @State private var isSubmitting = false
    @State private var charCount = 0

    private let categories: [PrayerCategory] = [
        PrayerCategory(icon: "🏥", label: "Healing",        placeholder: "I'm requesting prayer for healing and health..."),
        PrayerCategory(icon: "👨‍👩‍👧", label: "Family",         placeholder: "I'm requesting prayer for my family..."),
        PrayerCategory(icon: "💼", label: "Finances",       placeholder: "I'm requesting prayer for financial breakthrough..."),
        PrayerCategory(icon: "🌱", label: "Spiritual Growth", placeholder: "I'm requesting prayer for spiritual direction..."),
        PrayerCategory(icon: "💙", label: "Grief & Loss",   placeholder: "I'm requesting prayer as I navigate grief..."),
        PrayerCategory(icon: "🙏", label: "General",        placeholder: "I have a prayer request I'd like to share..."),
    ]

    var body: some View {
        NavigationStack {
            ZStack {
                Theme.background.ignoresSafeArea()
                ConstellationView(particleCount: 35)
                    .ignoresSafeArea()
                    .opacity(0.45)

                if submitted {
                    PrayerSuccessView()
                } else {
                    ScrollView(showsIndicators: false) {
                        VStack(spacing: 24) {
                            // Hero
                            PrayerHero()

                            // Form
                            VStack(spacing: 20) {
                                // Category Chips
                                CategorySection(categories: categories, selected: $request.category)

                                // Name Field
                                PrayerField(label: "Your Name", isOptional: true) {
                                    PrayerTextInput(text: $request.name, placeholder: "Your name", keyboard: .default)
                                }

                                // Email Field
                                PrayerField(label: "Email", isOptional: true) {
                                    PrayerTextInput(text: $request.email, placeholder: "your@email.com", keyboard: .emailAddress)
                                }

                                // Prayer Request
                                PrayerField(label: "Your Prayer Request", isOptional: false) {
                                    VStack(alignment: .trailing, spacing: 6) {
                                        PrayerTextEditor(text: $request.request, charCount: $charCount)
                                        Text("\(charCount) / 1000")
                                            .font(.system(size: 11))
                                            .foregroundColor(Theme.textMuted)
                                    }
                                }

                                // Anonymous Toggle
                                AnonymousToggle(isOn: $request.shareAnonymously)

                                // Submit
                                GoldButton(title: isSubmitting ? "Submitting..." : "Submit Prayer Request",
                                           icon: isSubmitting ? nil : "hands.sparkles.fill",
                                           fullWidth: true) {
                                    submitPrayer()
                                }
                                .disabled(request.request.trimmingCharacters(in: .whitespacesAndNewlines).isEmpty || isSubmitting)
                            }
                            .glassCard(padding: 20, cornerRadius: 24)
                            .padding(.horizontal, 16)

                            // Sidebar info cards
                            SidebarCards()
                                .padding(.horizontal, 16)

                            Spacer(minLength: 120)
                        }
                    }
                }
            }
            .navigationTitle("Request Prayer")
            .navigationBarTitleDisplayMode(.inline)
            .toolbarColorScheme(.dark, for: .navigationBar)
            .toolbarBackground(Theme.surface1, for: .navigationBar)
        }
    }

    private func submitPrayer() {
        guard !request.request.trimmingCharacters(in: .whitespacesAndNewlines).isEmpty else { return }
        isSubmitting = true
        DispatchQueue.main.asyncAfter(deadline: .now() + 1.5) {
            isSubmitting = false
            submitted = true
        }
    }
}

// MARK: - Sub-views

private struct PrayerHero: View {
    var body: some View {
        VStack(spacing: 14) {
            EyebrowBadge(title: "We're Here For You")
            Text("Request Prayer")
                .font(.system(size: 36, weight: .black, design: .rounded))
                .foregroundStyle(
                    LinearGradient(colors: [.white, Theme.goldLight, Theme.gold], startPoint: .topLeading, endPoint: .bottomTrailing)
                )
            Text("We would be honored to lift you up. Share your request and our prayer team will stand with you in faith.")
                .font(.system(size: 15))
                .foregroundColor(Theme.textMuted)
                .multilineTextAlignment(.center)
                .lineSpacing(4)
        }
        .padding(.horizontal, 24)
        .padding(.top, 20)
    }
}

private struct CategorySection: View {
    let categories: [PrayerCategory]
    @Binding var selected: String

    var body: some View {
        VStack(alignment: .leading, spacing: 10) {
            Text("PRAYER CATEGORY")
                .font(.system(size: 11, weight: .semibold))
                .tracking(2.5)
                .foregroundColor(Theme.gold)
            FlowLayout(spacing: 8) {
                ForEach(categories) { cat in
                    Button {
                        selected = cat.label
                    } label: {
                        Text("\(cat.icon) \(cat.label)")
                            .font(.system(size: 13, weight: .medium))
                            .foregroundColor(selected == cat.label ? .white : Theme.goldLight)
                            .padding(.horizontal, 14)
                            .padding(.vertical, 8)
                            .background {
                                Capsule()
                                    .fill(selected == cat.label ? Theme.gold.opacity(0.25) : Theme.gold.opacity(0.06))
                                    .overlay {
                                        Capsule()
                                            .stroke(selected == cat.label ? Theme.gold : Theme.gold.opacity(0.2), lineWidth: 1)
                                    }
                            }
                    }
                    .buttonStyle(.plain)
                }
            }
        }
    }
}

private struct PrayerField<Content: View>: View {
    let label: String
    let isOptional: Bool
    @ViewBuilder let content: () -> Content

    var body: some View {
        VStack(alignment: .leading, spacing: 8) {
            HStack(spacing: 6) {
                Text(label.uppercased())
                    .font(.system(size: 11, weight: .semibold))
                    .tracking(2)
                    .foregroundColor(Theme.gold)
                if isOptional {
                    Text("(optional)")
                        .font(.system(size: 11))
                        .foregroundColor(Theme.textMuted)
                }
            }
            content()
        }
    }
}

private struct PrayerTextInput: View {
    @Binding var text: String
    let placeholder: String
    let keyboard: UIKeyboardType
    @FocusState private var focused: Bool

    var body: some View {
        TextField(placeholder, text: $text)
            .keyboardType(keyboard)
            .focused($focused)
            .padding(14)
            .background {
                RoundedRectangle(cornerRadius: 12, style: .continuous)
                    .fill(Color.white.opacity(0.04))
                    .overlay {
                        RoundedRectangle(cornerRadius: 12, style: .continuous)
                            .stroke(focused ? Theme.gold : Theme.gold.opacity(0.2), lineWidth: 1)
                    }
                    .shadow(color: focused ? Theme.gold.opacity(0.12) : .clear, radius: 8)
            }
            .foregroundColor(Theme.textPrimary)
            .tint(Theme.gold)
    }
}

private struct PrayerTextEditor: View {
    @Binding var text: String
    @Binding var charCount: Int
    @FocusState private var focused: Bool

    var body: some View {
        TextEditor(text: $text)
            .focused($focused)
            .frame(minHeight: 120)
            .padding(10)
            .scrollContentBackground(.hidden)
            .background {
                RoundedRectangle(cornerRadius: 12, style: .continuous)
                    .fill(Color.white.opacity(0.04))
                    .overlay {
                        RoundedRectangle(cornerRadius: 12, style: .continuous)
                            .stroke(focused ? Theme.gold : Theme.gold.opacity(0.2), lineWidth: 1)
                    }
            }
            .foregroundColor(Theme.textPrimary)
            .tint(Theme.gold)
            .onChange(of: text) { _, val in
                if val.count > 1000 { text = String(val.prefix(1000)) }
                charCount = text.count
            }
    }
}

private struct AnonymousToggle: View {
    @Binding var isOn: Bool

    var body: some View {
        HStack(spacing: 12) {
            Toggle("", isOn: $isOn)
                .labelsHidden()
                .tint(Theme.gold)
            Text("Share my request anonymously with the congregation")
                .font(.system(size: 14))
                .foregroundColor(Theme.textMuted)
                .fixedSize(horizontal: false, vertical: true)
        }
    }
}

private struct SidebarCards: View {
    var body: some View {
        VStack(spacing: 14) {
            PrayerInfoCard(eye: "Our Promise", title: "You Are Not Alone",
                          body: "Our prayer team receives every request and lifts each need before the Lord with faith, sincerity, and love. You are seen, heard, and covered.")
            ScriptureInfoCard(verse: "Do not be anxious about anything, but in every situation, by prayer and petition, with thanksgiving, present your requests to God.", ref: "Philippians 4:6")
            ScriptureInfoCard(verse: "The prayer of a righteous person is powerful and effective.", ref: "James 5:16")
            NavigationLink(destination: LocationView()) {
                PrayerInfoCard(eye: "Need To Talk In Person?", title: "Come See Us",
                              body: "Our pastoral team is available for prayer and counseling. Join us any Sunday at New Bethel.")
            }
            .buttonStyle(.plain)
        }
    }
}

private struct PrayerInfoCard: View {
    let eye: String
    let title: String
    let body: String

    var body: some View {
        VStack(alignment: .leading, spacing: 8) {
            Text(eye.uppercased())
                .font(.system(size: 10, weight: .semibold))
                .tracking(2.5)
                .foregroundColor(Theme.gold)
            Text(title)
                .font(.system(size: 17, weight: .bold))
                .foregroundColor(.white)
            Text(body)
                .font(.system(size: 14))
                .foregroundColor(Theme.textMuted)
                .lineSpacing(4)
        }
        .glassCard(padding: 18, cornerRadius: 18)
    }
}

private struct ScriptureInfoCard: View {
    let verse: String
    let ref: String

    var body: some View {
        VStack(alignment: .leading, spacing: 10) {
            Text(""\(verse)"")
                .font(.system(size: 14, weight: .regular, design: .serif))
                .italic()
                .foregroundColor(Theme.textPrimary.opacity(0.75))
                .lineSpacing(4)
            Text("— \(ref)")
                .font(.system(size: 11, weight: .semibold))
                .tracking(1.5)
                .textCase(.uppercase)
                .foregroundColor(Theme.gold)
        }
        .padding(16)
        .background {
            RoundedRectangle(cornerRadius: 16, style: .continuous)
                .fill(Theme.gold.opacity(0.05))
                .overlay {
                    RoundedRectangle(cornerRadius: 16, style: .continuous)
                        .stroke(Theme.goldBorder, lineWidth: 1)
                }
                .overlay(alignment: .leading) {
                    RoundedRectangle(cornerRadius: 2)
                        .fill(Theme.gold)
                        .frame(width: 2)
                }
        }
    }
}

private struct PrayerSuccessView: View {
    var body: some View {
        VStack(spacing: 28) {
            Spacer()
            ZStack {
                Circle()
                    .fill(Theme.goldGradient)
                    .frame(width: 100, height: 100)
                    .shadow(color: Theme.goldGlow, radius: 30)
                Text("🙏")
                    .font(.system(size: 44))
            }
            .scaleEffect(1.0)
            .animation(.spring(response: 0.6, dampingFraction: 0.6), value: true)

            VStack(spacing: 12) {
                Text("Thank You")
                    .font(.system(size: 32, weight: .black, design: .rounded))
                    .foregroundStyle(Theme.goldTextGradient)
                Text("We're praying for you. Our prayer team has received your request and will be lifting you up before the Lord.")
                    .font(.system(size: 16))
                    .foregroundColor(Theme.textMuted)
                    .multilineTextAlignment(.center)
                    .lineSpacing(5)
                    .padding(.horizontal, 32)
            }
            Spacer()
        }
    }
}

// MARK: - Flow Layout (Wrap)
struct FlowLayout: Layout {
    var spacing: CGFloat = 8

    func sizeThatFits(proposal: ProposedViewSize, subviews: Subviews, cache: inout ()) -> CGSize {
        let width = proposal.width ?? 300
        var height: CGFloat = 0
        var rowWidth: CGFloat = 0
        var rowHeight: CGFloat = 0
        for subview in subviews {
            let size = subview.sizeThatFits(.unspecified)
            if rowWidth + size.width > width && rowWidth > 0 {
                height += rowHeight + spacing
                rowWidth = 0
                rowHeight = 0
            }
            rowWidth += size.width + spacing
            rowHeight = max(rowHeight, size.height)
        }
        height += rowHeight
        return CGSize(width: width, height: height)
    }

    func placeSubviews(in bounds: CGRect, proposal: ProposedViewSize, subviews: Subviews, cache: inout ()) {
        var x = bounds.minX
        var y = bounds.minY
        var rowHeight: CGFloat = 0
        for subview in subviews {
            let size = subview.sizeThatFits(.unspecified)
            if x + size.width > bounds.maxX && x > bounds.minX {
                y += rowHeight + spacing
                x = bounds.minX
                rowHeight = 0
            }
            subview.place(at: CGPoint(x: x, y: y), proposal: ProposedViewSize(size))
            x += size.width + spacing
            rowHeight = max(rowHeight, size.height)
        }
    }
}

#Preview {
    PrayerView()
        .environmentObject(DataStore())
}
