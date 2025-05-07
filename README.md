# SPOT: Smart Playlists On Transformers 🎵🤖

SPOT is a Discord bot that generates AI-curated Spotify playlists based on user-inputted vibes or moods. Whether you're hanging out in a study session, gaming late into the night, or just want a fresh sound, SPOT uses the power of OpenAI and Spotify to recommend tracks that match your moment — all with a single command.

---

## 🚀 Why It Exists

In Discord calls, music can quickly get stale. Reusing the same playlist becomes repetitive, and discovering new music that fits a specific mood can feel like a chore. Sometimes you know *exactly* what vibe you're going for — “rainy day jazz” or “summer sunset drive” — but you can't quite build the right playlist.

SPOT solves that.

With one command like `/vibe summer beach`, SPOT turns natural language into a personalized playlist, making it easy to set the mood instantly.

---

## 🧠 How It Works (System Architecture)

```
[ Discord User ]
       ⬇️   /vibe "chill sunset"
[ VibeModule.cs ]
       ⬇️   → RequestModel.cs
[ CommandInterface.cs ]
       ⬇️
[ OpenAIService.cs ]  → (Query expansion using GPT)
       ⬇️
[ SpotifyService.cs ] → (Searches & curates track links)
       ⬇️
[ CommandInterface.cs ]
       ⬇️
[ Discord Bot Replies ] → Returns playlist / track links
```

- **Discord Layer**  
  - `VibeModule.cs`: Handles user interaction via the `/vibe` slash command.
  - `RequestModel.cs`: Carries the user query and username.

- **Core Services Layer**  
  - `CommandInterface.cs`: Orchestrates the flow between AI and Spotify services.
  - `OpenAIService.cs`: Expands user input into a refined mood or list of songs using GPT.
  - `SpotifyService.cs`: Searches Spotify for matching songs and formats results.

- **Runtime Layer**  
  - `BotService.cs`: Manages the Discord client and command registration.
  - `Program.cs`: The entrypoint that launches the bot.

---

## 💡 Features

- 🎧 Natural-language music vibes (e.g., “lofi horror soundtrack”)
- 🤖 GPT-powered prompt expansion for genre and mood discovery
- 🎵 Spotify integration to fetch actual songs and links
- 🔒 Secure token management for OpenAI and Spotify
- ⚙️ Modular structure for future web or CLI extensions

---

## 🛠️ Requirements

- .NET 6+
- OpenAI API Key (`OPENAI_API_KEY`)
- Spotify Client ID/Secret (`SPOTIFY_CLIENT_ID`, `SPOTIFY_CLIENT_SECRET`, and `SPOTIFY_REDIRECT_URL`)
- Discord Bot Token (`DISCORD_TOKEN`)

---

## 🗣️ Example Usage

```bash
/vibe moody midnight walk
```

**Bot Reply:**

```
Here's your expanded vibe: "moody midnight walk"
- Nightcall - Kavinsky
- Retrograde - James Blake
- Oblivion - Grimes
...
```

---

## 📦 Future Improvements

- ✅ OAuth-based Spotify playlist creation
- 🌍 Add locale-specific recommendations
- 📱 Add a web frontend for vibe generation outside of Discord
- 🧠 Fine-tuning GPT with custom music data

---

## 🤝 Contributing

Pull requests and issues are welcome! This project was created as part of a **Concepts of Programming Languages** final, but we’d love to see where else it can go.

---

## 📃 License

MIT License.
