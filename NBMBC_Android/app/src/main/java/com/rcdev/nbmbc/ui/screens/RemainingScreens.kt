package com.rcdev.nbmbc.ui.screens

import android.content.Intent
import android.net.Uri
import androidx.compose.foundation.*
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.foundation.shape.*
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.*
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.graphics.*
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.unit.*
import androidx.lifecycle.compose.collectAsStateWithLifecycle
import androidx.navigation.NavController
import com.rcdev.nbmbc.data.AppViewModel
import com.rcdev.nbmbc.data.Event
import com.rcdev.nbmbc.data.Group
import com.rcdev.nbmbc.data.Sermon
import com.rcdev.nbmbc.ui.components.*
import com.rcdev.nbmbc.ui.theme.*

// ── Sermon Detail ─────────────────────────────────────────────────────────────
@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun SermonDetailScreen(sermon: Sermon, navController: NavController) {
    val context = LocalContext.current
    Scaffold(
        containerColor = AppColors.Background,
        topBar = {
            TopAppBar(
                title = { Text(sermon.title, color = Color.White, fontWeight = FontWeight.Bold, maxLines = 1) },
                navigationIcon = { IconButton({ navController.popBackStack() }) { Icon(Icons.Default.ArrowBack, null, tint = AppColors.Gold) } },
                colors = TopAppBarDefaults.topAppBarColors(containerColor = AppColors.Surface1)
            )
        }
    ) { padding ->
        Column(modifier = Modifier.fillMaxSize().padding(padding).verticalScroll(rememberScrollState())) {
            // Video placeholder
            Box(modifier = Modifier.fillMaxWidth().height(220.dp)
                .background(Brush.verticalGradient(listOf(AppColors.Surface2, AppColors.Surface3))),
                contentAlignment = Alignment.Center) {
                Column(horizontalAlignment = Alignment.CenterHorizontally, verticalArrangement = Arrangement.spacedBy(12.dp)) {
                    Text("▶", fontSize = 52.sp, color = AppColors.Gold.copy(0.8f))
                    if (sermon.videoUrl != null) {
                        GoldButton("Watch on YouTube", {
                            context.startActivity(Intent(Intent.ACTION_VIEW, Uri.parse(sermon.videoUrl)))
                        })
                    } else {
                        Text("Video Coming Soon", color = AppColors.TextMuted, fontSize = 15.sp)
                    }
                }
            }
            Column(modifier = Modifier.padding(24.dp), verticalArrangement = Arrangement.spacedBy(16.dp)) {
                sermon.series?.let { EyebrowBadge(it, showDot = false) }
                Text(sermon.title, style = MaterialTheme.typography.headlineSmall, fontWeight = FontWeight.Bold, color = Color.White)
                Row(horizontalArrangement = Arrangement.SpaceBetween, modifier = Modifier.fillMaxWidth()) {
                    Text("👤 ${sermon.speaker}", fontSize = 13.sp, color = AppColors.TextMuted)
                    Text("📅 ${sermon.formattedDate}", fontSize = 13.sp, color = AppColors.TextMuted)
                }
                GoldDivider()
                sermon.description?.let {
                    Text(it, style = MaterialTheme.typography.bodyLarge, color = AppColors.TextPrimary.copy(0.85f), lineHeight = 26.sp)
                }
                ScriptureCard("So faith comes from hearing, and hearing through the word of Christ.", "Romans 10:17")
                Spacer(Modifier.height(16.dp))
            }
        }
    }
}

