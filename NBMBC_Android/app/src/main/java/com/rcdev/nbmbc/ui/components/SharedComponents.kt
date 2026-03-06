package com.rcdev.nbmbc.ui.components

import androidx.compose.animation.core.*
import androidx.compose.foundation.background
import androidx.compose.foundation.border
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.draw.drawBehind
import androidx.compose.ui.geometry.Offset
import androidx.compose.ui.graphics.*
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.unit.*
import com.rcdev.nbmbc.ui.theme.*

// ── Eyebrow Badge ─────────────────────────────────────────────────────────────
@Composable
fun EyebrowBadge(title: String, showDot: Boolean = true) {
    Row(
        modifier = Modifier
            .clip(CircleShape)
            .background(AppColors.Glass)
            .border(1.dp, AppColors.GoldBorder, CircleShape)
            .padding(horizontal = 20.dp, vertical = 5.dp),
        verticalAlignment = Alignment.CenterVertically,
        horizontalArrangement = Arrangement.spacedBy(8.dp)
    ) {
        if (showDot) AnimatedDot()
        Text(
            title.uppercase(),
            style = MaterialTheme.typography.labelSmall,
            fontWeight = FontWeight.SemiBold,
            letterSpacing = 3.sp,
            color = AppColors.Gold,
            fontSize = 11.sp
        )
    }
}

@Composable
fun AnimatedDot() {
    val infiniteTransition = rememberInfiniteTransition(label = "dot")
    val alpha by infiniteTransition.animateFloat(
        initialValue = 1f, targetValue = 0.2f,
        animationSpec = infiniteRepeatable(
            animation = tween(1000, easing = FastOutSlowInEasing),
            repeatMode = RepeatMode.Reverse
        ), label = "dotAlpha"
    )
    Box(
        modifier = Modifier
            .size(6.dp)
            .clip(CircleShape)
            .background(AppColors.Gold.copy(alpha = alpha))
    )
}

// ── Glass Card ────────────────────────────────────────────────────────────────
@Composable
fun GlassCard(
    modifier: Modifier = Modifier,
    cornerRadius: Dp = 20.dp,
    content: @Composable ColumnScope.() -> Unit
) {
    val shape = RoundedCornerShape(cornerRadius)
    Column(
        modifier = modifier
            .clip(shape)
            .background(AppColors.Surface1.copy(alpha = 0.85f))
            .border(1.dp, AppColors.GoldBorder, shape)
            .drawBehind {
                // Top gold accent bar
                drawLine(
                    brush = Brush.horizontalGradient(listOf(Color.Transparent, AppColors.Gold, Color.Transparent)),
                    start = Offset(0f, 0f),
                    end = Offset(size.width, 0f),
                    strokeWidth = 2f
                )
            }
    ) {
        content()
    }
}

// ── Gold Divider ──────────────────────────────────────────────────────────────
@Composable
fun GoldDivider(modifier: Modifier = Modifier) {
    Box(
        modifier = modifier
            .fillMaxWidth()
            .height(1.dp)
            .background(GoldDividerBrush)
    )
}

// ── Empty State ────────────────────────────────────────────────────────────────
@Composable
fun EmptyStateView(icon: String, title: String, subtitle: String) {
    Column(
        modifier = Modifier
            .fillMaxWidth()
            .padding(48.dp),
        horizontalAlignment = Alignment.CenterHorizontally,
        verticalArrangement = Arrangement.spacedBy(12.dp)
    ) {
        Text(icon, fontSize = 52.sp, textAlign = TextAlign.Center)
        Text(title, color = AppColors.TextPrimary.copy(alpha = 0.5f),
            style = MaterialTheme.typography.titleMedium, fontWeight = FontWeight.Bold,
            textAlign = TextAlign.Center)
        Text(subtitle, color = AppColors.TextMuted,
            style = MaterialTheme.typography.bodySmall, textAlign = TextAlign.Center)
    }
}

// ── Section Header ─────────────────────────────────────────────────────────────
@Composable
fun SectionHeader(eyebrow: String, title: String) {
    Column(verticalArrangement = Arrangement.spacedBy(6.dp)) {
        Text(
            eyebrow.uppercase(),
            fontSize = 11.sp,
            fontWeight = FontWeight.SemiBold,
            letterSpacing = 3.sp,
            color = AppColors.Gold
        )
        Text(
            title,
            style = MaterialTheme.typography.headlineSmall,
            fontWeight = FontWeight.Bold,
            color = AppColors.TextPrimary
        )
    }
}

// ── Scripture Quote ────────────────────────────────────────────────────────────
@Composable
fun ScriptureCard(verse: String, reference: String, modifier: Modifier = Modifier) {
    Row(
        modifier = modifier
            .fillMaxWidth()
            .clip(RoundedCornerShape(12.dp))
            .background(AppColors.Gold.copy(alpha = 0.06f))
    ) {
        Box(
            modifier = Modifier
                .width(2.dp)
                .fillMaxHeight()
                .background(AppColors.Gold)
        )
        Column(
            modifier = Modifier.padding(14.dp),
            verticalArrangement = Arrangement.spacedBy(8.dp)
        ) {
            Text(
                "\u201c$verse\u201d",
                style = MaterialTheme.typography.bodyMedium,
                color = AppColors.TextPrimary.copy(alpha = 0.75f),
                lineHeight = 22.sp
            )
            Text(
                "\u2014 $reference",
                fontSize = 11.sp,
                fontWeight = FontWeight.SemiBold,
                letterSpacing = 1.5.sp,
                color = AppColors.Gold
            )
        }
    }
}
