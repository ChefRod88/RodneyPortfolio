import SwiftUI
import WebKit

struct SermonDetailView: View {
    let sermon: Sermon
    @Environment(\.dismiss) private var dismiss

    var body: some View {
        ZStack {
            Theme.background.ignoresSafeArea()
            ConstellationView(particleCount: 30)
                .ignoresSafeArea()
                .opacity(0.3)

            ScrollView(showsIndicators: false) {
                VStack(spacing: 0) {
                    // Video Player Area
                    VideoPlayerSection(videoURL: sermon.videoURL)

                    // Sermon Info
                    VStack(alignment: .leading, spacing: 20) {
                        if let series = sermon.series {
                            EyebrowBadge(title: series, showDot: false)
                        }

                        Text(sermon.title)
                            .font(.system(size: 28, weight: .bold, design: .rounded))
                            .foregroundColor(.white)

                        HStack(spacing: 12) {
                            Label(sermon.speaker, systemImage: "person.fill")
                            Spacer()
                            Label(sermon.formattedDate, systemImage: "calendar")
                        }
                        .font(.system(size: 13))
                        .foregroundColor(Theme.textMuted)

                        Rectangle()
                            .fill(LinearGradient(colors: [.clear, Theme.gold, .clear], startPoint: .leading, endPoint: .trailing))
                            .frame(height: 1)

                        if let desc = sermon.description {
                            Text(desc)
                                .font(.system(size: 16))
                                .foregroundColor(Theme.textPrimary.opacity(0.8))
                                .lineSpacing(6)
                        }

                        // Scripture Callout
                        ScriptureCallout(
                            text: "So faith comes from hearing, and hearing through the word of Christ.",
                            reference: "Romans 10:17"
                        )

                        // Share / Actions
                        HStack(spacing: 12) {
                            ShareButton()
                            if sermon.videoURL != nil {
                                GoldButton(title: "Watch Full Sermon", icon: "play.circle.fill", fullWidth: true) {}
                            }
                        }
                    }
                    .padding(24)
                    .padding(.bottom, 120)
                }
            }
        }
        .navigationTitle(sermon.title)
        .navigationBarTitleDisplayMode(.inline)
        .toolbarColorScheme(.dark, for: .navigationBar)
        .toolbarBackground(Theme.surface1, for: .navigationBar)
    }
}

private struct VideoPlayerSection: View {
    let videoURL: String?

    var body: some View {
        ZStack {
            LinearGradient(
                colors: [Theme.surface2, Theme.surface3, Theme.background],
                startPoint: .top,
                endPoint: .bottom
            )

            if let url = videoURL, let webURL = URL(string: url) {
                WebView(url: webURL)
            } else {
                VStack(spacing: 16) {
                    Image(systemName: "play.rectangle.fill")
                        .font(.system(size: 60))
                        .foregroundStyle(Theme.goldTextGradient)
                    Text("Video Coming Soon")
                        .font(.system(size: 16, weight: .semibold))
                        .foregroundColor(Theme.textMuted)
                }
            }
        }
        .frame(height: 230)
        .overlay(alignment: .top) {
            LinearGradient(
                colors: [Theme.surface2, .clear],
                startPoint: .top,
                endPoint: .bottom
            )
            .frame(height: 3)
        }
    }
}

private struct ScriptureCallout: View {
    let text: String
    let reference: String

    var body: some View {
        VStack(alignment: .leading, spacing: 10) {
            Text(""\(text)"")
                .font(.system(size: 15, weight: .regular, design: .serif))
                .italic()
                .foregroundColor(Theme.textPrimary.opacity(0.75))
                .lineSpacing(4)
            Text("— \(reference)")
                .font(.system(size: 11, weight: .semibold))
                .tracking(1.5)
                .textCase(.uppercase)
                .foregroundColor(Theme.gold)
        }
        .padding(16)
        .background {
            RoundedRectangle(cornerRadius: 12, style: .continuous)
                .fill(Theme.gold.opacity(0.06))
                .overlay(alignment: .leading) {
                    RoundedRectangle(cornerRadius: 2)
                        .fill(Theme.gold)
                        .frame(width: 2)
                }
        }
    }
}

private struct ShareButton: View {
    var body: some View {
        Button {
            // Share sheet
        } label: {
            HStack(spacing: 8) {
                Image(systemName: "square.and.arrow.up")
                Text("Share")
            }
            .font(.system(size: 14, weight: .semibold))
            .foregroundColor(Theme.goldLight)
            .padding(.horizontal, 24)
            .padding(.vertical, 14)
            .background {
                Capsule()
                    .stroke(Theme.gold.opacity(0.4), lineWidth: 1)
            }
        }
        .buttonStyle(.plain)
    }
}

// MARK: - WKWebView wrapper
struct WebView: UIViewRepresentable {
    let url: URL

    func makeUIView(context: Context) -> WKWebView {
        let config = WKWebViewConfiguration()
        config.allowsInlineMediaPlayback = true
        let webView = WKWebView(frame: .zero, configuration: config)
        webView.backgroundColor = UIColor(Theme.background)
        webView.scrollView.isScrollEnabled = false
        return webView
    }

    func updateUIView(_ webView: WKWebView, context: Context) {
        webView.load(URLRequest(url: url))
    }
}

#Preview {
    NavigationStack {
        SermonDetailView(sermon: DataStore().sermons[0])
    }
    .environmentObject(DataStore())
}
