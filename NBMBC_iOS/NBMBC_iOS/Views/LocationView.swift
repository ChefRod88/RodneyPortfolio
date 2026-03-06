import SwiftUI
import MapKit

struct LocationView: View {
    @EnvironmentObject var store: DataStore
    @State private var region = MKCoordinateRegion(
        center: CLLocationCoordinate2D(latitude: 28.0222, longitude: -81.7329),
        span: MKCoordinateSpan(latitudeDelta: 0.01, longitudeDelta: 0.01)
    )

    // Church coordinates: 2901 Avenue T NW, Winter Haven, FL
    private let churchCoord = CLLocationCoordinate2D(latitude: 28.0222, longitude: -81.7329)

    var body: some View {
        ZStack {
            Theme.background.ignoresSafeArea()
            ConstellationView(particleCount: 25)
                .ignoresSafeArea()
                .opacity(0.35)

            ScrollView(showsIndicators: false) {
                VStack(spacing: 0) {
                    // Map
                    MapSection(region: $region, churchCoord: churchCoord)

                    // Info
                    VStack(spacing: 20) {
                        AddressCard(church: store.church, churchCoord: churchCoord)
                        ServiceTimesLocationCard()
                        DirectionsCard(church: store.church, churchCoord: churchCoord)
                    }
                    .padding(20)
                    .padding(.bottom, 100)
                }
            }
        }
        .navigationTitle("Location")
        .navigationBarTitleDisplayMode(.inline)
        .toolbarColorScheme(.dark, for: .navigationBar)
        .toolbarBackground(Theme.surface1, for: .navigationBar)
    }
}

private struct MapSection: View {
    @Binding var region: MKCoordinateRegion
    let churchCoord: CLLocationCoordinate2D

    var body: some View {
        ZStack(alignment: .bottom) {
            Map(coordinateRegion: $region, annotationItems: [ChurchAnnotation(coordinate: churchCoord)]) { item in
                MapAnnotation(coordinate: item.coordinate) {
                    ChurchMapPin()
                }
            }
            .frame(height: 280)
            .colorScheme(.dark)

            LinearGradient(
                colors: [.clear, Theme.background],
                startPoint: .top,
                endPoint: .bottom
            )
            .frame(height: 80)
        }
    }
}

private struct ChurchMapPin: View {
    @State private var pulse = false

    var body: some View {
        VStack(spacing: 0) {
            ZStack {
                Circle()
                    .fill(Theme.gold.opacity(0.25))
                    .frame(width: 50, height: 50)
                    .scaleEffect(pulse ? 1.4 : 1.0)
                    .opacity(pulse ? 0 : 1)
                Circle()
                    .fill(Theme.goldGradient)
                    .frame(width: 32, height: 32)
                    .shadow(color: Theme.goldGlow, radius: 10)
                Image(systemName: "building.columns.fill")
                    .font(.system(size: 14))
                    .foregroundColor(Theme.background)
            }
            Triangle()
                .fill(Theme.goldGradient)
                .frame(width: 12, height: 8)
        }
        .onAppear {
            withAnimation(.easeOut(duration: 1.5).repeatForever(autoreverses: false)) {
                pulse = true
            }
        }
    }
}

private struct Triangle: Shape {
    func path(in rect: CGRect) -> Path {
        var path = Path()
        path.move(to: CGPoint(x: rect.midX, y: rect.maxY))
        path.addLine(to: CGPoint(x: rect.minX, y: rect.minY))
        path.addLine(to: CGPoint(x: rect.maxX, y: rect.minY))
        path.closeSubpath()
        return path
    }
}

struct ChurchAnnotation: Identifiable {
    let id = UUID()
    var coordinate: CLLocationCoordinate2D
}

private struct AddressCard: View {
    let church: ChurchInfo
    let churchCoord: CLLocationCoordinate2D

    var body: some View {
        HStack(alignment: .top, spacing: 14) {
            ZStack {
                Circle()
                    .fill(Theme.goldGradient)
                    .frame(width: 48, height: 48)
                    .shadow(color: Theme.goldGlow, radius: 12)
                Image(systemName: "location.fill")
                    .font(.system(size: 20))
                    .foregroundColor(Theme.background)
            }

            VStack(alignment: .leading, spacing: 6) {
                Text("OUR ADDRESS")
                    .font(.system(size: 10, weight: .semibold))
                    .tracking(2.5)
                    .foregroundColor(Theme.gold)
                Text(church.address)
                    .font(.system(size: 16, weight: .bold))
                    .foregroundColor(.white)
                Text("Winter Haven, Florida 33881")
                    .font(.system(size: 14))
                    .foregroundColor(Theme.textMuted)
            }
            Spacer()
        }
        .glassCard(padding: 18, cornerRadius: 18)
    }
}

private struct ServiceTimesLocationCard: View {
    private let services = [
        ("Sunday School", "9:30 AM"),
        ("Morning Worship", "10:30 AM"),
        ("Bible Study", "Wednesday 7 PM"),
    ]

    var body: some View {
        VStack(alignment: .leading, spacing: 14) {
            HStack(spacing: 10) {
                Image(systemName: "clock.fill")
                    .foregroundColor(Theme.gold)
                Text("SERVICE TIMES")
                    .font(.system(size: 11, weight: .semibold))
                    .tracking(2.5)
                    .foregroundColor(Theme.gold)
            }
            ForEach(services, id: \.0) { s in
                HStack {
                    HStack(spacing: 8) {
                        Circle()
                            .fill(Theme.gold)
                            .frame(width: 5, height: 5)
                        Text(s.0)
                            .font(.system(size: 15))
                            .foregroundColor(.white)
                    }
                    Spacer()
                    Text(s.1)
                        .font(.system(size: 14, weight: .bold))
                        .foregroundColor(Theme.goldLight)
                }
                if s.0 != services.last!.0 {
                    Divider().background(Theme.goldBorder)
                }
            }
        }
        .glassCard(padding: 18, cornerRadius: 18)
    }
}

private struct DirectionsCard: View {
    let church: ChurchInfo
    let churchCoord: CLLocationCoordinate2D

    var body: some View {
        VStack(spacing: 12) {
            GoldButton(title: "Get Directions in Maps", icon: "map.fill", fullWidth: true) {
                openInMaps()
            }
            Link(destination: URL(string: "tel:\(church.phone.filter { $0.isNumber })")!) {
                HStack(spacing: 8) {
                    Image(systemName: "phone.fill")
                    Text("Call \(church.phone)")
                }
                .font(.system(size: 15, weight: .semibold))
                .foregroundColor(Theme.goldLight)
                .frame(maxWidth: .infinity)
                .padding(.vertical, 14)
                .background { Capsule().stroke(Theme.gold.opacity(0.4), lineWidth: 1) }
            }
            .buttonStyle(.plain)
        }
    }

    private func openInMaps() {
        let mapItem = MKMapItem(placemark: MKPlacemark(coordinate: churchCoord))
        mapItem.name = "New Bethel Missionary Baptist Church"
        mapItem.openInMaps(launchOptions: [MKLaunchOptionsDirectionsModeKey: MKLaunchOptionsDirectionsModeDriving])
    }
}

#Preview {
    NavigationStack {
        LocationView()
    }
    .environmentObject(DataStore())
}
