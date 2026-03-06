package com.rcdev.nbmbc.ui.components

import androidx.compose.foundation.background
import androidx.compose.foundation.border
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.material3.*
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import com.rcdev.nbmbc.ui.theme.*

@Composable
fun GoldButton(
    text: String,
    onClick: () -> Unit,
    modifier: Modifier = Modifier,
    fullWidth: Boolean = false,
    leadingIcon: @Composable (() -> Unit)? = null
) {
    Button(
        onClick = onClick,
        modifier = modifier.then(if (fullWidth) Modifier.fillMaxWidth() else Modifier),
        colors = ButtonDefaults.buttonColors(containerColor = Color.Transparent),
        contentPadding = PaddingValues(0.dp),
        shape = CircleShape
    ) {
        Row(
            modifier = Modifier
                .clip(CircleShape)
                .background(GoldGradient)
                .padding(horizontal = if (fullWidth) 0.dp else 28.dp, vertical = 14.dp)
                .then(if (fullWidth) Modifier.fillMaxWidth() else Modifier),
            horizontalArrangement = Arrangement.Center,
            verticalAlignment = Alignment.CenterVertically
        ) {
            leadingIcon?.invoke()
            if (leadingIcon != null) Spacer(Modifier.width(8.dp))
            Text(
                text.uppercase(),
                color = AppColors.Background,
                fontWeight = FontWeight.Bold,
                fontSize = 13.sp,
                letterSpacing = 1.2.sp
            )
        }
    }
}

@Composable
fun GhostButton(
    text: String,
    onClick: () -> Unit,
    modifier: Modifier = Modifier,
    fullWidth: Boolean = false
) {
    OutlinedButton(
        onClick = onClick,
        modifier = modifier.then(if (fullWidth) Modifier.fillMaxWidth() else Modifier),
        shape = CircleShape,
        colors = ButtonDefaults.outlinedButtonColors(
            containerColor = Color.Transparent,
            contentColor = AppColors.GoldLight
        ),
        border = androidx.compose.foundation.BorderStroke(1.dp, AppColors.Gold.copy(alpha = 0.45f))
    ) {
        Text(
            text.uppercase(),
            fontWeight = FontWeight.SemiBold,
            fontSize = 13.sp,
            letterSpacing = 1.sp
        )
    }
}
