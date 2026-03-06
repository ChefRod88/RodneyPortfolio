package com.rcdev.nbmbc.ui.screens

import androidx.compose.foundation.*
import androidx.compose.foundation.layout.*
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
import com.rcdev.nbmbc.Screen
import com.rcdev.nbmbc.data.AppViewModel
import com.rcdev.nbmbc.data.Sermon
import com.rcdev.nbmbc.ui.components.*
import com.rcdev.nbmbc.ui.theme.*

@Composable
fun HomeScreen(vm: AppViewModel, navController: NavController, modifier: Modifier = Modifier) {
    val sermons by vm.sermons.collectAsStateWithLifecycle()
    val church = vm.church

    Column(modifier = modifier.fillMaxSize().verticalScroll(rememberScrollState())) {
        // Hero
        Box(
            modifier = Modifier
                .fillMaxWidth()
                .height(500.dp)
                .background(
                    Brush.verticalGradient(listOf(AppColors.Surface2, AppColors.Surface3, AppColors.Background))
                )
        ) {
            // Gold radial glow
            Box(modifier = Modifier.fillMaxSize().background(
                Brush.radialGradient(listOf(AppColors.Gold.copy(alpha = 0.1f), Color.Transparent))
            ))
            Column(
                modifier = Modifier.fillMaxSize().padding(24.dp),
                verticalArrangement = Arrangement.Center,
                horizontalAlignment = Alignment.CenterHorizontally
            ) {
                Spacer(Modifier.weight(1f))
                EyebrowBadge("Winter Haven, Florida")
                Spacer(Modifier.height(20.dp))
                Text(
                    "Rooted In Faith.\nCommitted To\nCommunity.",
                    style = MaterialTheme.typography.displaySmall,
                    fontWeight = FontWeight.Black,
                    textAlign = TextAlign.Center,
                    lineHeight = 44.sp,
                    brush = GoldTextGradient
                )
                Spacer(Modifier.height(14.dp))
                Text(
                    "Every Sunday we gather, worship, and grow\ntogether as one family in Christ.",
                    style = MaterialTheme.typography.bodyMedium,
                    color = AppColors.TextMuted,
                    textAlign = TextAlign.Center,
                    lineHeight = 22.sp
                )
                Spacer(Modifier.height(24.dp))
                Row(horizontalArrangement = Arrangement.spacedBy(12.dp)) {
                    GoldButton("Plan A Visit", { navController.navigate(Screen.About.route) })
                    GhostButton("Watch Live", { navController.navigate(Screen.Live.route) })
                }
                Spacer(Modifier.height(40.dp))
            }
        }

        // Stats Bar
        Row(
            modifier = Modifier
                .fillMaxWidth()
                .background(AppColors.Surface1)
                .drawBehind {
                    drawLine(Brush.horizontalGradient(listOf(Color.Transparent, AppColors.Gold.copy(0.25f), Color.Transparent)),
                        Offset(0f, 0f), Offset(size.width, 0f), 1f)
                    drawLine(Brush.horizontalGradient(listOf(Color.Transparent, AppColors.Gold.copy(0.25f), Color.Transparent)),
                        Offset(0f, size.height), Offset(size.width, size.height), 1f)
                }
                .padding(vertical = 20.dp),
            horizontalArrangement = Arrangement.SpaceEvenly
        ) {
            listOf("1965" to "Established", "${church.memberCount}" to "Members",
                "${church.servicesPerYear}" to "Services/Yr", "1" to "Community")
                .forEachIndexed { i, (val_, label) ->
                    Column(horizontalAlignment = Alignment.CenterHorizontally) {
                        Text(val_, style = MaterialTheme.typography.headlineMedium,
                            fontWeight = FontWeight.Black, brush = GoldTextGradient)
                        Text(label.uppercase(), fontSize = 9.sp, letterSpacing = 1.5.sp,
                            color = AppColors.TextMuted, fontWeight = FontWeight.SemiBold)
                    }
                }
        }

        // Mission Section
        Box(
            modifier = Modifier.fillMaxWidth()
                .background(Brush.linearGradient(listOf(AppColors.Background, AppColors.Surface2, AppColors.Background)))
                .padding(28.dp),
            contentAlignment = Alignment.Center
        ) {
            Column(horizontalAlignment = Alignment.CenterHorizontally, verticalArrangement = Arrangement.spacedBy(14.dp)) {
                EyebrowBadge("Our Purpose", showDot = false)
                Text(church.missionStatement, style = MaterialTheme.typography.headlineSmall,
                    fontWeight = FontWeight.Bold, textAlign = TextAlign.Center, color = Color.White, lineHeight = 32.sp)
                GoldDivider(Modifier.width(56.dp))
                Text(church.missionSubtext, style = MaterialTheme.typography.bodyMedium,
                    color = AppColors.TextMuted, textAlign = TextAlign.Center, lineHeight = 22.sp)
                GoldButton("What We Believe", { navController.navigate(Screen.About.route) })
            }
        }

        // Ministries
        Column(modifier = Modifier.fillMaxWidth().background(AppColors.Surface1).padding(20.dp),
            verticalArrangement = Arrangement.spacedBy(16.dp)) {
            SectionHeader("Get Involved", "Get Connected")
            Row(Modifier.horizontalScroll(rememberScrollState()), horizontalArrangement = Arrangement.spacedBy(12.dp)) {
                listOf(
                    Triple("Kids", "Ministry", "A vibrant place where children discover Jesus."),
                    Triple("Men's Ministry", "Ministry", "Men growing in faith and brotherhood."),
                    Triple("City Wide Mission", "Outreach", "Reaching Winter Haven with love and the Gospel.")
                ).forEach { (title, tag, desc) ->
                    MinistryCard(tag, title, desc) { navController.navigate(Screen.Groups.route) }
                }
            }
        }

        // Sermon Preview
        Column(modifier = Modifier.fillMaxWidth().background(AppColors.Background).padding(20.dp),
            verticalArrangement = Arrangement.spacedBy(14.dp)) {
            Row(Modifier.fillMaxWidth(), horizontalArrangement = Arrangement.SpaceBetween, verticalAlignment = Alignment.Top) {
                SectionHeader("Word of God", "Latest Sermons")
                GoldButton("All", { navController.navigate(Screen.Sermons.route) })
            }
            sermons.take(3).forEach { sermon ->
                SermonRowCard(sermon) { navController.navigate("sermon/${sermon.id}") }
            }
        }

        // Give Section
        Box(modifier = Modifier.fillMaxWidth()
            .background(Brush.linearGradient(listOf(AppColors.Surface2, AppColors.Background, AppColors.Surface1)))
            .padding(28.dp)) {
            Column(horizontalAlignment = Alignment.CenterHorizontally, verticalArrangement = Arrangement.spacedBy(16.dp)) {
                EyebrowBadge("Support The Ministry", showDot = false)
                Text("Give & Make An Impact", style = MaterialTheme.typography.headlineSmall,
                    fontWeight = FontWeight.Bold, color = Color.White, textAlign = TextAlign.Center)
                GoldDivider(Modifier.width(56.dp))
                Text("Your generosity powers the mission — feeding the hungry, reaching the streets of Winter Haven, and sharing the Gospel.",
                    color = AppColors.TextMuted, textAlign = TextAlign.Center, lineHeight = 22.sp)
                ScriptureCard(
                    "Each of you should give what you have decided in your heart to give, not reluctantly or under compulsion, for God loves a cheerful giver.",
                    "2 Corinthians 9:7"
                )
                GoldButton("Give via Cash App", { navController.navigate(Screen.Give.route) }, fullWidth = true)
            }
        }

        Spacer(Modifier.height(20.dp))
    }
}