// ── Event Detail ──────────────────────────────────────────────────────────────
@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun EventDetailScreen(event: Event, navController: NavController) {
    val context = LocalContext.current
    Scaffold(
        containerColor = AppColors.Background,
        topBar = {
            TopAppBar(
                title = { Text(event.title, color = Color.White, fontWeight = FontWeight.Bold, maxLines = 1) },
                navigationIcon = { IconButton({ navController.popBackStack() }) { Icon(Icons.Default.ArrowBack, null, tint = AppColors.Gold) } },
                colors = TopAppBarDefaults.topAppBarColors(containerColor = AppColors.Surface1)
            )
        }
    ) { padding ->
        Column(modifier = Modifier.fillMaxSize().padding(padding).verticalScroll(rememberScrollState())) {
            // Hero
            Box(modifier = Modifier.fillMaxWidth().height(200.dp)
                .background(Brush.verticalGradient(listOf(AppColors.Surface2, AppColors.Background))),
                contentAlignment = Alignment.Center) {
                Column(horizontalAlignment = Alignment.CenterHorizontally, verticalArrangement = Arrangement.spacedBy(12.dp)) {
                    Box(modifier = Modifier.size(80.dp).clip(CircleShape).background(GoldGradient), contentAlignment = Alignment.Center) {
                        Icon(Icons.Default.Event, null, tint = AppColors.Background, modifier = Modifier.size(38.dp))
                    }
                    Text(event.title, style = MaterialTheme.typography.titleLarge, fontWeight = FontWeight.Bold,
                        color = Color.White, textAlign = TextAlign.Center)
                }
            }
            Column(modifier = Modifier.padding(20.dp), verticalArrangement = Arrangement.spacedBy(14.dp)) {
                Row(horizontalArrangement = Arrangement.spacedBy(8.dp)) {
                    InfoChip("📅 ${event.formattedDate}")
                    InfoChip("🕐 ${event.formattedTime}")
                }
                event.location?.let { InfoChip("📍 $it") }
                event.capacity?.let { InfoChip("👥 $it spots") }
                GoldDivider()
                event.description?.let {
                    Text("ABOUT THIS EVENT", fontSize = 11.sp, letterSpacing = 2.sp, color = AppColors.Gold, fontWeight = FontWeight.SemiBold)
                    Text(it, style = MaterialTheme.typography.bodyLarge, color = AppColors.TextPrimary.copy(0.85f), lineHeight = 26.sp)
                }
                GoldButton("Add to Calendar", {}, fullWidth = true)
                event.location?.let {
                    GhostButton("Get Directions", {
                        val uri = Uri.parse("geo:0,0?q=${Uri.encode(it)}")
                        context.startActivity(Intent(Intent.ACTION_VIEW, uri))
                    }, fullWidth = true)
                }
                Spacer(Modifier.height(16.dp))
            }
        }
    }
}

@Composable
private fun InfoChip(text: String) {
    Box(modifier = Modifier.clip(CircleShape)
        .background(AppColors.Gold.copy(0.1f))
        .border(1.dp, AppColors.Gold.copy(0.3f), CircleShape)
        .padding(horizontal = 14.dp, vertical = 8.dp)) {
        Text(text, fontSize = 13.sp, color = AppColors.GoldLight, fontWeight = FontWeight.Medium)
    }
}

// ── Groups Screen ─────────────────────────────────────────────────────────────
@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun GroupsScreen(vm: AppViewModel, navController: NavController) {
    val groups by vm.groups.collectAsStateWithLifecycle()
    var selectedCat by remember { mutableStateOf<String?>(null) }
    val cats = groups.map { it.category }.distinct().sorted()
    val filtered = if (selectedCat == null) groups else groups.filter { it.category == selectedCat }

    Scaffold(
        containerColor = AppColors.Background,
        topBar = {
            TopAppBar(title = { Text("Groups", color = Color.White, fontWeight = FontWeight.Bold) },
                navigationIcon = { IconButton({ navController.popBackStack() }) { Icon(Icons.Default.ArrowBack, null, tint = AppColors.Gold) } },
                colors = TopAppBarDefaults.topAppBarColors(containerColor = AppColors.Surface1))
        }
    ) { padding ->
        LazyColumn(modifier = Modifier.fillMaxSize().padding(padding), contentPadding = PaddingValues(bottom = 20.dp)) {
            item {
                // Category filter
                Row(modifier = Modifier.fillMaxWidth().horizontalScroll(rememberScrollState()).padding(12.dp),
                    horizontalArrangement = Arrangement.spacedBy(8.dp)) {
                    FilterChip(selected = selectedCat == null, onClick = { selectedCat = null },
                        label = { Text("All") }, colors = chipColors(selectedCat == null))
                    cats.forEach { cat ->
                        FilterChip(selected = selectedCat == cat, onClick = { selectedCat = if (selectedCat == cat) null else cat },
                            label = { Text(cat) }, colors = chipColors(selectedCat == cat))
                    }
                }
            }
            items(filtered) { group -> GroupCard(group) }
        }
    }
}

