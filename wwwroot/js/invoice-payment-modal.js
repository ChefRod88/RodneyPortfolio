// Standalone payment modal (Pages/Portal/PaymentModal partial)
let stripeInstance = null;
let stripeElements = null;
let stripeCardElement = null;
let currentInvoiceId = null;
let currentAmount = 0;

function openPaymentModal(invoiceId, amount, invoiceNumber) {
  currentInvoiceId = invoiceId;
  currentAmount = amount;

  document.getElementById("pm-amount-display").textContent = "$" + parseFloat(amount).toFixed(2);
  document.getElementById("pm-invoice-label").textContent = "Invoice #" + invoiceNumber;
  document.getElementById("cashapp-amount").textContent = "$" + parseFloat(amount).toFixed(2);
  document.getElementById("paymentModal").style.display = "flex";

  initStripe();
  switchTab("stripe");
}

function closePaymentModal() {
  document.getElementById("paymentModal").style.display = "none";
  stripeCardElement = null;
  stripeElements = null;
}

function switchTab(tab) {
  document.querySelectorAll(".pm-tab").forEach((t) => t.classList.remove("active"));
  document.querySelectorAll(".pm-panel").forEach((p) => (p.style.display = "none"));
  document.getElementById("tab-" + tab).classList.add("active");
  document.getElementById("panel-" + tab).style.display = "block";
  if (tab === "stripe") initStripe();
}

function initStripe() {
  if (stripeCardElement) return;
  const pubKey = document.getElementById("stripe-pub-key")?.value;
  if (!pubKey || !window.Stripe) return;

  stripeInstance = window.Stripe(pubKey);
  stripeElements = stripeInstance.elements();
  stripeCardElement = stripeElements.create("card", {
    style: {
      base: {
        color: "#e0e0e0",
        fontFamily: "Courier New, monospace",
        fontSize: "15px",
        "::placeholder": { color: "#555" },
      },
      invalid: { color: "#ff4444" },
    },
  });
  stripeCardElement.mount("#stripe-card-element");
  stripeCardElement.on("change", (e) => {
    document.getElementById("stripe-card-errors").textContent = e.error ? e.error.message : "";
  });
}

async function handleStripePayment() {
  if (!stripeCardElement) return;
  const btn = document.getElementById("stripe-pay-btn");
  const btnText = document.getElementById("stripe-btn-text");
  const spinner = document.getElementById("stripe-spinner");

  btn.disabled = true;
  btnText.textContent = "Processing...";
  spinner.style.display = "inline-block";

  try {
    const res = await fetch("/Portal/CreatePaymentIntent", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ invoiceId: currentInvoiceId }),
    });
    const data = await res.json();

    if (!res.ok) throw new Error(data.error || "Could not initiate payment.");

    const { error, paymentIntent } = await stripeInstance.confirmCardPayment(data.clientSecret, {
      payment_method: { card: stripeCardElement },
    });

    if (error) throw new Error(error.message);

    await fetch("/Portal/ConfirmPayment", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ invoiceId: currentInvoiceId, paymentIntentId: paymentIntent.id }),
    });

    closePaymentModal();
    showPaymentSuccess("stripe");
    setTimeout(() => location.reload(), 2500);
  } catch (err) {
    document.getElementById("stripe-card-errors").textContent = err.message;
    btn.disabled = false;
    btnText.textContent = "Pay Now";
    spinner.style.display = "none";
  }
}

async function handleCashAppConfirm() {
  await fetch("/Portal/CashAppPending", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ invoiceId: currentInvoiceId }),
  });
  closePaymentModal();
  showPaymentSuccess("cashapp");
  setTimeout(() => location.reload(), 3000);
}

function showPaymentSuccess(method) {
  const msg =
    method === "stripe"
      ? "✅ Payment successful! Your receipt is on its way."
      : "📱 Thanks! We'll confirm your Cash App payment shortly.";
  const toast = document.createElement("div");
  toast.style.cssText =
    "position:fixed;bottom:24px;right:24px;background:#0d1117;border:1px solid #00ff4150;color:#00ff41;padding:16px 22px;border-radius:10px;font-family:Courier New,monospace;font-size:0.85rem;z-index:99999;box-shadow:0 4px 30px #00ff4120;animation:pm-fade-in 0.3s ease;max-width:300px;line-height:1.5;";
  toast.textContent = msg;
  document.body.appendChild(toast);
  setTimeout(() => toast.remove(), 3000);
}
