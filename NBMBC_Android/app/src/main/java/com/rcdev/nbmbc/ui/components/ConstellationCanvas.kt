package com.rcdev.nbmbc.ui.components

import androidx.compose.animation.core.withInfiniteAnimationFrameMillis
import androidx.compose.foundation.Canvas
import androidx.compose.runtime.*
import androidx.compose.ui.Modifier
import androidx.compose.ui.geometry.Offset
import androidx.compose.ui.graphics.Color
import com.rcdev.nbmbc.ui.theme.AppColors
import kotlin.math.*

@Composable
fun ConstellationCanvas(
    modifier: Modifier = Modifier,
    particleCount: Int = 60,
    maxDist: Float = 280f,
    goldColor: Color = AppColors.Gold
) {
    var time by remember { mutableLongStateOf(0L) }

    LaunchedEffect(Unit) {
        while (true) {
            withInfiniteAnimationFrameMillis { frameTimeMillis ->
                time = frameTimeMillis
            }
        }
    }

    Canvas(modifier = modifier) {
        val w = size.width
        val h = size.height
        val elapsed = time / 1000.0

        // Compute particle positions deterministically from elapsed time
        val particles = (0 until particleCount).map { i ->
            val seed = i * 137.508
            val baseX = (sin(seed * 0.7913) * 0.5 + 0.5) * w
            val baseY = (cos(seed * 0.4127) * 0.5 + 0.5) * h
            val vx = sin(seed * 1.3307) * 0.18f
            val vy = cos(seed * 0.8191) * 0.18f
            var x = (baseX + vx * elapsed * 30).toFloat() % w
            var y = (baseY + vy * elapsed * 30).toFloat() % h
            if (x < 0) x += w
            if (y < 0) y += h
            val r = ((sin(seed * 2.1) * 0.5 + 0.5) * 2.4 + 0.5).toFloat()
            val a = ((sin(seed * 1.7) * 0.5 + 0.5) * 0.35 + 0.08).toFloat()
            Particle(x, y, r, a)
        }

        // Draw connecting lines
        for (i in particles.indices) {
            for (j in (i + 1) until particles.size) {
                val dx = particles[i].x - particles[j].x
                val dy = particles[i].y - particles[j].y
                val dist = sqrt(dx * dx + dy * dy)
                if (dist < maxDist) {
                    val alpha = 0.18f * (1f - dist / maxDist)
                    drawLine(
                        color = goldColor.copy(alpha = alpha),
                        start = Offset(particles[i].x, particles[i].y),
                        end = Offset(particles[j].x, particles[j].y),
                        strokeWidth = 1.2f
                    )
                }
            }
        }

        // Draw dots
        for (p in particles) {
            drawCircle(
                color = goldColor.copy(alpha = p.alpha),
                radius = p.radius,
                center = Offset(p.x, p.y)
            )
        }
    }
}

private data class Particle(val x: Float, val y: Float, val radius: Float, val alpha: Float)
