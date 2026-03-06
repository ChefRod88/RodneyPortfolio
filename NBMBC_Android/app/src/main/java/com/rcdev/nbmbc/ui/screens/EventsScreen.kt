package com.rcdev.nbmbc.ui.screens

import androidx.compose.foundation.*
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.foundation.shape.*
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
import com.rcdev.nbmbc.data.AppViewModel
import com.rcdev.nbmbc.data.Event
import com.rcdev.nbmbc.ui.components.*
import com.rcdev.nbmbc.ui.theme.*

@Composable
fun EventsScreen(vm: AppViewModel, navController: NavController, modifier: Modifier = Modifier) {
    val events by vm.events.collectAsStateWithLifecycle()

    LazyColumn(modifier = modifier.fillMaxSize(), contentPadding = PaddingValues(bottom = 20.dp)) {
        item {
            Box(modifier = Modifier.fillMaxWidth()
                .background(Brush.verticalGradient(listOf(AppColors.Surface2, AppColors.Background)))
                .padding(32.dp)) {
                Column(horizontalAlignment = Alignment.CenterHorizontally, verticalArrangement = Arrangement.spacedBy(14.dp)) {
                    EyebrowBadge("N.B.M.B.C.")
                    Text("Upcoming Events", style = MaterialTheme.typography.displaySmall,
                        fontWeight = FontWeight.Black, brush = GoldTextGradient, textAlign = TextAlign.Center)
                    Text("Find events to attend and get connected with our community in Winter Haven, Florida.",
                        color = AppColors.TextMuted, textAlign = TextAlign.Center, lineHeight = 22.sp)
                }
            }
        }

        if (events.isEmpty()) {
            item { EmptyStateView("📅", "No Events Scheduled", "Check back soon for upcoming events and gatherings.") }
        }

        items(events) { event ->
            EventCard(event) { navController.navigate("event/${event.id}") }
        }
    }
}

@Composable
fun EventCard(event: Event, onClick: () -> Unit) {
    Row(
        modifier = Modifier
            .fillMaxWidth()
            .padding(horizontal = 16.dp, vertical = 6.dp)
            .clip(RoundedCornerShape(18.dp))
            .background(AppColors.Surface1)
            .border(1.dp, AppColors.GoldBorder, RoundedCornerShape(18.dp))
            .clickable { onClick() }
            .drawBehind {
                drawLine(Brush.horizontalGradient(listOf(Color.Transparent, AppColors.Gold.copy(0.15f), Color.Transparent)),
                    Offset(0f, 0f), Offset(size.width, 0f), 2f)
            }
            .padding(14.dp),
        verticalAlignment = Alignment.CenterVertically,
        horizontalArrangement = Arrangement.spacedBy(12.dp)
    ) {
        // Date Badge
        Column(
            modifier = Modifier
                .width(52.dp)
                .clip(RoundedCornerShape(10.dp))
                .background(AppColors.Surface2)
                .border(1.dp, AppColors.GoldBorder, RoundedCornerShape(10.dp))
                .padding(vertical = 10.dp),
            horizontalAlignment = Alignment.CenterHorizontally
        ) {
            Text(event.monthAbbr, fontSize = 9.sp, fontWeight = FontWeight.Bold, letterSpacing = 1.sp, color = AppColors.Gold)
            Text(event.dayNumber, fontSize = 24.sp, fontWeight = FontWeight.Black, color = Color.White)
        }
        // Info
        Column(modifier = Modifier.weight(1f), verticalArrangement = Arrangement.spacedBy(4.dp)) {
            Text(event.title, fontWeight = FontWeight.Bold, color = Color.White, fontSize = 15.sp, maxLines = 2)
            Text("🕐 ${event.formattedTime}", fontSize = 12.sp, color = AppColors.TextMuted)
            event.location?.let { Text("📍 $it", fontSize = 12.sp, color = AppColors.TextMuted) }
            event.capacity?.let { Text("👥 $it spots available", fontSize = 11.sp, color = AppColors.Gold.copy(0.7f)) }
        }
        Text("›", fontSize = 20.sp, color = AppColors.TextMuted)
    }
}
