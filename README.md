# Modular discord bot

---

## Description

Bot for managing discord server ss13(Rockhill)

## Functions

### AI

#### Interactions (using with slash commands '/')
- [openai_mode]: Set the mode for the OpenAI(steam/no_stream).
- [openai_add_allowed_role]: Add a role to the allowed roles.
- [openai_remove_allowed_role]: Remove a role to the allowed roles.
- [openai_max_requests]: Set the max requests for the OpenAI.
- [openai_reset_requests]: Reset the request amount for the OpenAI.
- [openai_enable]: Enable the OpenAI.
- [openai_disable]: Disable the OpenAI.
- [openai_new_thread]: Reassign the OpenAI thread

#### Commands (using with prefix (setup in config/bot.json))

- [mind]: queries llm and sends a response.


### Round status

#### Interactions (using with slash commands '/')
- [rs_host]: Set the host for the round status.
- [rs_port]: Set the port for the round status.
- [rs_add_allowed_role]: Add a role that can access the round status.
- [rs_remove_allowed_role]: Remove a role that can access the round status.


### Installation

Download the latest release from the [releases page](https://github.com/InstantRemedy/ModularDiscordBot/releases/tag/v2.1.0)

#### Windows

1. Unzip the win-x64 folder
2. Open config folder and edit json config files
3. Open the bot folder and run the ModularDiscordBot.exe file


#### Linux (Ubuntu 24.04 LTS)

##### Native
1. Unzip the linux-x64 directory
```bash
# using 7z
sudo apt install p7zip-full # if not installed
7z x ModularDiscordBot.7z
```

2. Open the config folder and edit json config files
```bash
cd ModularDiscordBot/config
```

3. Edit the config files
```bash
# nano, vim, vi, etc.
nano bot.json
nano opopen-ai.json
nano round-status.json
cd ..
```

4. Copy the config files to app directory
```bash
cp -r config ./app
```

5. cd to the app directory
```bash
cd app
```

6. Change the chmod of the app directory
```bash
chmod +x ModularDiscordBot
```

7. Run the bot
```bash
./ModularDiscordBot
```

##### Docker

1. Unzip the linux-x64 directory
```bash
# using 7z
sudo apt install p7zip-full # if not installed
7z x ModularDiscordBot.7z
```

2. Open the config folder and edit json config files
```bash
cd ModularDiscordBot
```

3. Edit the config files
```bash
# nano, vim, vi, etc.
nano config/bot.json
nano config/open-ai.json
nano config/round-status.json
```


4. All directories and files are already located for docker, just run 
```
docker-compose up. # -d for detached mode
```
