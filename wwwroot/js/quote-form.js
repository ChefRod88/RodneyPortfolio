// Quote form (Pages/Index)
async function handleQuoteSubmit(e) {
  e.preventDefault();
  const form = document.getElementById("quoteForm");
  const errorBox = document.getElementById("quoteError");
  const submitBtn = form.querySelector('button[type="submit"]');
  const formFields = form.querySelectorAll(".rc-form-row,.rc-form-group,button[type=submit]");
  const turnstileResponse =
    typeof turnstile !== "undefined" ? turnstile.getResponse() : "";

  errorBox.style.display = "none";
  errorBox.textContent = "";

  if (!turnstileResponse) {
    errorBox.textContent = "Please complete the CAPTCHA before submitting.";
    errorBox.style.display = "block";
    return;
  }

  submitBtn.disabled = true;

  try {
    const response = await fetch(form.action, {
      method: "POST",
      body: new FormData(form),
      headers: { "X-Requested-With": "XMLHttpRequest" },
    });

    if (!response.ok) {
      let message = "Unable to submit right now. Please try again.";
      try {
        const payload = await response.json();
        if (payload?.message) {
          message = payload.message;
        }
      } catch {
        // Keep generic message if response body is not JSON.
      }

      throw new Error(message);
    }

    formFields.forEach((el) => (el.style.display = "none"));
    document.getElementById("quoteSuccess").style.display = "block";
  } catch (err) {
    errorBox.textContent =
      err instanceof Error && err.message
        ? err.message
        : "Unable to submit right now. Please email rodney@globalrcdev.com directly.";
    errorBox.style.display = "block";
    submitBtn.disabled = false;
    if (typeof turnstile !== "undefined") {
      turnstile.reset();
    }
  }
}
