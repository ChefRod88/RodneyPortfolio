package com.rcdev.nbmbc.ui.screens

import androidx.compose.foundation.*
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.shape.*
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.graphics.*
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.unit.*
import com.rcdev.nbmbc.data.prayerCategories
import com.rcdev.nbmbc.ui.components.*
import com.rcdev.nbmbc.ui.theme.*

@Composable
fun PrayerScreen(modifier: Modifier = Modifier) {
    var name by remember { mutableStateOf("") }
    var email by remember { mutableStateOf("") }
    var request by remember { mutableStateOf("") }
    var selectedCategory by remember { mutableStateOf("General") }
    var anonymous by remember { mutableStateOf(false) }
    var submitted by remember { mutableStateOf(false) }
    var submitting by remember { mutableStateOf(false) }

    Column(modifier = modifier.fillMaxSize().verticalScroll(rememberScrollState())) {
        // Hero
        Box(modifier = Modifier.fillMaxWidth()
            .background(Brush.verticalGradient(listOf(AppColors.Surface2, AppColors.Background)))
            .padding(32.dp)) {
            Column(horizontalAlignment = Alignment.CenterHorizontally, verticalArrangement = Arrangement.spacedBy(14.dp)) {
                EyebrowBadge("We're Here For You")
                Text("Request Prayer", style = MaterialTheme.typography.displaySmall,
                    fontWeight = FontWeight.Black, brush = GoldTextGradient, textAlign = TextAlign.Center)
                Text("We would be honored to lift you up. Share your request and our prayer team will stand with you in faith.",
                    color = AppColors.TextMuted, textAlign = TextAlign.Center, lineHeight = 22.sp)
            }
        }

        if (submitted) {
            SuccessView()
        } else {
            Column(modifier = Modifier.padding(16.dp), verticalArrangement = Arrangement.spacedBy(16.dp)) {
                // Form Card
                GlassCard(modifier = Modifier.fillMaxWidth()) {
                    Column(modifier = Modifier.padding(20.dp), verticalArrangement = Arrangement.spacedBy(18.dp)) {

                        // Category chips
                        Column(verticalArrangement = Arrangement.spacedBy(8.dp)) {
                            Text("PRAYER CATEGORY", fontSize = 11.sp, letterSpacing = 2.sp,
                                color = AppColors.Gold, fontWeight = FontWeight.SemiBold)
                            FlowRow(horizontalArrangement = Arrangement.spacedBy(8.dp),
                                verticalArrangement = Arrangement.spacedBy(8.dp)) {
                                prayerCategories.forEach { cat ->
                                    val sel = selectedCategory == cat.label
                                    FilterChip(
                                        selected = sel,
                                        onClick = { selectedCategory = cat.label },
                                        label = { Text("${cat.emoji} ${cat.label}", fontSize = 13.sp) },
                                        colors = FilterChipDefaults.filterChipColors(
                                            selectedContainerColor = AppColors.Gold.copy(0.22f),
                                            selectedLabelColor = Color.White,
                                            containerColor = AppColors.Gold.copy(0.06f),
                                            labelColor = AppColors.GoldLight
                                        ),
                                        border = FilterChipDefaults.filterChipBorder(
                                            enabled = true, selected = sel,
                                            selectedBorderColor = AppColors.Gold,
                                            borderColor = AppColors.Gold.copy(0.2f)
                                        )
                                    )
                                }
                            }
                        }

                        PrayerField("YOUR NAME (OPTIONAL)") {
                            OutlinedTextField(value = name, onValueChange = { name = it },
                                placeholder = { Text("Your name", color = AppColors.TextMuted) },
                                modifier = Modifier.fillMaxWidth(),
                                colors = prayerTextFieldColors(),
                                shape = RoundedCornerShape(12.dp)
                            )
                        }

                        PrayerField("EMAIL (OPTIONAL)") {
                            OutlinedTextField(value = email, onValueChange = { email = it },
                                placeholder = { Text("your@email.com", color = AppColors.TextMuted) },
                                modifier = Modifier.fillMaxWidth(),
                                colors = prayerTextFieldColors(),
                                shape = RoundedCornerShape(12.dp)
                            )
                        }

                        PrayerField("YOUR PRAYER REQUEST") {
                            OutlinedTextField(
                                value = request, onValueChange = { if (it.length <= 1000) request = it },
                                placeholder = { Text("Share your heart with us...", color = AppColors.TextMuted) },
                                modifier = Modifier.fillMaxWidth().heightIn(min = 120.dp),
                                maxLines = 10,
                                colors = prayerTextFieldColors(),
                                shape = RoundedCornerShape(12.dp)
                            )
                            Text("${request.length} / 1000", fontSize = 11.sp, color = AppColors.TextMuted,
                                modifier = Modifier.fillMaxWidth(), textAlign = TextAlign.End)
                        }

                        Row(verticalAlignment = Alignment.CenterVertically, horizontalArrangement = Arrangement.spacedBy(10.dp)) {
                            Switch(checked = anonymous, onCheckedChange = { anonymous = it },
                                colors = SwitchDefaults.colors(checkedThumbColor = AppColors.Background, checkedTrackColor = AppColors.Gold))
                            Text("Share my request anonymously with the congregation",
                                fontSize = 14.sp, color = AppColors.TextMuted, lineHeight = 20.sp)
                        }

                        GoldButton(
                            text = if (submitting) "Submitting..." else "Submit Prayer Request",
                            onClick = {
                                if (request.isNotBlank() && !submitting) {
                                    submitting = true
                                    submitted = true
                                }
                            },
                            fullWidth = true
                        )
                    }
                }

                // Scripture sidebar cards
                ScriptureCard("Do not be anxious about anything, but in every situation, by prayer and petition, with thanksgiving, present your requests to God.", "Philippians 4:6")
                ScriptureCard("The prayer of a righteous person is powerful and effective.", "James 5:16")

                GlassCard(modifier = Modifier.fillMaxWidth()) {
                    Column(modifier = Modifier.padding(18.dp), verticalArrangement = Arrangement.spacedBy(6.dp)) {
                        Text("OUR PROMISE", fontSize = 10.sp, letterSpacing = 2.sp, color = AppColors.Gold, fontWeight = FontWeight.SemiBold)
                        Text("You Are Not Alone", fontWeight = FontWeight.Bold, color = Color.White, fontSize = 17.sp)
                        Text("Our prayer team receives every request and lifts each need before the Lord with faith, sincerity, and love.",
                            fontSize = 14.sp, color = AppColors.TextMuted, lineHeight = 20.sp)
                    }
                }

                Spacer(Modifier.height(8.dp))
            }
        }
    }
}

