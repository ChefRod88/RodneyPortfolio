// Portal dashboard: tabs + payment modal (Views/Portal/Dashboard)
const paymentModalEl = document.getElementById("paymentModal");
const stripePk = paymentModalEl?.dataset.stripePk ?? "";
const stripe = window.Stripe ? window.Stripe(stripePk) : null;
let stripeElements = null;
let currentInvoiceId = null;

function showTab(name, btn) {
  document.querySelectorAll(".portal-tab-content").forEach((t) => (t.style.display = "none"));
  document.querySelectorAll(".portal-tab").forEach((t) => t.classList.remove("portal-tab-active"));
  document.getElementById("tab-" + name).style.display = "block";
  btn.classList.add("portal-tab-active");
}

function openPaymentModal(invoiceId, amount, invoiceNum, desc) {
  currentInvoiceId = invoiceId;
  document.getElementById("pm-invoice-label").textContent =
    "Invoice #" + invoiceNum + (desc ? " — " + desc : "");
  document.getElementById("pm-amount-display").textContent = "$" + parseFloat(amount).toFixed(2);
  document.getElementById("pm-cashapp-amount").textContent = "$" + parseFloat(amount).toFixed(2);
  document.getElementById("paymentModal").style.display = "flex";
  document.body.style.overflow = "hidden";
  resetStripePanel();
  switchPayTab("stripe");
  initStripeElements(invoiceId);
}

function closePaymentModal() {
  document.getElementById("paymentModal").style.display = "none";
  document.body.style.overflow = "";
  if (stripeElements) {
    document.getElementById("payment-element").innerHTML = "";
    stripeElements = null;
  }
}

function switchPayTab(tab) {
  document.querySelectorAll(".pm-tab").forEach((t) => t.classList.remove("pm-tab-active"));
  document.getElementById("pm-panel-stripe").style.display = "none";
  document.getElementById("pm-panel-cashapp").style.display = "none";
  document.getElementById("pm-tab-" + tab).classList.add("pm-tab-active");
  document.getElementById("pm-panel-" + tab).style.display = "block";
}

async function initStripeElements(invoiceId) {
  if (!stripe) {
    showPmError("Payment is unavailable. Please refresh the page.");
    return;
  }
  try {
    const csrf =
      document.querySelector("input[name='__RequestVerificationToken']")?.value ?? "";
    const resp = await fetch("/Portal/Dashboard/CreatePaymentIntent", {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        RequestVerificationToken: csrf,
      },
      body: JSON.stringify({ invoiceId }),
    });
    const data = await resp.json();
    if (!data.clientSecret) {
      showPmError("Unable to initialize payment. Please try again.");
      return;
    }
    stripeElements = stripe.elements({
      clientSecret: data.clientSecret,
      appearance: {
        theme: "night",
        variables: {
          colorPrimary: "#00d4ff",
          colorBackground: "#0d1117",
          colorText: "#e8f4f8",
          fontFamily: "Rajdhani, sans-serif",
          borderRadius: "6px",
        },
      },
    });
    stripeElements.create("payment").mount("#payment-element");
  } catch (err) {
    showPmError("Payment setup failed. Please refresh and try again.");
  }
}

async function submitStripePayment() {
  if (!stripe || !stripeElements) return;
  const btn = document.getElementById("pm-stripe-btn");
  btn.disabled = true;
  document.getElementById("pm-processing").style.display = "flex";
  document.getElementById("pm-stripe-error").textContent = "";

  const { error, paymentIntent } = await stripe.confirmPayment({
    elements: stripeElements,
    redirect: "if_required",
  });

  if (error) {
    showPmError(error.message);
    btn.disabled = false;
    document.getElementById("pm-processing").style.display = "none";
    return;
  }

  const csrf =
    document.querySelector("input[name='__RequestVerificationToken']")?.value ?? "";
  await fetch("/Portal/Dashboard/ConfirmPayment", {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      RequestVerificationToken: csrf,
    },
    body: JSON.stringify({ invoiceId: currentInvoiceId, paymentIntentId: paymentIntent.id }),
  });

  document.getElementById("pm-processing").style.display = "none";
  document.getElementById("pm-success").style.display = "block";
  btn.style.display = "none";
  setTimeout(() => { closePaymentModal(); location.reload(); }, 2500);
}

async function submitCashApp() {
  const csrf =
    document.querySelector("input[name='__RequestVerificationToken']")?.value ?? "";
  await fetch("/Portal/Dashboard/CashAppPending", {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      RequestVerificationToken: csrf,
    },
    body: JSON.stringify({ invoiceId: currentInvoiceId }),
  });
  closePaymentModal();
  showToast("📱 Thanks! We'll confirm your Cash App payment shortly.");
  setTimeout(() => location.reload(), 3000);
}

function resetStripePanel() {
  const btn = document.getElementById("pm-stripe-btn");
  btn.disabled = false;
  btn.style.display = "block";
  document.getElementById("pm-processing").style.display = "none";
  document.getElementById("pm-success").style.display = "none";
  document.getElementById("pm-stripe-error").textContent = "";
  document.getElementById("payment-element").innerHTML = "";
  stripeElements = null;
}

function showPmError(msg) {
  document.getElementById("pm-stripe-error").textContent = msg;
}

function showToast(msg) {
  const t = document.createElement("div");
  t.style.cssText =
    "position:fixed;bottom:24px;right:24px;background:#0d1117;border:1px solid rgba(0,212,255,0.3);color:#00d4ff;padding:14px 20px;border-radius:10px;font-family:monospace;font-size:0.82rem;z-index:99999;box-shadow:0 4px 30px rgba(0,212,255,0.1);max-width:300px;line-height:1.5;";
  t.textContent = msg;
  document.body.appendChild(t);
  setTimeout(() => t.remove(), 3500);
}
