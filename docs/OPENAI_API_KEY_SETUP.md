# OpenAI API Key Setup (Safe Steps)

Your project is now configured to use **User Secrets** for the OpenAI API key. The key is stored only on your machine and is never committed to git.

---

## ⚠️ IMPORTANT: Revoke Your Exposed Key First

If you shared your API key anywhere (chat, email, etc.), **revoke it immediately**:

1. Go to [platform.openai.com/api-keys](https://platform.openai.com/api-keys)
2. Find the key you shared
3. Click the trash icon to **revoke/delete** it
4. Create a **new** key (you'll use this new one below)

---

## Step 1: Open Terminal in Your Project Folder

```bash
cd /Users/rodneyamoschery/dev/RodneyPortfolio
```

---

## Step 2: Store Your API Key (Use Your NEW Key)

Replace `YOUR_NEW_KEY_HERE` with your new key from OpenAI:

```bash
dotnet user-secrets set "OpenAI:ApiKey" "YOUR_NEW_KEY_HERE"
```

---

## Step 3: Turn Off Demo Mode

So the chatbot uses the real OpenAI API instead of canned responses:

```bash
dotnet user-secrets set "OpenAI:UseDemoMode" "false"
```

---

## Step 4: Verify It Worked

```bash
dotnet user-secrets list
```

You should see:
- `OpenAI:ApiKey` = (your key, partially hidden)
- `OpenAI:UseDemoMode` = False

---

## Step 5: Run the App

```bash
dotnet run
```

Open the chatbot and ask a question. It should now use the real OpenAI API.

---

## Where Are Secrets Stored?

- **Mac/Linux:** `~/.microsoft/usersecrets/a1b2c3d4-e5f6-4a7b-9c8d-1e2f3a4b5c6d/secrets.json`
- **Windows:** `%APPDATA%\Microsoft\UserSecrets\a1b2c3d4-e5f6-4a7b-9c8d-1e2f3a4b5c6d\secrets.json`

This file is **never** committed to git.

---

## For Production (Azure Web App)

Your GitHub Actions workflow injects the API key during deployment. Add it to **GitHub Secrets**:

1. Go to your repo: [github.com/ChefRod88/RodneyPortfolio](https://github.com/ChefRod88/RodneyPortfolio)
2. Click **Settings** → **Secrets and variables** → **Actions**
3. Click **New repository secret**
4. Name: `OPENAI_API_KEY`
5. Value: paste your OpenAI API key
6. Click **Add secret**

On the next push to `main`, the workflow will deploy with the key. The chatbot will use the real OpenAI API in production.

**Important:** Use a fresh key. If you shared your key anywhere, revoke it at [platform.openai.com/api-keys](https://platform.openai.com/api-keys) and create a new one before adding to GitHub Secrets.