@Composable
private fun PrayerField(label: String, content: @Composable ColumnScope.() -> Unit) {
    Column(verticalArrangement = Arrangement.spacedBy(6.dp)) {
        Text(label, fontSize = 11.sp, letterSpacing = 2.sp, color = AppColors.Gold, fontWeight = FontWeight.SemiBold)
        content()
    }
}

@Composable
private fun prayerTextFieldColors() = OutlinedTextFieldDefaults.colors(
    focusedBorderColor = AppColors.Gold,
    unfocusedBorderColor = AppColors.Gold.copy(0.2f),
    focusedTextColor = AppColors.TextPrimary,
    unfocusedTextColor = AppColors.TextPrimary,
    cursorColor = AppColors.Gold,
    focusedContainerColor = Color.White.copy(0.04f),
    unfocusedContainerColor = Color.White.copy(0.04f)
)

@Composable
private fun SuccessView() {
    Column(
        modifier = Modifier.fillMaxWidth().padding(48.dp),
        horizontalAlignment = Alignment.CenterHorizontally,
        verticalArrangement = Arrangement.spacedBy(20.dp)
    ) {
        Box(modifier = Modifier.size(100.dp).clip(CircleShape)
            .background(GoldGradient), contentAlignment = Alignment.Center) {
            Text("🙏", fontSize = 44.sp)
        }
        Text("Thank You", style = MaterialTheme.typography.headlineMedium,
            fontWeight = FontWeight.Black, brush = GoldTextGradient, textAlign = TextAlign.Center)
        Text("We're praying for you. Our prayer team has received your request and will be lifting you up before the Lord.",
            color = AppColors.TextMuted, textAlign = TextAlign.Center, lineHeight = 22.sp)
    }
}
