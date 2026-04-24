// Quote form (Pages/Index)
async function handleQuoteSubmit(e) {
  e.preventDefault();
  const form = document.getElementById("quoteForm");
  const errorBox = document.getElementById("quoteError");
  const submitBtn = form.querySelector('button[type="submit"]');
  const formFields = form.querySelectorAll("input,select,textarea,button[type=submit]");

  errorBox.style.display = "none";
  errorBox.textContent = "";
  submitBtn.disabled = true;

  try {
    const response = await fetch(form.action, {
      method: "POST",
      body: new FormData(form),
      headers: { "X-Requested-With": "XMLHttpRequest" },
    });

    if (!response.ok) {
      throw new Error("submit-failed");
    }

    formFields.forEach((el) => (el.style.display = "none"));
    document.getElementById("quoteSuccess").style.display = "block";
  } catch (err) {
    errorBox.textContent =
      "Unable to submit right now. Please email rodney@globalrcdev.com directly.";
    errorBox.style.display = "block";
    submitBtn.disabled = false;
  }
}
