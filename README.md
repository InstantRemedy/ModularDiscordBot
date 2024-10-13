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