@Composable
private fun chipColors(selected: Boolean) = FilterChipDefaults.filterChipColors(
    selectedContainerColor = AppColors.Gold.copy(0.25f), selectedLabelColor = Color.White,
    containerColor = AppColors.Gold.copy(0.06f), labelColor = AppColors.GoldLight
)

@Composable
private fun GroupCard(group: Group) {
    Row(
        modifier = Modifier.fillMaxWidth().padding(horizontal = 16.dp, vertical = 6.dp)
            .clip(RoundedCornerShape(18.dp)).background(AppColors.Surface1)
            .border(1.dp, AppColors.GoldBorder, RoundedCornerShape(18.dp)).padding(16.dp),
        verticalAlignment = Alignment.Top, horizontalArrangement = Arrangement.spacedBy(14.dp)
    ) {
        Box(modifier = Modifier.size(64.dp).clip(RoundedCornerShape(14.dp))
            .background(Brush.linearGradient(listOf(AppColors.Surface2, AppColors.Surface3)))
            .border(1.dp, AppColors.GoldBorder, RoundedCornerShape(14.dp)),
            contentAlignment = Alignment.Center) {
            Text("✦", fontSize = 26.sp, color = AppColors.Gold)
        }
        Column(modifier = Modifier.weight(1f), verticalArrangement = Arrangement.spacedBy(4.dp)) {
            Text(group.category.uppercase(), fontSize = 9.sp, letterSpacing = 2.sp, color = AppColors.Gold, fontWeight = FontWeight.SemiBold)
            Text(group.title, fontWeight = FontWeight.Bold, color = Color.White, fontSize = 16.sp)
            Text(group.description, fontSize = 13.sp, color = AppColors.TextMuted, lineHeight = 18.sp, maxLines = 3)
            group.meetingTime?.let { Text("🕐 $it", fontSize = 12.sp, color = AppColors.GoldLight.copy(0.7f)) }
        }
    }
}

