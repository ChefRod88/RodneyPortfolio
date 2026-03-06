import SwiftUI

struct GroupsView: View {
    @EnvironmentObject var store: DataStore
    @State private var selectedCategory: String? = nil

    private var categories: [String] {
        let cats = store.groups.map { $0.category }
        return Array(Set(cats)).sorted()
    }

    private var filteredGroups: [Group] {
        guard let cat = selectedCategory else { return store.groups }
        return store.groups.filter { $0.category == cat }
    }

    var body: some View {
        ZStack {
            Theme.background.ignoresSafeArea()
            ConstellationView(particleCount: 35)
                .ignoresSafeArea()
                .opacity(0.4)

            ScrollView(showsIndicators: false) {
                LazyVStack(spacing: 0) {
                    // Hero
                    GroupsHero()

                    // Category Filter
                    ScrollView(.horizontal, showsIndicators: false) {
                        HStack(spacing: 10) {
                            FilterChip(label: "All", isSelected: selectedCategory == nil) {
                                selectedCategory = nil
                            }
                            ForEach(categories, id: \.self) { cat in
                                FilterChip(label: cat, isSelected: selectedCategory == cat) {
                                    selectedCategory = selectedCategory == cat ? nil : cat
                                }
                            }
                        }
                        .padding(.horizontal, 16)
                    }
                    .padding(.vertical, 16)

                    // Groups Grid
                    VStack(spacing: 14) {
                        ForEach(filteredGroups) { group in
                            GroupCard(group: group)
                        }
                    }
                    .padding(.horizontal, 16)
                    .padding(.bottom, 120)
                    .animation(.spring(response: 0.35), value: filteredGroups.map { $0.id })
                }
            }
        }
        .navigationTitle("Groups")
        .navigationBarTitleDisplayMode(.inline)
        .toolbarColorScheme(.dark, for: .navigationBar)
        .toolbarBackground(Theme.surface1, for: .navigationBar)
    }
}

private struct GroupsHero: View {
    var body: some View {
        ZStack {
            LinearGradient(colors: [Theme.surface2, Theme.background], startPoint: .top, endPoint: .bottom)
            GridPattern()
            VStack(spacing: 16) {
                EyebrowBadge(title: "Get Involved")
                Text("Find Your Group")
                    .font(.system(size: 36, weight: .black, design: .rounded))
                    .foregroundStyle(
                        LinearGradient(colors: [.white, Theme.goldLight, Theme.gold], startPoint: .topLeading, endPoint: .bottomTrailing)
                    )
                Text("Connect with a community that will help you grow in faith, build friendships, and make a difference.")
                    .font(.system(size: 15))
                    .foregroundColor(Theme.textMuted)
                    .multilineTextAlignment(.center)
                    .lineSpacing(4)
            }
            .padding(36)
        }
    }
}

struct GroupCard: View {
    let group: Group

    var body: some View {
        HStack(spacing: 16) {
            // Icon
            ZStack {
                RoundedRectangle(cornerRadius: 16, style: .continuous)
                    .fill(
                        LinearGradient(
                            colors: [Theme.surface2, Theme.surface3],
                            startPoint: .topLeading,
                            endPoint: .bottomTrailing
                        )
                    )
                    .frame(width: 70, height: 70)
                    .overlay {
                        RoundedRectangle(cornerRadius: 16, style: .continuous)
                            .stroke(Theme.goldBorder, lineWidth: 1)
                    }
                Image(systemName: group.icon)
                    .font(.system(size: 28))
                    .foregroundStyle(Theme.goldTextGradient)
            }

            VStack(alignment: .leading, spacing: 6) {
                HStack(spacing: 8) {
                    Text(group.category.uppercased())
                        .font(.system(size: 10, weight: .semibold))
                        .tracking(2)
                        .foregroundColor(Theme.gold)
                }
                Text(group.title)
                    .font(.system(size: 17, weight: .bold))
                    .foregroundColor(.white)
                Text(group.description)
                    .font(.system(size: 13))
                    .foregroundColor(Theme.textMuted)
                    .lineLimit(2)
                    .lineSpacing(2)

                if let time = group.meetingTime {
                    HStack(spacing: 4) {
                        Image(systemName: "clock")
                            .font(.system(size: 11))
                        Text(time)
                            .font(.system(size: 12, weight: .medium))
                    }
                    .foregroundColor(Theme.goldLight.opacity(0.7))
                }
            }

            Spacer()
        }
        .padding(16)
        .background {
            RoundedRectangle(cornerRadius: 20, style: .continuous)
                .fill(Theme.surface1)
                .overlay {
                    RoundedRectangle(cornerRadius: 20, style: .continuous)
                        .stroke(Theme.goldBorder, lineWidth: 1)
                }
                .overlay(alignment: .top) {
                    RoundedRectangle(cornerRadius: 20, style: .continuous)
                        .fill(LinearGradient(colors: [Theme.gold.opacity(0.15), .clear], startPoint: .leading, endPoint: .trailing))
                        .frame(height: 2)
                        .clipShape(RoundedRectangle(cornerRadius: 20, style: .continuous))
                }
        }
    }
}

struct FilterChip: View {
    let label: String
    let isSelected: Bool
    let action: () -> Void

    var body: some View {
        Button(action: action) {
            Text(label)
                .font(.system(size: 13, weight: .semibold))
                .foregroundColor(isSelected ? Theme.background : Theme.goldLight)
                .padding(.horizontal, 18)
                .padding(.vertical, 9)
                .background {
                    Capsule()
                        .fill(isSelected ? AnyShapeStyle(Theme.goldGradient) : AnyShapeStyle(Theme.gold.opacity(0.08)))
                        .overlay {
                            Capsule()
                                .stroke(isSelected ? Theme.gold : Theme.gold.opacity(0.25), lineWidth: 1)
                        }
                }
        }
        .buttonStyle(.plain)
    }
}

#Preview {
    NavigationStack {
        GroupsView()
    }
    .environmentObject(DataStore())
}
