// Support form (Pages/Support)
async function handleSupportSubmit(e) {
  e.preventDefault();
  const form = document.getElementById("supportForm");
  const errorBox = document.getElementById("supportError");
  const successBox = document.getElementById("supportSuccess");
  const submitBtn = form.querySelector('button[type="submit"]');
  const formFields = form.querySelectorAll(".rc-form-row,.rc-form-group,button[type=submit]");
  const turnstileResponse =
    typeof turnstile !== "undefined" ? turnstile.getResponse() : "";

  errorBox.hidden = true;
  errorBox.textContent = "";
  successBox.hidden = true;

  if (!turnstileResponse) {
    errorBox.textContent = "Please complete the CAPTCHA before submitting.";
    errorBox.hidden = false;
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
    document.querySelector(".support-form .cf-turnstile")?.closest(".rc-form-group")?.remove();
    successBox.hidden = false;
  } catch (err) {
    errorBox.textContent =
      err instanceof Error && err.message
        ? err.message
        : "Unable to submit right now. Please email rodney@globalrcdev.com directly.";
    errorBox.hidden = false;
    submitBtn.disabled = false;
    if (typeof turnstile !== "undefined") {
      turnstile.reset();
    }
  }
}
