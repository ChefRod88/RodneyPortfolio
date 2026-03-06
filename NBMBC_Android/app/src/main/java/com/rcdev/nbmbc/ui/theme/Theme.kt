package com.rcdev.nbmbc.ui.theme

import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.darkColorScheme
import androidx.compose.runtime.Composable
import androidx.compose.ui.graphics.Brush
import androidx.compose.ui.graphics.Color

// ── Design Tokens ────────────────────────────────────────────────────────────
object AppColors {
    val Gold        = Color(0xFFC9A84C)
    val GoldLight   = Color(0xFFF0D080)
    val Background  = Color(0xFF070710)
    val Surface1    = Color(0xFF0E0E1C)
    val Surface2    = Color(0xFF14142A)
    val Surface3    = Color(0xFF1C1C38)
    val TextPrimary = Color(0xFFF0EDE6)
    val TextMuted   = Color(0xFF9A9485)
    val GoldBorder  = Color(0x2EC9A84C)
    val GoldGlow    = Color(0x59C9A84C)
    val Glass       = Color(0x0AFFFFFF)
    val CashGreen   = Color(0xFF00D632)
    val CashGreenDark = Color(0xFF009922)
    val LiveRed     = Color(0xFFE53935)
}

val GoldGradient = Brush.linearGradient(
    colors = listOf(AppColors.Gold, Color(0xFF7A5500))
)
val GoldTextGradient = Brush.linearGradient(
    colors = listOf(Color.White, AppColors.GoldLight, AppColors.Gold)
)
val SurfaceGradient = Brush.linearGradient(
    colors = listOf(AppColors.Surface2, AppColors.Background, AppColors.Surface1)
)
val HeroGradient = Brush.verticalGradient(
    colors = listOf(Color.Transparent, AppColors.Background.copy(alpha = 0.9f))
)
val GoldDividerBrush = Brush.horizontalGradient(
    colors = listOf(Color.Transparent, AppColors.Gold, Color.Transparent)
)

private val DarkColorScheme = darkColorScheme(
    primary = AppColors.Gold,
    onPrimary = AppColors.Background,
    secondary = AppColors.GoldLight,
    onSecondary = AppColors.Background,
    background = AppColors.Background,
    surface = AppColors.Surface1,
    onBackground = AppColors.TextPrimary,
    onSurface = AppColors.TextPrimary,
)

@Composable
fun NBMBCTheme(content: @Composable () -> Unit) {
    MaterialTheme(
        colorScheme = DarkColorScheme,
        content = content
    )
}