// ── Live Screen ───────────────────────────────────────────────────────────────
@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun LiveScreen(vm: AppViewModel, navController: NavController) {
    val context = LocalContext.current
    val church = vm.church
    Scaffold(
        containerColor = AppColors.Background,
        topBar = {
            TopAppBar(title = { Text("Watch Live", color = Color.White, fontWeight = FontWeight.Bold) },
                navigationIcon = { IconButton({ navController.popBackStack() }) { Icon(Icons.Default.ArrowBack, null, tint = AppColors.Gold) } },
                colors = TopAppBarDefaults.topAppBarColors(containerColor = AppColors.Surface1))
        }
    ) { padding ->
        Column(modifier = Modifier.fillMaxSize().padding(padding).verticalScroll(rememberScrollState())) {
            Box(modifier = Modifier.fillMaxWidth().height(260.dp).background(Color.Black),
                contentAlignment = Alignment.Center) {
                Column(horizontalAlignment = Alignment.CenterHorizontally, verticalArrangement = Arrangement.spacedBy(16.dp)) {
                    Text("▶", fontSize = 60.sp, color = Color(0xFFE53935).copy(0.9f))
                    Text("Live Stream", color = Color.White, fontWeight = FontWeight.Bold, fontSize = 18.sp)
                    Button(onClick = { context.startActivity(Intent(Intent.ACTION_VIEW, Uri.parse(church.facebookUrl))) },
                        colors = ButtonDefaults.buttonColors(containerColor = Color(0xFF1877F2))) {
                        Text("Open Facebook Live", color = Color.White, fontWeight = FontWeight.Bold)
                    }
                }
            }
            Column(modifier = Modifier.padding(20.dp), verticalArrangement = Arrangement.spacedBy(16.dp)) {
                GlassCard(modifier = Modifier.fillMaxWidth()) {
                    Column(modifier = Modifier.padding(18.dp), verticalArrangement = Arrangement.spacedBy(10.dp)) {
                        Text("SERVICE TIMES", fontSize = 11.sp, letterSpacing = 2.sp, color = AppColors.Gold, fontWeight = FontWeight.SemiBold)
                        listOf("Sunday School" to "9:30 AM", "Morning Worship" to "10:30 AM", "Bible Study" to "Wednesday 7 PM").forEach { (name, time) ->
                            Row(modifier = Modifier.fillMaxWidth(), horizontalArrangement = Arrangement.SpaceBetween) {
                                Text(name, color = Color.White, fontSize = 15.sp)
                                Text(time, color = AppColors.GoldLight, fontWeight = FontWeight.Bold, fontSize = 14.sp)
                            }
                            if (name != "Bible Study") HorizontalDivider(color = AppColors.GoldBorder, thickness = 0.5.dp)
                        }
                    }
                }
                GlassCard(modifier = Modifier.fillMaxWidth()) {
                    Column(modifier = Modifier.padding(18.dp), verticalArrangement = Arrangement.spacedBy(12.dp)) {
                        Text("CONNECT WITH US", fontSize = 11.sp, letterSpacing = 2.sp, color = AppColors.Gold, fontWeight = FontWeight.SemiBold)
                        Row(modifier = Modifier.fillMaxWidth().clickable {
                            context.startActivity(Intent(Intent.ACTION_VIEW, Uri.parse(church.youtubeUrl)))
                        }, verticalAlignment = Alignment.CenterVertically, horizontalArrangement = Arrangement.spacedBy(12.dp)) {
                            Icon(Icons.Default.PlayArrow, null, tint = Color(0xFFFF0000), modifier = Modifier.size(24.dp))
                            Text("Watch on YouTube", color = Color.White, fontSize = 15.sp, modifier = Modifier.weight(1f))
                            Icon(Icons.Default.OpenInNew, null, tint = AppColors.TextMuted, modifier = Modifier.size(16.dp))
                        }
                        HorizontalDivider(color = AppColors.GoldBorder, thickness = 0.5.dp)
                        Row(modifier = Modifier.fillMaxWidth().clickable {
                            context.startActivity(Intent(Intent.ACTION_VIEW, Uri.parse(church.facebookUrl)))
                        }, verticalAlignment = Alignment.CenterVertically, horizontalArrangement = Arrangement.spacedBy(12.dp)) {
                            Icon(Icons.Default.Facebook, null, tint = Color(0xFF1877F2), modifier = Modifier.size(24.dp))
                            Text("Watch on Facebook", color = Color.White, fontSize = 15.sp, modifier = Modifier.weight(1f))
                            Icon(Icons.Default.OpenInNew, null, tint = AppColors.TextMuted, modifier = Modifier.size(16.dp))
                        }
                    }
                }
            }
        }
    }
}

