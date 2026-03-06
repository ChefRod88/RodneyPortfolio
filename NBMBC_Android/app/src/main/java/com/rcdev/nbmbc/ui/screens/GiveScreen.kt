package com.rcdev.nbmbc.ui.screens

import android.content.Intent
import android.net.Uri
import androidx.compose.foundation.*
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.shape.*
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.ArrowBack
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
import androidx.navigation.NavController
import com.rcdev.nbmbc.data.AppViewModel
import com.rcdev.nbmbc.ui.components.*
import com.rcdev.nbmbc.ui.theme.*

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun GiveScreen(vm: AppViewModel, navController: NavController) {
    val context = LocalContext.current
    val church = vm.church
    var selectedAmount by remember { mutableIntStateOf(25) }
    val amounts = listOf(10, 25, 50, 100)

    Scaffold(
        containerColor = AppColors.Background,
        topBar = {
            TopAppBar(
                title = { Text("Give", color = Color.White, fontWeight = FontWeight.Bold) },
                navigationIcon = {
                    IconButton({ navController.popBackStack() }) {
                        Icon(Icons.Default.ArrowBack, null, tint = AppColors.Gold)
                    }
                },
                colors = TopAppBarDefaults.topAppBarColors(containerColor = AppColors.Surface1)
            )
        }
    ) { padding ->
        Column(modifier = Modifier.fillMaxSize().padding(padding).verticalScroll(rememberScrollState()).padding(20.dp),
            verticalArrangement = Arrangement.spacedBy(20.dp)) {

            // Header
            Column(horizontalAlignment = Alignment.CenterHorizontally, verticalArrangement = Arrangement.spacedBy(12.dp),
                modifier = Modifier.fillMaxWidth()) {
                Box(modifier = Modifier.size(72.dp).clip(CircleShape).background(GoldGradient),
                    contentAlignment = Alignment.Center) {
                    Text("❤️", fontSize = 30.sp)
                }
                Text("Support The Ministry", style = MaterialTheme.typography.headlineSmall,
                    fontWeight = FontWeight.Black, brush = GoldTextGradient, textAlign = TextAlign.Center)
                Text("Your generosity fuels the Gospel mission at New Bethel — feeding the hungry, reaching the streets, and sharing the love of Christ.",
                    color = AppColors.TextMuted, textAlign = TextAlign.Center, lineHeight = 22.sp)
            }

            // Amount selector
            GlassCard(modifier = Modifier.fillMaxWidth()) {
                Column(modifier = Modifier.padding(20.dp), verticalArrangement = Arrangement.spacedBy(12.dp)) {
                    Text("SELECT AN AMOUNT", fontSize = 11.sp, letterSpacing = 2.sp,
                        color = AppColors.Gold, fontWeight = FontWeight.SemiBold)
                    Row(horizontalArrangement = Arrangement.spacedBy(8.dp)) {
                        amounts.forEach { amount ->
                            val sel = selectedAmount == amount
                            Button(
                                onClick = { selectedAmount = amount },
                                modifier = Modifier.weight(1f),
                                colors = ButtonDefaults.buttonColors(
                                    containerColor = if (sel) AppColors.Gold.copy(0.25f) else AppColors.Gold.copy(0.08f),
                                    contentColor = if (sel) Color.White else AppColors.GoldLight
                                ),
                                border = BorderStroke(1.dp, if (sel) AppColors.Gold else AppColors.Gold.copy(0.2f)),
                                shape = CircleShape
                            ) {
                                Text("$$amount", fontWeight = FontWeight.Bold)
                            }
                        }
                    }
                    Text("Or give any amount directly in Cash App", fontSize = 12.sp, color = AppColors.TextMuted)
                }
            }

            // Cash App card
            GlassCard(modifier = Modifier.fillMaxWidth()) {
                Column(modifier = Modifier.padding(20.dp), verticalArrangement = Arrangement.spacedBy(16.dp)) {
                    Row(verticalAlignment = Alignment.CenterVertically, horizontalArrangement = Arrangement.spacedBy(14.dp)) {
                        Box(modifier = Modifier.size(56.dp).clip(RoundedCornerShape(14.dp))
                            .background(Brush.linearGradient(listOf(AppColors.CashGreen, AppColors.CashGreenDark))),
                            contentAlignment = Alignment.Center) {
                            Text("$", fontSize = 24.sp, fontWeight = FontWeight.Black, color = Color.White)
                        }
                        Column {
                            Text("PASTOR'S CASH APP", fontSize = 10.sp, letterSpacing = 2.sp, color = AppColors.TextMuted)
                            Text(church.cashAppTag, fontSize = 22.sp, fontWeight = FontWeight.Bold, color = AppColors.GoldLight)
                        }
                    }

                    Button(
                        onClick = {
                            val intent = Intent(Intent.ACTION_VIEW, Uri.parse(church.cashAppUrl))
                            context.startActivity(intent)
                        },
                        modifier = Modifier.fillMaxWidth(),
                        colors = ButtonDefaults.buttonColors(containerColor = Color.Transparent),
                        contentPadding = PaddingValues(0.dp),
                        shape = RoundedCornerShape(14.dp)
                    ) {
                        Row(
                            modifier = Modifier.fillMaxWidth()
                                .clip(RoundedCornerShape(14.dp))
                                .background(Brush.horizontalGradient(listOf(AppColors.CashGreen, AppColors.CashGreenDark)))
                                .padding(16.dp),
                            horizontalArrangement = Arrangement.Center
                        ) {
                            Text("Open Cash App  →", color = Color.White, fontWeight = FontWeight.Bold, fontSize = 16.sp)
                        }
                    }

                    Text("🔒 Secure payment via Cash App  •  ${church.cashAppTag}",
                        fontSize = 12.sp, color = AppColors.TextMuted, textAlign = TextAlign.Center,
                        modifier = Modifier.fillMaxWidth())
                }
            }

            ScriptureCard(
                "Each of you should give what you have decided in your heart to give, not reluctantly or under compulsion, for God loves a cheerful giver.",
                "2 Corinthians 9:7"
            )

            Spacer(Modifier.height(16.dp))
        }
    }
}
