import SwiftUI

/// Animated matrix-style constellation background using TimelineView + Canvas.
/// Particles animate deterministically based on elapsed time — no mutable state in Canvas.
struct ConstellationView: View {
    var particleCount: Int = 60
    var maxDist: Double = 130
    var goldColor: Color = Theme.gold

    var body: some View {
        GeometryReader { geo in
            TimelineView(.animation) { timeline in
                let elapsed = timeline.date.timeIntervalSinceReferenceDate
                Canvas { ctx, size in
                    let pts = computeParticles(elapsed: elapsed, size: size)
                    // Draw lines
                    for i in 0..<pts.count {
                        for j in (i + 1)..<pts.count {
                            let dx = pts[i].x - pts[j].x
                            let dy = pts[i].y - pts[j].y
                            let d = sqrt(dx * dx + dy * dy)
                            guard d < maxDist else { continue }
                            let alpha = 0.18 * (1 - d / maxDist)
                            var path = Path()
                            path.move(to: CGPoint(x: pts[i].x, y: pts[i].y))
                            path.addLine(to: CGPoint(x: pts[j].x, y: pts[j].y))
                            ctx.stroke(path,
                                       with: .color(goldColor.opacity(alpha)),
                                       lineWidth: 0.6)
                        }
                    }
                    // Draw dots
                    for p in pts {
                        let rect = CGRect(x: p.x - p.r, y: p.y - p.r, width: p.r * 2, height: p.r * 2)
                        ctx.fill(Path(ellipseIn: rect), with: .color(goldColor.opacity(p.a)))
                    }
                }
            }
        }
        .allowsHitTesting(false)
    }

    private struct Particle {
        var x, y, r, a: Double
    }

    private func computeParticles(elapsed: Double, size: CGSize) -> [Particle] {
        (0..<particleCount).map { i in
            let seed = Double(i) * 137.508
            let baseX = (sin(seed * 0.7913) * 0.5 + 0.5) * size.width
            let baseY = (cos(seed * 0.4127) * 0.5 + 0.5) * size.height
            let vx = sin(seed * 1.3307) * 0.18
            let vy = cos(seed * 0.8191) * 0.18
            var x = baseX + vx * elapsed * 30
            var y = baseY + vy * elapsed * 30
            x = x.truncatingRemainder(dividingBy: size.width)
            y = y.truncatingRemainder(dividingBy: size.height)
            if x < 0 { x += size.width }
            if y < 0 { y += size.height }
            let r = (sin(seed * 2.1) * 0.5 + 0.5) * 1.2 + 0.25
            let a = (sin(seed * 1.7) * 0.5 + 0.5) * 0.35 + 0.08
            return Particle(x: x, y: y, r: r, a: a)
        }
    }
}

#Preview {
    ConstellationView()
        .background(Theme.background)
        .ignoresSafeArea()
}
