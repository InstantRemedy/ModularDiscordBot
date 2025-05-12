FROM ubuntu:24.04

# Create a new user
RUN useradd -d /opt/discord-bot -m -s /bin/bash discord-bot

# Change to the new user
USER discord-bot

# Set the working directory
WORKDIR /app

# Copy the Lavalink jar
COPY ./app /app

# Run the jar
CMD ["/app/ModularDiscordBot"]