@Composable
private fun MinistryCard(tag: String, title: String, desc: String, onClick: () -> Unit) {
    Column(
        modifier = Modifier
            .width(180.dp)
            .clip(RoundedCornerShape(16.dp))
            .background(AppColors.Surface1)
            .border(1.dp, AppColors.GoldBorder, RoundedCornerShape(16.dp))
            .clickable { onClick() }
            .padding(14.dp),
        verticalArrangement = Arrangement.spacedBy(6.dp)
    ) {
        Box(modifier = Modifier.fillMaxWidth().height(100.dp)
            .clip(RoundedCornerShape(12.dp))
            .background(Brush.linearGradient(listOf(AppColors.Surface2, AppColors.Surface3))),
            contentAlignment = Alignment.Center) {
            Text("✦", fontSize = 36.sp, color = AppColors.Gold.copy(alpha = 0.4f))
        }
        Text(tag.uppercase(), fontSize = 9.sp, letterSpacing = 2.sp, color = AppColors.Gold, fontWeight = FontWeight.SemiBold)
        Text(title, fontWeight = FontWeight.Bold, color = Color.White, fontSize = 15.sp)
        Text(desc, fontSize = 12.sp, color = AppColors.TextMuted, lineHeight = 16.sp, maxLines = 3)
        Text("Learn More →", fontSize = 11.sp, color = AppColors.Gold, fontWeight = FontWeight.SemiBold)
    }
}

@Composable
fun SermonRowCard(sermon: Sermon, onClick: () -> Unit) {
    Row(
        modifier = Modifier
            .fillMaxWidth()
            .clip(RoundedCornerShape(14.dp))
            .background(AppColors.Surface1)
            .border(1.dp, AppColors.GoldBorder, RoundedCornerShape(14.dp))
            .clickable { onClick() }
            .padding(14.dp),
        verticalAlignment = Alignment.CenterVertically,
        horizontalArrangement = Arrangement.spacedBy(12.dp)
    ) {
        Box(modifier = Modifier.size(80.dp, 60.dp).clip(RoundedCornerShape(10.dp))
            .background(Brush.linearGradient(listOf(AppColors.Surface2, AppColors.Surface3))),
            contentAlignment = Alignment.Center) {
            Text("▶", fontSize = 22.sp, color = AppColors.Gold.copy(alpha = 0.8f))
        }
        Column(modifier = Modifier.weight(1f), verticalArrangement = Arrangement.spacedBy(3.dp)) {
            sermon.series?.let { Text(it.uppercase(), fontSize = 9.sp, letterSpacing = 1.5.sp, color = AppColors.Gold) }
            Text(sermon.title, fontWeight = FontWeight.SemiBold, color = Color.White, fontSize = 14.sp, maxLines = 2)
            Text("${sermon.speaker}  •  ${sermon.formattedDate}", fontSize = 11.sp, color = AppColors.TextMuted)
        }
        Text("›", fontSize = 18.sp, color = AppColors.TextMuted)
    }
}