// ── About Screen ──────────────────────────────────────────────────────────────
@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun AboutScreen(vm: AppViewModel, navController: NavController) {
    val context = LocalContext.current
    val church = vm.church
    Scaffold(
        containerColor = AppColors.Background,
        topBar = {
            TopAppBar(title = { Text("About", color = Color.White, fontWeight = FontWeight.Bold) },
                navigationIcon = { IconButton({ navController.popBackStack() }) { Icon(Icons.Default.ArrowBack, null, tint = AppColors.Gold) } },
                colors = TopAppBarDefaults.topAppBarColors(containerColor = AppColors.Surface1))
        }
    ) { padding ->
        Column(modifier = Modifier.fillMaxSize().padding(padding).verticalScroll(rememberScrollState())) {
            Box(modifier = Modifier.fillMaxWidth()
                .background(Brush.verticalGradient(listOf(AppColors.Surface2, AppColors.Surface3, AppColors.Background)))
                .padding(32.dp), contentAlignment = Alignment.Center) {
                Column(horizontalAlignment = Alignment.CenterHorizontally, verticalArrangement = Arrangement.spacedBy(14.dp)) {
                    Box(modifier = Modifier.size(88.dp).clip(CircleShape).background(GoldGradient), contentAlignment = Alignment.Center) {
                        Icon(Icons.Default.Church, null, tint = AppColors.Background, modifier = Modifier.size(42.dp))
                    }
                    Text(church.name, style = MaterialTheme.typography.titleLarge, fontWeight = FontWeight.Black,
                        brush = GoldTextGradient, textAlign = TextAlign.Center)
                    Text("Est. ${church.established}  •  ${church.location}".uppercase(), fontSize = 11.sp,
                        letterSpacing = 2.sp, color = AppColors.TextMuted)
                }
            }
            Column(modifier = Modifier.padding(20.dp), verticalArrangement = Arrangement.spacedBy(20.dp)) {
                Column(horizontalAlignment = Alignment.CenterHorizontally, verticalArrangement = Arrangement.spacedBy(12.dp),
                    modifier = Modifier.fillMaxWidth().clip(RoundedCornerShape(20.dp))
                        .background(Brush.linearGradient(listOf(AppColors.Background, AppColors.Surface2, AppColors.Background)))
                        .padding(24.dp)) {
                    EyebrowBadge("Our Mission", showDot = false)
                    Text(church.missionStatement, style = MaterialTheme.typography.titleMedium, fontWeight = FontWeight.Bold,
                        color = Color.White, textAlign = TextAlign.Center, lineHeight = 28.sp)
                    GoldDivider(Modifier.width(56.dp))
                    Text(church.missionSubtext, color = AppColors.TextMuted, textAlign = TextAlign.Center, lineHeight = 22.sp)
                }
                SectionHeader("What We Believe", "Core Beliefs")
                church.beliefs.forEach { belief ->
                    Row(verticalAlignment = Alignment.Top, horizontalArrangement = Arrangement.spacedBy(12.dp)) {
                        Box(modifier = Modifier.size(44.dp).clip(CircleShape).background(AppColors.Gold.copy(0.12f)),
                            contentAlignment = Alignment.Center) {
                            Text("✦", fontSize = 18.sp, color = AppColors.Gold)
                        }
                        Column(verticalArrangement = Arrangement.spacedBy(3.dp)) {
                            Text(belief.title, fontWeight = FontWeight.Bold, color = Color.White, fontSize = 15.sp)
                            Text(belief.description, fontSize = 13.sp, color = AppColors.TextMuted, lineHeight = 18.sp)
                        }
                    }
                }
                SectionHeader("Get In Touch", "Contact Us")
                listOf(
                    Triple("📍", church.address, "geo:0,0?q=${Uri.encode(church.address)}"),
                    Triple("📞", church.phone, "tel:${church.phone.filter { it.isDigit() }}"),
                    Triple("✉️", church.email, "mailto:${church.email}")
                ).forEach { (emoji, text, url) ->
                    Row(modifier = Modifier.fillMaxWidth().clip(RoundedCornerShape(14.dp))
                        .background(AppColors.Surface1).border(1.dp, AppColors.GoldBorder, RoundedCornerShape(14.dp))
                        .clickable { context.startActivity(Intent(Intent.ACTION_VIEW, Uri.parse(url))) }.padding(14.dp),
                        verticalAlignment = Alignment.CenterVertically, horizontalArrangement = Arrangement.spacedBy(12.dp)) {
                        Text(emoji, fontSize = 20.sp)
                        Text(text, color = AppColors.TextPrimary, fontSize = 14.sp, modifier = Modifier.weight(1f))
                        Icon(Icons.Default.OpenInNew, null, tint = AppColors.TextMuted, modifier = Modifier.size(16.dp))
                    }
                }
                Spacer(Modifier.height(8.dp))
            }
        }
    }
}

