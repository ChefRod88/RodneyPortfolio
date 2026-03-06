package com.rcdev.nbmbc.ui.screens

import androidx.compose.foundation.*
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.grid.*
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.*
import androidx.compose.ui.graphics.*
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.unit.*
import androidx.lifecycle.compose.collectAsStateWithLifecycle
import androidx.navigation.NavController
import com.rcdev.nbmbc.Screen
import com.rcdev.nbmbc.data.AppViewModel
import com.rcdev.nbmbc.data.Sermon
import com.rcdev.nbmbc.ui.components.*
import com.rcdev.nbmbc.ui.theme.*

@Composable
fun SermonsScreen(vm: AppViewModel, navController: NavController, modifier: Modifier = Modifier) {
    val sermons by vm.sermons.collectAsStateWithLifecycle()

    LazyVerticalGrid(
        columns = GridCells.Fixed(2),
        modifier = modifier.fillMaxSize(),
        contentPadding = PaddingValues(bottom = 20.dp)
    ) {
        item(span = { GridItemSpan(2) }) {
            Box(modifier = Modifier.fillMaxWidth()
                .background(Brush.verticalGradient(listOf(AppColors.Surface2, AppColors.Background)))
                .padding(32.dp)) {
                Column(horizontalAlignment = Alignment.CenterHorizontally, verticalArrangement = Arrangement.spacedBy(16.dp)) {
                    EyebrowBadge("Word of God")
                    Text("Sermon Library", style = MaterialTheme.typography.displaySmall,
                        fontWeight = FontWeight.Black, brush = GoldTextGradient, textAlign = TextAlign.Center)
                    Text("Watch our latest teachings and grow in the Word.", color = AppColors.TextMuted, textAlign = TextAlign.Center)
                    GhostButton("Watch Live Stream") { navController.navigate(Screen.Live.route) }
                }
            }
        }

        if (sermons.isEmpty()) {
            item(span = { GridItemSpan(2) }) {
                EmptyStateView("🎥", "No Sermons Yet", "Check back soon — messages are on the way.")
            }
        }

        items(sermons) { sermon ->
            SermonGridCard(sermon) { navController.navigate("sermon/${sermon.id}") }
        }
    }
}

@Composable
fun SermonGridCard(sermon: Sermon, onClick: () -> Unit) {
    Column(
        modifier = Modifier
            .padding(8.dp)
            .clip(RoundedCornerShape(16.dp))
            .background(AppColors.Surface1)
            .border(1.dp, AppColors.GoldBorder, RoundedCornerShape(16.dp))
            .clickable { onClick() }
            .drawBehind {
                drawLine(Brush.horizontalGradient(listOf(Color.Transparent, AppColors.Gold.copy(0.2f), Color.Transparent)),
                    Offset(0f, 0f), Offset(size.width, 0f), 2f)
            }
    ) {
        Box(modifier = Modifier.fillMaxWidth().height(110.dp)
            .background(Brush.linearGradient(listOf(AppColors.Surface2, AppColors.Surface3))),
            contentAlignment = Alignment.Center) {
            Column(horizontalAlignment = Alignment.CenterHorizontally) {
                Text("▶", fontSize = 32.sp, color = AppColors.Gold.copy(0.8f))
                sermon.series?.let {
                    Text(it.uppercase(), fontSize = 8.sp, letterSpacing = 1.5.sp, color = AppColors.Gold.copy(0.6f))
                }
            }
        }
        Column(modifier = Modifier.padding(12.dp), verticalArrangement = Arrangement.spacedBy(4.dp)) {
            sermon.series?.let { Text(it.uppercase(), fontSize = 9.sp, letterSpacing = 2.sp, color = AppColors.Gold) }
            Text(sermon.title, fontWeight = FontWeight.SemiBold, color = Color.White, fontSize = 13.sp, maxLines = 2, lineHeight = 18.sp)
            Text(sermon.formattedDate, fontSize = 11.sp, color = AppColors.TextMuted)
            Text("▶ Watch Now", fontSize = 11.sp, color = AppColors.GoldLight, fontWeight = FontWeight.SemiBold)
        }
    }
}
