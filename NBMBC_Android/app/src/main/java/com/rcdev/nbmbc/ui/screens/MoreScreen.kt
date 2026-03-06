package com.rcdev.nbmbc.ui.screens

import androidx.compose.foundation.*
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.shape.*
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.*
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.*
import androidx.compose.ui.graphics.*
import androidx.compose.ui.graphics.vector.ImageVector
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.unit.*
import androidx.navigation.NavController
import com.rcdev.nbmbc.Screen
import com.rcdev.nbmbc.data.AppViewModel
import com.rcdev.nbmbc.ui.components.*
import com.rcdev.nbmbc.ui.theme.*

@Composable
fun MoreScreen(vm: AppViewModel, navController: NavController, modifier: Modifier = Modifier) {
    val church = vm.church
    Column(modifier = modifier.fillMaxSize().verticalScroll(rememberScrollState())) {
        // Header
        Column(modifier = Modifier.fillMaxWidth().padding(28.dp),
            horizontalAlignment = Alignment.CenterHorizontally, verticalArrangement = Arrangement.spacedBy(10.dp)) {
            Box(modifier = Modifier.size(80.dp).clip(CircleShape).background(GoldGradient),
                contentAlignment = Alignment.Center) {
                Icon(Icons.Default.Church, null, tint = AppColors.Background, modifier = Modifier.size(38.dp))
            }
            Text(church.shortName, style = MaterialTheme.typography.titleLarge,
                fontWeight = FontWeight.Black, brush = GoldTextGradient)
            Text(church.location.uppercase(), fontSize = 11.sp, letterSpacing = 2.sp, color = AppColors.TextMuted)
        }

        // Media Group
        MenuGroup("Media") {
            MenuItem(Icons.Default.LiveTv, AppColors.LiveRed, "Watch Live", "Join our live stream") {
                navController.navigate(Screen.Live.route)
            }
            HorizontalDivider(color = AppColors.GoldBorder, thickness = 0.5.dp, modifier = Modifier.padding(start = 70.dp))
            MenuItem(Icons.Default.LibraryBooks, AppColors.Gold, "Sermons", "Browse the library") {
                navController.navigate(Screen.Sermons.route)
            }
        }

        Spacer(Modifier.height(8.dp))

        // Connect Group
        MenuGroup("Connect") {
            MenuItem(Icons.Default.Group, Color(0xFF3D8EFF), "Groups", "Find your community") {
                navController.navigate(Screen.Groups.route)
            }
            HorizontalDivider(color = AppColors.GoldBorder, thickness = 0.5.dp, modifier = Modifier.padding(start = 70.dp))
            MenuItem(Icons.Default.Event, Color(0xFF34C759), "Events", "What's happening") {
                navController.navigate(Screen.Events.route)
            }
            HorizontalDivider(color = AppColors.GoldBorder, thickness = 0.5.dp, modifier = Modifier.padding(start = 70.dp))
            MenuItem(Icons.Default.VolunteerActivism, Color(0xFFAF52DE), "Prayer", "Request prayer") {
                navController.navigate(Screen.Prayer.route)
            }
        }

        Spacer(Modifier.height(8.dp))

        // About Group
        MenuGroup("About") {
            MenuItem(Icons.Default.Info, AppColors.Gold, "Who We Are", "Our story & beliefs") {
                navController.navigate(Screen.About.route)
            }
            HorizontalDivider(color = AppColors.GoldBorder, thickness = 0.5.dp, modifier = Modifier.padding(start = 70.dp))
            MenuItem(Icons.Default.LocationOn, Color(0xFFFF6B35), "Location", "Find us in Winter Haven") {
                navController.navigate(Screen.Location.route)
            }
        }

        Spacer(Modifier.height(8.dp))

        // Give Card
        Row(
            modifier = Modifier
                .fillMaxWidth()
                .padding(horizontal = 16.dp)
                .clip(RoundedCornerShape(18.dp))
                .background(AppColors.Surface1)
                .border(1.dp, AppColors.CashGreen.copy(0.3f), RoundedCornerShape(18.dp))
                .clickable { navController.navigate(Screen.Give.route) }
                .padding(16.dp),
            verticalAlignment = Alignment.CenterVertically,
            horizontalArrangement = Arrangement.spacedBy(14.dp)
        ) {
            Box(modifier = Modifier.size(52.dp).clip(RoundedCornerShape(14.dp))
                .background(Brush.linearGradient(listOf(AppColors.CashGreen, AppColors.CashGreenDark))),
                contentAlignment = Alignment.Center) {
                Text("$", fontSize = 24.sp, fontWeight = FontWeight.Black, color = Color.White)
            }
            Column(modifier = Modifier.weight(1f)) {
                Text("Give Online", fontWeight = FontWeight.Bold, color = Color.White, fontSize = 16.sp)
                Text("Support the ministry via Cash App", fontSize = 12.sp, color = AppColors.TextMuted)
            }
            Icon(Icons.Default.Favorite, null, tint = AppColors.CashGreen, modifier = Modifier.size(20.dp))
        }

        // Footer
        Column(modifier = Modifier.fillMaxWidth().padding(24.dp),
            horizontalAlignment = Alignment.CenterHorizontally, verticalArrangement = Arrangement.spacedBy(4.dp)) {
            Text("RC DEV", fontSize = 12.sp, letterSpacing = 3.sp, color = AppColors.Gold.copy(0.4f), fontWeight = FontWeight.Bold)
            Text("Version 1.0.0  •  New Bethel MBBC", fontSize = 10.sp, color = AppColors.TextMuted.copy(0.5f))
        }
        Spacer(Modifier.height(8.dp))
    }
}

@Composable
private fun MenuGroup(title: String, content: @Composable ColumnScope.() -> Unit) {
    Column(modifier = Modifier.fillMaxWidth().padding(horizontal = 16.dp)) {
        Text(title.uppercase(), fontSize = 10.sp, letterSpacing = 2.sp,
            color = AppColors.TextMuted, fontWeight = FontWeight.SemiBold,
            modifier = Modifier.padding(bottom = 6.dp, start = 4.dp))
        Column(
            modifier = Modifier
                .fillMaxWidth()
                .clip(RoundedCornerShape(18.dp))
                .background(AppColors.Surface1)
                .border(1.dp, AppColors.GoldBorder, RoundedCornerShape(18.dp))
        ) { content() }
    }
}

@Composable
private fun MenuItem(icon: ImageVector, color: Color, label: String, subtitle: String, onClick: () -> Unit) {
    Row(
        modifier = Modifier
            .fillMaxWidth()
            .clickable { onClick() }
            .padding(horizontal = 16.dp, vertical = 14.dp),
        verticalAlignment = Alignment.CenterVertically,
        horizontalArrangement = Arrangement.spacedBy(14.dp)
    ) {
        Box(modifier = Modifier.size(42.dp).clip(RoundedCornerShape(10.dp)).background(color.copy(0.18f)),
            contentAlignment = Alignment.Center) {
            Icon(icon, null, tint = color, modifier = Modifier.size(22.dp))
        }
        Column(modifier = Modifier.weight(1f)) {
            Text(label, fontWeight = FontWeight.SemiBold, color = Color.White, fontSize = 16.sp)
            Text(subtitle, fontSize = 12.sp, color = AppColors.TextMuted)
        }
        Icon(Icons.Default.ChevronRight, null, tint = AppColors.TextMuted, modifier = Modifier.size(18.dp))
    }
}
