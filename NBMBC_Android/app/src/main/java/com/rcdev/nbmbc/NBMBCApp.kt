package com.rcdev.nbmbc

import androidx.compose.foundation.background
import androidx.compose.foundation.layout.*
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.*
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.drawBehind
import androidx.compose.ui.geometry.Offset
import androidx.compose.ui.graphics.*
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.*
import androidx.lifecycle.viewmodel.compose.viewModel
import androidx.navigation.NavGraph.Companion.findStartDestination
import androidx.navigation.compose.*
import com.rcdev.nbmbc.data.AppViewModel
import com.rcdev.nbmbc.ui.components.ConstellationCanvas
import com.rcdev.nbmbc.ui.screens.*
import com.rcdev.nbmbc.ui.theme.*

sealed class Screen(val route: String) {
    data object Home    : Screen("home")
    data object Sermons : Screen("sermons")
    data object Events  : Screen("events")
    data object Prayer  : Screen("prayer")
    data object More    : Screen("more")

    // Detail routes
    data object SermonDetail : Screen("sermon/{id}")
    data object EventDetail  : Screen("event/{id}")
    data object Groups  : Screen("groups")
    data object Live    : Screen("live")
    data object About   : Screen("about")
    data object Location: Screen("location")
    data object Give    : Screen("give")
}

@Composable
fun NBMBCApp() {
    val navController = rememberNavController()
    val vm: AppViewModel = viewModel()

    val tabs = listOf(
        Triple(Screen.Home, Icons.Default.Home, "Home"),
        Triple(Screen.Sermons, Icons.Default.PlayArrow, "Sermons"),
        Triple(Screen.Events, Icons.Default.CalendarToday, "Events"),
        Triple(Screen.Prayer, Icons.Default.VolunteerActivism, "Prayer"),
        Triple(Screen.More, Icons.Default.Menu, "More"),
    )

    val navBackStackEntry by navController.currentBackStackEntryAsState()
    val currentRoute = navBackStackEntry?.destination?.route

    Scaffold(
        containerColor = AppColors.Background,
        bottomBar = {
            Box(
                modifier = Modifier
                    .background(AppColors.Surface1.copy(alpha = 0.92f))
                    .drawBehind {
                        drawLine(
                            brush = Brush.horizontalGradient(
                                listOf(Color.Transparent, AppColors.Gold.copy(alpha = 0.3f), Color.Transparent)
                            ),
                            start = Offset(0f, 0f), end = Offset(size.width, 0f), strokeWidth = 1.5f
                        )
                    }
                    .navigationBarsPadding()
            ) {
                NavigationBar(
                    containerColor = Color.Transparent,
                    tonalElevation = 0.dp
                ) {
                    tabs.forEach { (screen, icon, label) ->
                        val selected = currentRoute == screen.route
                        NavigationBarItem(
                            selected = selected,
                            onClick = {
                                navController.navigate(screen.route) {
                                    popUpTo(navController.graph.findStartDestination().id) { saveState = true }
                                    launchSingleTop = true
                                    restoreState = true
                                }
                            },
                            icon = {
                                Icon(icon, contentDescription = label,
                                    tint = if (selected) AppColors.Gold else AppColors.TextMuted.copy(alpha = 0.5f))
                            },
                            label = {
                                Text(label,
                                    color = if (selected) AppColors.Gold else AppColors.TextMuted.copy(alpha = 0.5f),
                                    fontSize = 10.sp, fontWeight = if (selected) FontWeight.Bold else FontWeight.Normal)
                            },
                            colors = NavigationBarItemDefaults.colors(
                                indicatorColor = AppColors.Gold.copy(alpha = 0.1f)
                            )
                        )
                    }
                }
            }
        }
    ) { padding ->
        Box(modifier = Modifier.fillMaxSize()) {
            // Global constellation background
            ConstellationCanvas(
                modifier = Modifier.fillMaxSize(),
                particleCount = 50
            )

            NavHost(
                navController = navController,
                startDestination = Screen.Home.route,
                modifier = Modifier.fillMaxSize()
            ) {
                composable(Screen.Home.route) {
                    HomeScreen(vm, navController, Modifier.padding(padding))
                }
                composable(Screen.Sermons.route) {
                    SermonsScreen(vm, navController, Modifier.padding(padding))
                }
                composable(Screen.Events.route) {
                    EventsScreen(vm, navController, Modifier.padding(padding))
                }
                composable(Screen.Prayer.route) {
                    PrayerScreen(Modifier.padding(padding))
                }
                composable(Screen.More.route) {
                    MoreScreen(vm, navController, Modifier.padding(padding))
                }
                composable(Screen.SermonDetail.route) { backStackEntry ->
                    val id = backStackEntry.arguments?.getString("id")?.toIntOrNull() ?: 0
                    val sermon = vm.sermons.collectAsState().value.find { it.id == id }
                    sermon?.let { SermonDetailScreen(it, navController) }
                }
                composable(Screen.EventDetail.route) { backStackEntry ->
                    val id = backStackEntry.arguments?.getString("id")?.toIntOrNull() ?: 0
                    val event = vm.events.collectAsState().value.find { it.id == id }
                    event?.let { EventDetailScreen(it, navController) }
                }
                composable(Screen.Groups.route)   { GroupsScreen(vm, navController) }
                composable(Screen.Live.route)     { LiveScreen(vm, navController) }
                composable(Screen.About.route)    { AboutScreen(vm, navController) }
                composable(Screen.Location.route) { LocationScreen(vm, navController) }
                composable(Screen.Give.route)     { GiveScreen(vm, navController) }
            }
        }
    }
}