// ── Location Screen ───────────────────────────────────────────────────────────
@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun LocationScreen(vm: AppViewModel, navController: NavController) {
    val context = LocalContext.current
    val church = vm.church
    Scaffold(
        containerColor = AppColors.Background,
        topBar = {
            TopAppBar(title = { Text("Location", color = Color.White, fontWeight = FontWeight.Bold) },
                navigationIcon = { IconButton({ navController.popBackStack() }) { Icon(Icons.Default.ArrowBack, null, tint = AppColors.Gold) } },
                colors = TopAppBarDefaults.topAppBarColors(containerColor = AppColors.Surface1))
        }
    ) { padding ->
        Column(modifier = Modifier.fillMaxSize().padding(padding).verticalScroll(rememberScrollState())) {
            // Map placeholder (full Maps integration requires API key)
            Box(modifier = Modifier.fillMaxWidth().height(280.dp)
                .background(Brush.linearGradient(listOf(AppColors.Surface2, AppColors.Surface3))),
                contentAlignment = Alignment.Center) {
                Column(horizontalAlignment = Alignment.CenterHorizontally, verticalArrangement = Arrangement.spacedBy(12.dp)) {
                    Text("📍", fontSize = 52.sp)
                    Text("New Bethel MBBC", color = Color.White, fontWeight = FontWeight.Bold, fontSize = 16.sp)
                    Text("Winter Haven, Florida", color = AppColors.TextMuted, fontSize = 13.sp)
                    GoldButton("Open in Google Maps", {
                        val uri = Uri.parse("geo:28.0222,-81.7329?q=${Uri.encode(church.address)}")
                        context.startActivity(Intent(Intent.ACTION_VIEW, uri))
                    })
                }
            }
            Column(modifier = Modifier.padding(20.dp), verticalArrangement = Arrangement.spacedBy(16.dp)) {
                GlassCard(modifier = Modifier.fillMaxWidth()) {
                    Row(modifier = Modifier.padding(18.dp), verticalAlignment = Alignment.CenterVertically,
                        horizontalArrangement = Arrangement.spacedBy(14.dp)) {
                        Box(modifier = Modifier.size(48.dp).clip(CircleShape).background(GoldGradient),
                            contentAlignment = Alignment.Center) {
                            Icon(Icons.Default.LocationOn, null, tint = AppColors.Background, modifier = Modifier.size(24.dp))
                        }
                        Column {
                            Text("OUR ADDRESS", fontSize = 10.sp, letterSpacing = 2.sp, color = AppColors.Gold, fontWeight = FontWeight.SemiBold)
                            Text(church.address, fontWeight = FontWeight.Bold, color = Color.White, fontSize = 15.sp)
                        }
                    }
                }
                GlassCard(modifier = Modifier.fillMaxWidth()) {
                    Column(modifier = Modifier.padding(18.dp), verticalArrangement = Arrangement.spacedBy(10.dp)) {
                        Text("SERVICE TIMES", fontSize = 11.sp, letterSpacing = 2.sp, color = AppColors.Gold, fontWeight = FontWeight.SemiBold)
                        listOf("Sunday School" to "9:30 AM", "Morning Worship" to "10:30 AM", "Bible Study" to "Wednesday 7 PM").forEach { (n, t) ->
                            Row(Modifier.fillMaxWidth(), horizontalArrangement = Arrangement.SpaceBetween) {
                                Text(n, color = Color.White); Text(t, color = AppColors.GoldLight, fontWeight = FontWeight.Bold)
                            }
                        }
                    }
                }
                GoldButton("Get Directions", {
                    val uri = Uri.parse("google.navigation:q=${Uri.encode(church.address)}")
                    val intent = Intent(Intent.ACTION_VIEW, uri).apply { setPackage("com.google.android.apps.maps") }
                    if (intent.resolveActivity(context.packageManager) != null) context.startActivity(intent)
                    else context.startActivity(Intent(Intent.ACTION_VIEW, Uri.parse("geo:0,0?q=${Uri.encode(church.address)}")))
                }, fullWidth = true)
                GhostButton("Call ${church.phone}", {
                    context.startActivity(Intent(Intent.ACTION_DIAL, Uri.parse("tel:${church.phone.filter { it.isDigit() }}")))
                }, fullWidth = true)
                Spacer(Modifier.height(16.dp))
            }
        }
    }
}
