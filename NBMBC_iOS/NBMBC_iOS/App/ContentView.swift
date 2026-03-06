import SwiftUI

struct ContentView: View {
    @State private var selectedTab = 0
    @State private var showGive = false

    var body: some View {
        ZStack(alignment: .bottom) {
            TabView(selection: $selectedTab) {
                HomeView(showGive: $showGive)
                    .tag(0)

                SermonsView()
                    .tag(1)

                EventsView()
                    .tag(2)

                PrayerView()
                    .tag(3)

                MoreView(showGive: $showGive)
                    .tag(4)
            }
            .tabViewStyle(.page(indexDisplayMode: .never))
            .ignoresSafeArea()

            // Custom Tab Bar
            CustomTabBar(selectedTab: $selectedTab, showGive: $showGive)
        }
        .sheet(isPresented: $showGive) {
            GiveView()
                .presentationDetents([.large])
                .presentationDragIndicator(.visible)
        }
        .background(Theme.background)
        .ignoresSafeArea()
    }
}

struct CustomTabBar: View {
    @Binding var selectedTab: Int
    @Binding var showGive: Bool

    private let tabs: [(icon: String, label: String)] = [
        ("house.fill", "Home"),
        ("play.rectangle.fill", "Sermons"),
        ("calendar", "Events"),
        ("hands.sparkles.fill", "Prayer"),
        ("line.3.horizontal", "More")
    ]

    var body: some View {
        HStack(spacing: 0) {
            ForEach(Array(tabs.enumerated()), id: \.offset) { index, tab in
                Button {
                    withAnimation(.spring(response: 0.3, dampingFraction: 0.7)) {
                        selectedTab = index
                    }
                } label: {
                    VStack(spacing: 4) {
                        Image(systemName: tab.icon)
                            .font(.system(size: 20, weight: selectedTab == index ? .semibold : .regular))
                            .foregroundStyle(selectedTab == index ? Theme.gold : Theme.textMuted.opacity(0.6))
                        Text(tab.label)
                            .font(.system(size: 10, weight: .medium))
                            .foregroundStyle(selectedTab == index ? Theme.gold : Theme.textMuted.opacity(0.5))
                    }
                    .frame(maxWidth: .infinity)
                    .padding(.vertical, 10)
                    .background {
                        if selectedTab == index {
                            RoundedRectangle(cornerRadius: 12)
                                .fill(Theme.gold.opacity(0.08))
                                .padding(.horizontal, 4)
                        }
                    }
                }
                .buttonStyle(.plain)
            }
        }
        .padding(.horizontal, 8)
        .padding(.top, 8)
        .padding(.bottom, 28)
        .background {
            Rectangle()
                .fill(.ultraThinMaterial)
                .overlay {
                    Rectangle()
                        .fill(Theme.surface1.opacity(0.85))
                }
                .overlay(alignment: .top) {
                    Rectangle()
                        .fill(
                            LinearGradient(
                                colors: [Theme.gold.opacity(0.25), .clear],
                                startPoint: .leading,
                                endPoint: .trailing
                            )
                        )
                        .frame(height: 1)
                }
        }
        .ignoresSafeArea(edges: .bottom)
    }
}

#Preview {
    ContentView()
        .environmentObject(DataStore())
}
